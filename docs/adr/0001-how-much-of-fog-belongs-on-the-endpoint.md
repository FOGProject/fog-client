# 0001 — How much of FOG belongs on the endpoint

## Status

**Open. This ADR deliberately carries no recommendation.**

It exists to make one decision decidable by laying out both futures with the
same rigour, because the choice governs everything downstream:
[ADR-0002](0002-runtime-if-a-compiled-agent-survives.md) is conditional on it,
while [ADR-0003](0003-client-identity-in-the-application-layer.md) and
[ADR-0004](0004-the-client-api-is-described-in-fogs-own-openapi-document.md) are
independent of it and can be settled first. The FOG maintainers settle this one
in review.

What this ADR *does* decide is narrower and, I think, uncontroversial: that
agentless **push** is rejected in both futures, and that a "thin agent" middle
ground is not the compromise it appears to be.

## Context

The question came up as "clientless would be amazing but I doubt we can pull
that off." That instinct is right, and the word is wrong — which is most of what
this document is about.

Start with the number that reframes the whole question:

```sh
# the ten features
find Modules -name '*.cs' -not -name '*.Designer.cs' | xargs cat | wc -l
# 3013

# the machinery that exists to ship them as a self-updating compiled binary
find UniversalInstaller SetupHelper UpdateHelper UpdateWaiter \
     ../zazzles/Zazzles/Modules/Updater ../zazzles/Zazzles/Bus \
     -name '*.cs' | xargs cat | wc -l
# 5658
```

**Nearly twice as much code exists to let the agent be a self-updating compiled
binary with its own IPC bus as exists to do the work.** Out of 17,526 total lines
across `fog-client` and `zazzles`, roughly 3,000 are the product. That ratio is
the argument, and every other observation in this ADR is downstream of it.

The surrounding facts: no functional commit since March 2023 (v0.13.0), while the
server it talks to shipped PKI work this year. Everything targets .NET Framework
4.5.2 and runs under Mono on Linux — Mono having been donated to WineHQ in 2024.
Both CI pipelines invoke `xbuild`, which modern Mono removed, on a Travis free
tier that ended around 2021. Neither has plausibly run in years.

## What is already settled, so the debate is about the real remainder

Before choosing a future, it is worth being precise about which of the ten
modules actually need a resident FOG-authored process. The answer is **one**, and
that one is already broken.

| Module | Deploy-time (offline) | Scheduled script | Verdict |
|---|---|---|---|
| HostnameChanger — rename | already implemented in FOS | trivial | **agentless today** |
| HostnameChanger — AD join | ODJ blob or unattend creds | `Add-Computer` | agentless, big caveat |
| HostnameChanger — product key | unattend `<ProductKey>` | `slmgr` (already a shell-out) | agentless |
| TaskReboot | n/a | 1-minute task | agentless (pull) |
| SnapinClient | first run only | yes | agentless (pull) |
| PrinterManager | initial set only | `Add-Printer`/`lpadmin` | agentless (pull) |
| DefaultPrinterManager | no — per-user | per-user logon task | agentless (pull) |
| PowerManagement | no | *the native scheduler is the feature* | agentless — and better |
| UserTracker | no | yes (it already polls) | agentless (pull) |
| AutoLogOut | no | Windows yes, Wayland no | **needs endpoint code** |
| DisplayManager | no | `Add-Type` P/Invoke | should be deleted |
| ClientUpdater | n/a | ceases to exist | eliminated |

Five observations that carry the table:

**Deploy-time is not hypothetical — FOG already ships it.**
`funcs.sh:changeHostname()` mounts the deployed NTFS volume with `ntfs-3g` and
rewrites twenty `ComputerName`/`Hostname` registry values through `reged`. It is
called from `completeTasking()`, gated on the `hostearly` flag that
`bootmenu.class.php` puts on the kernel command line. **The rename module is
already agentless in production**; the C# path is a fallback for hosts renamed
after deploy. `/images/postdownloadscripts/fog.postdownload` is an existing,
documented, sourced hook, and FOS already carries `chntpw`, `ntfs-3g`,
`cifs-utils` and libcurl. FOS is a root shell on the target disk with the server
one HTTP call away.

**PowerManagement should be surrendered, not ported.** It embeds Quartz.NET to
evaluate cron expressions. Task Scheduler and systemd timers *are* cron. The
native path is strictly more reliable, because it survives the service crashing
and it works when the FOG server is down. `ShutdownGUI` — 337 lines plus twelve
translations (cs, de, es, eu, fr, hu, it, nl, no, pl, pt, ro) — is replaced by
the countdown `shutdown /c` gives for free, which `shutdown /a` aborts.

**UserTracker is not event-driven and never was.** `UserTracker.cs` diffs
`User.AllLoggedIn()` against the previous poll. A scheduled script runs the
identical algorithm. Task Scheduler's native `LogonTrigger` and systemd user
units are *better* than what ships today.

**Scheduled scripts are faster than the agent.** The server returns
`'sleep' => $checkin + mt_rand(1, 91)` — 60 seconds plus up to 91 of jitter, so a
worst case around two and a half minutes. Task Scheduler's real floor is one
minute (`schtasks /sc minute /mo 1`; the five-minute limit is a GUI constraint,
not an engine one). **"We need a resident daemon for responsiveness" is false
against the current baseline** and must not be allowed into the argument.

**`LinuxHostName.cs` is the reductio** — 172 Mono-dependent lines, containing a
function literally named `BruteForce()`, to write `/etc/hostname`,
`/etc/HOSTNAME`, `/etc/hosts` and `/etc/sysconfig/network`. `hostnamectl
set-hostname` is one line. Multiply by every module and you have the Linux half
of the client.

The one genuine holdout is **AutoLogOut**, and it deserves an honest note rather
than load-bearing status: idle detection needs session-scoped `GetLastInputInfo`,
so it cannot run as SYSTEM. On Linux `xprintidle` is X11-only and **Wayland
exposes no idle-time API a third party can read** — `org.gnome.Mutter.IdleMonitor`
is GNOME-specific and gated. The README's compatibility matrix claims Linux
support that does not exist on any modern desktop. Scope this module to Windows
and say so; **do not let one module that is already broken on half the supported
platforms justify an architecture.**

`DisplayManager` should be deleted rather than ported: Windows-only, and it only
ever touches `GetDisplays()[0]`, which does not do the job it claims to in a world
of multi-head laptops and per-monitor DPI.

## Future A — deploy-time provisioning plus signed scripts on native schedulers

Provisioning (rename, AD join, product key, first printers, first software)
happens offline during the deploy, applied by FOS to a filesystem FOG already has
root on. Everything recurring becomes a signed script on Task Scheduler or a
systemd timer.

**What it buys.** It deletes roughly 99% of the C# and, with it, the entire
Windows-only build chain. The README currently requires Windows, WiX, .NET 4.6,
MSBuild 2015, Windows SDK 7, and a documented registry hack to stop the SDK
installer failing. That is why this client has almost no contributors. A
signed-script client is contributable-to by anyone who can read PowerShell —
which is every FOG admin. For a project whose client sat untouched for two and a
half years, that may be worth more than the 17,000 lines.

It also eliminates version skew as a category. A component fetched fresh on every
run cannot be out of date, so there is no updater, no update ordering problem, no
`UpdateWaiter`, no `SmartInstaller.exe`, and no "half my fleet is on an old
client" support thread.

**How script trust works, since this is the obvious objection.** Not Authenticode
plus `Set-ExecutionPolicy AllSigned` — Microsoft is explicit that execution policy
is not a security boundary, it is trivially bypassed by anyone already admin, and
it has no Linux analogue. Instead, verify a **detached signature over the script
body against FOG's existing CA before executing** — roughly thirty lines, and
identical in shape in PowerShell and in `openssl dgst -verify`. FOG already has
every primitive: `RSA.IsFromCA`, the `srvpublic.crt` endpoint, and a per-install
CA that recent server commits anchor in the server's own trust store.

The bootstrap that performs that verification is necessarily unsigned — it is the
root of trust. **This is where deploy-time pays off a second time:** the bootstrap
and the CA public key are planted offline by FOS, onto a disk FOG fully controls,
before the OS has ever booted. That is a strictly better anchor than today's
model, where the MSI is fetched over the network onto an already-running OS. The
strongest objection to Future A turns into an argument for it.

**What it costs, stated plainly.**

*Two implementations to keep in sync* — PowerShell and POSIX shell, with no
compiler catching drift. Mitigated but not erased by the fact that the platforms
already diverge heavily today: AD join and product key are Windows-only, CUPS is
Linux-only. The genuinely shared surface is fetch, verify, rename, snapin, report.

*AD join has no clean fully-agentless answer, and this is the one place
"clientless" costs security rather than gaining it.* Offline domain join is the
right shape, because `djoin /provision` yields a single-use, single-machine
computer-account credential rather than a reusable domain one. But **`djoin.exe`
is Windows-only and there is no supported way to mint ODJ blobs from a Linux
server**, which is what FOG runs on. The three options are a small Windows helper
box that mints blobs on request (best security, worst deployment story for a
two-person IT team); `<UnattendedJoin><Credentials>` with a delegated join-only
account, noting that `PlainText=false` is obfuscation rather than encryption and
that copies linger under `Panther\Unattend\`; or keeping AD join agent-side. The
pragmatic default is the second with a tightly-delegated account. Do not paper
over this.

*Deploy-time is provisioning, not configuration management.* It cannot reassign
printers when a machine moves labs, cannot revoke a snapin, cannot enforce
"FOG-managed printers only" against a user who added their own. Anything sold as
"this removes the need for ongoing management" is wrong — which is precisely why
Future A is deploy-time **plus** scheduled scripts, not deploy-time alone.

## Future B — keep a compiled agent

The agent stays an agent; [ADR-0002](0002-runtime-if-a-compiled-agent-survives.md)
then decides its runtime, and recommends the current .NET LTS. The case is that
17,526 lines encode a decade of Windows edge cases — session isolation, token
impersonation, domain-join error codes, printer driver quirks — that are cheap to
keep and expensive to re-derive, and that the port is far smaller than it looks
because only five language constructs actually block it.

The case against is the ratio at the top of this document: a port preserves the
5,658 lines of installer, updater and bus machinery along with the 3,013 lines
that do the work, and commits the project to maintaining a signed, versioned,
cross-platform binary on a build chain almost nobody can reproduce — for another
decade.

## Rejected in both futures: agentless push

Agentless *push* — the FOG server reaching out to endpoints over WinRM or SSH — is
rejected on three independent grounds, any one of which is sufficient. This
rejection does not depend on which future wins.

**1. Credential inversion.** Push requires the FOG server to store reusable
administrative credentials for every endpoint. The fair counter deserves stating
first: a compromised FOG server can already push a snapin and get SYSTEM, so the
server is already fleet-fatal. True. But the *shape* of the blast radius differs
categorically. Today's capability is pull-gated and non-portable — an attacker
gets execution only on hosts that voluntarily poll, only at poll cadence, only
while FOG is up and serving, and the capability evaporates the moment the server
is taken offline. It cannot be exfiltrated; you have to keep owning the server to
keep using it. A credential store is portable, reusable, copied out in one
request, usable from anywhere against machines that never checked in, and for AD
credentials it reaches every non-FOG asset in the domain. Rotating after a breach
means touching every endpoint. And consider where that store would live: a large
PHP application with a long CVE history, on a flat school VLAN, administered by
one or two people with no budget. It is close to the worst available home for a
domain admin credential.

**2. Reachability.** Push requires the server to address the endpoint. FOG's fleet
is DHCP workstations and laptops that move between wired and wireless and
sometimes go home. Pull works from anywhere the host can reach the server URL —
through NAT, over VPN, on a subnet the server cannot route back into. Push
silently stops working for exactly the machines an SMB most wants managed. There
is no fix; it is a property of the direction of the connection.

**3. The bootstrap circularity.** WinRM is off by default on client SKUs.
`winrm quickconfig` aborts if any network profile is classified **Public**, which
is what NLA misclassification routinely does to a lab machine. `schtasks /s`
against a non-domain-joined Windows box has a nastier version: **Remote UAC token
filtering** strips administrative rights from remote logons by every local admin
except the built-in RID-500 account, unless `LocalAccountTokenFilterPolicy=1` is
set in the registry — which requires local access or a GPO. So on FOG's flagship
scenario, a lab of non-domain-joined Windows machines, both headline push
mechanisms need a GPO or an agent to enable the thing that was supposed to
replace the agent.

Note the resolution, because it is the interesting part: **deploy-time is what
breaks that circle** — FOG lays down the image, so FOG can set those keys offline.
And once you have deploy-time, you no longer need push.

## The trap: a "thin agent" is a ratchet, not a compromise

The obvious middle ground is a small daemon that does only what genuinely needs a
resident process. It should be resisted, and the reason is mechanical rather than
aesthetic.

Because it is compiled, it needs an updater. Because updating a running service
requires a second process, it needs a helper. Because it has two processes, it
needs an IPC bus. Because the bus spans sessions, it needs a per-user counterpart.

That is not speculation about what might happen. It is the recorded history of
this repository: `UniversalInstaller`, `Updater`, `Bus`, `UserServiceSpawner` and
`UpdateWaiter` are the 5,658 lines at the top of this document, and they exist for
no other reason. **Any thin-agent proposal must explain why it will not regrow
them**, and "we'll keep it small" is not that explanation.

Two claims that get used to justify a thin agent, neither of which survives
contact with the current code: that a daemon is needed for responsiveness (it is
slower than a scheduled task today), and that a daemon is needed for a reverse
channel (FOG does not have one — building the thin agent to get one is adding a
feature, not preserving one).

## The reframe, which is this ADR's actual contribution

Nobody manages end-user workstations agentlessly. Every agentless tool — Ansible,
`salt-ssh` — targets servers: always on, statically addressed, sshd already
running, legitimately co-owned by whoever holds the credentials. Everything
targeting laptops and lab machines ships an agent, without exception: SCCM,
Intune, osquery, Zabbix, CrowdStrike, Tailscale. FOG's fleet is laptops and lab
machines.

But the more useful pattern is the second one: **the tools that claim "no agent"
mean "we use an agent somebody else maintains."** Ansible does not run
agentlessly — it runs on sshd and WinRM, daemons written and maintained by
OpenSSH and Microsoft. Intune is marketed as agentless and is an agent Microsoft
ships in the box. *"Agentless" is a claim about who bears the maintenance cost of
the endpoint component, not about whether one exists.*

That is exactly the trade available here, and it is the honest version of the
original question. **FOG's problem was never that code runs on the endpoint. It is
that FOG writes, builds, signs, ships, versions and supports that code, on a
toolchain almost nobody can reproduce.** Task Scheduler, systemd, PowerShell and
CUPS are agents FOG does not have to write. Roughly 3,000 of the client's 17,526
lines do the actual work; the question this ADR puts to the maintainers is
whether to ship those 3,000 lines as scripts and delete the platform built to
carry them, or to keep the platform and modernise it.

## References

- [ADR-0002](0002-runtime-if-a-compiled-agent-survives.md) — the runtime, conditional on Future B
- [ADR-0003](0003-client-identity-in-the-application-layer.md) — client identity, independent of this decision
- `FOGProject/fog-client` issue #146 — "Make fog-client a 64 bit application", open since 2023
- `FOGProject/fos` `docs/adr/0011` — the boot-time config channel, which Future A's deploy-time tier depends on and which that ADR shows is not a solved problem
