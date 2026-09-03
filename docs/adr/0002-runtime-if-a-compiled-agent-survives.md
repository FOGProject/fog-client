# 0002 — If a compiled agent survives, it moves to .NET LTS

## Status

**Proposed, and conditional.** This ADR only applies if
[ADR-0001](0001-how-much-of-fog-belongs-on-the-endpoint.md) settles on Future B —
keeping a compiled agent. If it settles on Future A, most of this document is
moot and only Phase 0 survives.

Nothing here has been implemented. One number in it is an estimate rather than a
measurement, and it is flagged where it appears.

## Context

Every project in the solution targets .NET Framework 4.5.2 — thirteen `.csproj`
files, no exceptions. Linux and macOS run those same binaries under **Mono**,
which was donated to WineHQ in 2024 and is in maintenance.

The Linux situation is worse than "runs on an unmaintained runtime", and the
detail is worth leading with because it is the one that makes the case:
`Service/main.cs` calls `ServiceBase.Run()` — a *Windows service* entrypoint. On
Linux that is launched by `mono-service` from `UniversalInstaller/Scripts/control.sh`,
wrapped in a systemd unit that is `Type=oneshot` with `RemainAfterExit=yes`, with
process tracking done by a lockfile and `ps -p`. The unit's `ExecReload` line is
also simply broken — systemd runs no shell, so
`/opt/fog-service/control.sh start;/opt/fog-service/control.sh stop` passes
`start;/opt/fog-service/control.sh` as a literal argument. **The Linux daemon is a
Windows service under emulation, supervised by a shell script systemd cannot see
into.**

The build is in the same state. `.travis.yml` and zazzles' Jenkins script both
invoke `xbuild`, removed from modern Mono, on a Travis free tier discontinued
around 2021. There are no GitHub Actions in either repo. And `libs/Zazzles.dll` is
a checked-in binary whose two references disagree about what it is:
`Service.csproj` declares `Version=1.0.5764.34033, processorArchitecture=x86`
while `Modules.csproj` declares `Version=1.0.5966.27699, MSIL`, for the same file.
That is why issue #146 (64-bit) is still open.

The support floor for this decision is **Windows 10 21H2 / LTSC 2021 and Windows
11**. Worth recording why 21H2 rather than 22H2: 22H2 is the last *general*
Windows 10 release, but the LTSC line is LTSC 2021, built on 21H2 — there was
never a Windows 10 LTSC 2022. General 22H2 left support in October 2025; LTSC 2021
runs to January 2027 on Enterprise and January 2032 on IoT Enterprise. **The long
tail FOG has to serve is LTSC, not consumer**, so flooring at 22H2 would exclude
exactly the lab machines most likely to still be running FOG.

## Decision

Port to the current .NET LTS, published **self-contained single-file**, with
per-OS target frameworks — `net10.0` for the daemon and the shared library,
`net10.0-windows` for the Windows UI and Windows platform code — hosted on
`Microsoft.Extensions.Hosting` with `.UseWindowsService()` and `.UseSystemd()`.

## Why: the port is small because the seam already exists

The standard objection to .NET for cross-platform work is that half the BCL is
Windows-only. That is true, and here it is **irrelevant**, because
`System.Management`, `System.DirectoryServices` and the registry are already
confined to files under `Windows/` directories. The codebase computes
`Settings.OS` once and selects `IPower`/`IUser` implementations from it;
`Modules/HostnameChanger/{Windows,Linux,Mac}` and
`Modules/PrinterManager/{Windows,Unix}` follow the same shape. **The architecture
already partitions the code along exactly the line the platform packages need.**
That is an argument about *this* codebase, not about .NET in general, and it is
the strongest one available.

The genuine blockers are five constructs, not a rewrite:

| Construct | Sites | On .NET 8+ |
|---|---|---|
| `Thread.Abort()` | `zazzles/AbstractService.cs`, `Service/UserServiceSpawner.cs` | throws `PlatformNotSupportedException` |
| `AppDomain.CreateDomain` | `zazzles/Modules/Hotloader.cs` | unsupported — but zero call sites; delete the file |
| `cert.PublicKey.Key` cast to `RSACryptoServiceProvider` | `zazzles/Data/RSA.cs` | `GetRSAPublicKey()` / `GetRSAPrivateKey()` |
| `AesCryptoServiceProvider` | `zazzles/Middleware/Authentication.cs` | `Aes.Create()` |
| `ServiceBase.Run` | `Service/main.cs` | generic host |

Removing `Thread.Abort()` is the invasive one, because it is how the module loop
is stopped today — so it means threading a `CancellationToken` through
`ModuleLooper()`. That falls out naturally from adopting the generic host, whose
`IHostApplicationLifetime` hands you the token. **It is also a real bug fix:**
aborting the thread can tear down a module mid-write. The port fixes a live
correctness problem as a side effect.

The generic host is what deletes `mono-service` and `control.sh` outright, giving
`Type=notify` on Linux and real SCM integration on Windows from one `Program.cs`.

## Why not the alternatives

**Go** is the strongest rival and is genuinely better on two axes: a static
10–15 MB binary against .NET's self-contained payload, and cross-compilation from
a Linux CI runner with no Windows machine in the loop. Windows service support
(`golang.org/x/sys/windows/svc`) and systemd support are both good. It loses on
GUI — there is no good cross-platform Go toolkit for a countdown dialog with
twelve translations — and on WMI, where `go-ole` COM interop is workable but
nobody in FOG knows it. The decisive argument is none of that. It is that Go means
a **rewrite** of 17,526 lines and the re-derivation of a decade of Windows edge
cases — session isolation, token impersonation, domain-join error codes, printer
driver quirks — against a project constraint that there cannot be a long gap with
no releases. Go is not worse; it is worse *from here*.

**Rust** is everything said about Go, plus better Win32/WMI ergonomics
(`windows-rs` is generated from the official metadata) and real memory-safety
value in code that P/Invokes into `advapi32` with token privileges. It is rejected
on **maintainer sociology**, and that deserves to be said plainly rather than
dressed up as a technical objection: this is a volunteer project that went two and
a half years without a client commit. Choosing the language with the highest
barrier to a drive-by contribution is choosing to have fewer contributors.

**Node.js/TypeScript** is viable in the narrow technical sense and wrong on
posture. The privileged Win32 work — `NetJoinDomain`, `WTSQueryUserToken`,
`AdjustTokenPrivileges`, `WinVerifyTrust` — needs either a native addon (node-gyp,
a C++ toolchain in CI, an ABI recompile every Node major) or dynamic FFI with
hand-declared struct layouts. That puts the most security-sensitive code in the
product behind a layer no compiler checks. And a LocalSystem service that renames
domain-joined machines should be small, compiled, signed and dependency-light; a
JIT plus a `node_modules` tree of hundreds of transitive packages running as
SYSTEM is the opposite of all four. The current client has about eight third-party
dependencies. That asymmetry is the answer — not "JavaScript is slow", which for a
poll loop it is not.

**Electron** is a category error, and one paragraph is the right amount of
attention. The FOG client is a *service*: it runs as LocalSystem with no window
and no session. Electron exists to put a Chromium renderer in front of a Node main
process — it exists to give you a window. Ninety percent of this codebase has no
UI at all, and the part that does is `ShutdownGUI` (a countdown dialog) and `Tray`
(109 lines). Electron would ship roughly 150 MB of Chromium per endpoint to
replace about 500 lines of form code, and would *still* need a separate
non-Electron process to be the daemon, because Electron cannot sanely be a Windows
service. The outcome is Node's posture, plus a browser engine, plus the two-process
split you were trying to avoid.

**Staying on .NET Framework + Mono** is not a destination — Mono is in
maintenance, `xbuild` is gone so the build must change even in the do-nothing
option, `mono-service` has no future, and 4.5.2 itself is out of support. But its
first half is real: deleting dead code and standing up CI needs no TFM change.
That is Phase 0 below, and it is worth doing under Future A too.

## Component fate

Ten projects become six.

| Today | Fate |
|---|---|
| `Service` | survives, rewritten as a generic-host worker; `UserServiceSpawner.cs` deleted |
| `Modules` | survives as-is |
| `Zazzles` | **merges in as a source directory** |
| `UserService` | survives as a binary, but stops being spawned |
| `Tray` | merges into `UserService` — 109 lines that are a separate process only because of the Bus |
| `ShutdownGUI` | survives on Windows; Linux shells out |
| `PrinterManagerHelper` | dies — a WinForms+WMI diagnostic tool; fold anything useful into `Debugger` |
| `Debugger` | survives, slimmed |
| `UpdateHelper` | survives on Windows only |
| `UpdateWaiter` | dies — its whole job is waiting for a Mono/Windows service restart |
| `SetupHelper` | survives, retargeted to a WiX v4+ custom action |
| `UniversalInstaller` | splits: MSI on Windows, `.deb`/`.rpm` on Linux; the WinForms GUI installer dies |

**Zazzles merges as source rather than becoming a submodule**, which is what issue
#98 proposes. A submodule fixes the vendored-blob problem but keeps two repos in
lockstep for a library with exactly one consumer that has never shipped
independently — while the current costs are concrete: the version mismatch above,
and the fact that a contributor cannot build fog-client without also building
zazzles and copying a DLL. A root-level `dotnet build` producing everything is a
**precondition** for the CI work. Keep the zazzles repo archived for history and
attribution; it is a named person's project and absorbing it quietly would be the
wrong way to do this.

**WinForms survives on Windows and dies on Linux**, deliberately rather than by
inertia. On `net10.0-windows` it is supported, `NotifyIcon` works, and the twelve
`ShutdownGUI` localizations port unchanged as satellite assemblies. Rewriting in
Avalonia to get one UI codebase would cost that localization plumbing, add ~20 MB,
and introduce a framework nobody here knows — to serve the Linux desktop client
population, which is the smallest constituency FOG has. On Linux, WinForms was a
Mono feature and does not exist on .NET; the right answer is the idiom this
codebase already uses everywhere else, which is to shell out — `zenity` or
`kdialog`, `notify-send` for notifications, discovered at runtime the same way
`LinuxInstall.cs` already probes for systemd versus init. If Linux desktop demand
ever materialises, revisit Avalonia then; record that as the trigger rather than
deciding it now.

**The Bus becomes a named pipe on Windows and a Unix domain socket on Linux**,
both in-box, keeping the existing `Bus.Emit`/`Bus.Subscribe`/`Channel` API so no
caller moves. The point is not speed. It is that **both transports carry peer
identity** — `PipeSecurity` plus `GetImpersonationUserName()` on Windows,
`SO_PEERCRED` on Linux — which `ws://127.0.0.1:1277/` never did. That is what
retires the comment in `Bus.cs` (*"It MUST be assumed that this socket is
compromised — Do NOT send security relevant data across it"*) and makes
`ProtectedChannels` enforceable by **who is asking** rather than by a client-side
honour system. It also deletes `SuperWebSocket`, `WebSocket4Net` and four
`SuperSocket.*` DLLs.

There is a useful intermediate step: in-box `HttpListener` + `System.Net.WebSockets`
can reimplement the Bus with **zero protocol change**, deleting the six abandoned
libraries before the transport question is settled. That decouples "get off dead
dependencies" from "redesign IPC", which is why Phase 1 and Phase 3 are separate.

**The per-user split survives on Windows and is not negotiable** — Session 0
isolation means a LocalSystem service cannot draw a window in a user's session.
But the *spawning* model should die on both platforms. Today
`UserServiceSpawner.cs` polls `User.AllLoggedIn()` every five seconds and launches
an impersonated process per user, tracking them in a dictionary and killing them
with `Thread.Abort()`. Invert it: on Windows, install a per-user autostart that
**connects in** to the daemon's pipe, so the daemon stops being a process
supervisor and `ProcessPrivileges.dll` is deleted; on Linux, a **systemd user
unit** (`systemctl --global enable`), which is precisely the mechanism for "one
instance per logged-in session". That also gives Linux a user agent for the first
time — `LinuxInstall.cs` installs none today — and deletes both the
`su - <user> -c "mono ..."` spawn path and the `DISPLAY=:0` injection in
`ProcessHandler`.

## Consequences

**The payload grows by roughly an order of magnitude.** Today's
`SmartInstaller.exe` is a few MB; a self-contained .NET publish carrying WinForms
will land somewhere around 60–90 MB. NativeAOT is unavailable and trimming is
unsupported for WinForms, so this is not tunable away. **That figure is an
estimate, not a measurement** — it is the weakest quantitative claim in this ADR
and the one most likely to change the decision, so measure it in Phase 1 before
committing further. It lands on every FOG server's Apache in every update cycle.
If it proves untenable on slow site links, the mitigations are per-component
publishing (keep the small helpers small), framework-dependent deployment with the
runtime installed once, or reopening Go for the daemon specifically.

**`MetroFramework` must be removed**, so `ShutdownGUI` and `PrinterManagerHelper`
get a visual redesign whether or not anyone wanted one. It is an unversioned DLL
with no .NET port and no upstream. Budget for it; do not discover it in week six.

**This commits the project to a three-year LTS bump cadence.** An ADR that
recommends .NET without also committing to doing it again in 2028 — as a routine
TFM bump with CI, not as a project — is dishonest. FOG is in its current position
precisely because it got off .NET Framework's treadmill in 2016 and stayed off.

**WiX v3 is EOL** and the migration to v4+ lands in the same window, including
re-binding `SetupHelper`'s custom action. Delete `MSI/Installer.vdproj`, a Visual
Studio Setup project dead since VS2010. Keep the MSI itself — GPO and SCCM
deployment need it.

**Code signing is the hardest item and it is organisational, not technical** —
which is worth stating so it is not used to argue for or against any runtime.
`BuildTools/SignCode.cmd` signs with `/n "FOG Project - Sebastian Roth"`, an
individual's certificate, from a machine. Since June 2023 Microsoft requires
code-signing keys on FIPS-140-2 Level 2 hardware, which makes both "run signtool
on a laptop" and "sign in CI" awkward. Azure Trusted Signing or SignPath (free for
OSS) move the key off one person's machine, which is a bus-factor fix as much as a
compliance one.

**Self-update needs one correctness fix regardless.** Today the Linux client
downloads `SmartInstaller.exe` — a Windows PE — verifies its **Authenticode**
signature, and runs it under Mono. Linux should move to `.deb`/`.rpm` from a
FOG-hosted repo, or at minimum a signed tarball plus `systemctl restart`, which
deletes `UpdateWaiter` and most of `UpdateHelper` on that platform. Unify
verification on a signed manifest both platforms check identically, keeping
Authenticode on Windows as an additional OS-level check.

**64-bit (issue #146) becomes a publish RID**, so the build problem evaporates.
What remains is real: auditing registry redirection and any hardcoded
`Program Files (x86)` path. `win-arm64` then comes free from the same mechanism,
which matters now that Snapdragon-X laptops are appearing in fleets.

## Phasing

Each phase ships. Nothing is big-bang.

**Phase 0 — make the repo buildable.** No runtime change, no user-visible change.
Merge zazzles as source; delete `libs/Zazzles.dll`, `libs/EngineIoClientDotNet.dll`
(referenced but unused), `Debugger/Resources/Pipe{Client,Server}.dll` (orphaned
since the switch to the WebSocket bus), `Zazzles/Modules/Hotloader.cs` (zero call
sites, and unsupported on .NET anyway), `MSI/Installer.vdproj`, `FOGService.v12.suo`,
`*.csproj.user`, `.travis.yml`, `jenkins-build*`. Migrate `packages.config` to
`PackageReference`. Stand up GitHub Actions. **This is the highest-leverage item
in the document and it is runtime-independent — it is worth doing under Future A
too.**

**Phase 1 — .NET on Windows, existing protocol. The first release.** Fix the five
blockers, adopt the generic host, reimplement the Bus on in-box types with no wire
change, remove `MetroFramework`, publish self-contained `win-x64`, WiX v6 MSI,
signed. **It speaks the existing protocol, so it deploys against production FOG
servers today.** Linux keeps shipping the old Mono build meanwhile.

**Phase 2 — Linux, Mono-free.** Self-contained `linux-x64`, real `Type=notify`
unit, systemd user unit for the agent, `.deb`/`.rpm` packaging, and
`BootStrap/install.sh` stops adding a Mono repo. Mono leaves every endpoint.

**Phase 3 — IPC.** Named pipe / UDS with peer-identity authorization; invert the
Windows user agent to connect in.

**Phase 4 — protocol**, against `working-1.6`. See
[ADR-0003](0003-client-identity-in-the-application-layer.md).

The strangler property that makes this safe: **if everything stops after Phase 1,
FOG is still better off** — a supported runtime, live CI, 64-bit, six fewer
abandoned dependencies — and nothing is broken.

## One thing to record, because it will be re-proposed

**Keep the hardcoded `IModule[]` array.** It is regularly flagged as something
that "should be reflection". It should not. `Hotload` *was* the reflection
experiment, and it died with zero call sites. An explicit array is auditable,
trim-friendly, AOT-friendly, and makes the module set reviewable in a diff.

## References

- [ADR-0001](0001-how-much-of-fog-belongs-on-the-endpoint.md) — the decision this one is conditional on
- [ADR-0003](0003-client-identity-in-the-application-layer.md) — the protocol work in Phase 4
- `FOGProject/fog-client` issue #146 (64-bit), issue #98 (zazzles as a submodule — declined here, with reasons)
