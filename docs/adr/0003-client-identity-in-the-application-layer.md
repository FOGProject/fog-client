# 0003 — Client identity belongs in the application layer, not the certificate chain

## Status

**Proposed, and independent.** Unlike
[ADR-0002](0002-runtime-if-a-compiled-agent-survives.md), this ADR does *not*
depend on how [ADR-0001](0001-how-much-of-fog-belongs-on-the-endpoint.md) is
settled. Both futures need a client identity, and the answer is the same either
way. **That makes this the safest of the three to decide first**, and it should
not wait on the endpoint-shape debate.

It targets `working-1.6`: the PHP server, the `hosts` schema and `installfog.sh`
may all change, and a new client may require a new server.

A live vulnerability was found while writing this. It is **deliberately not
described here**, because this document is public and the fix is small enough
that the diff would disclose it. It has been written up for the private advisory
process in `fogproject`'s `SECURITY.md`, and the fixes must not wait for this
architecture decision to be settled.

## Context

The question that prompted this was whether FOG could "improve upon the cert
architecture instead of pinning the CA cert — issued certs per client, or
something that doesn't require as much PKI."

The second half of that instinct is right, and this ADR follows it. The first
half needs a correction first, because the problem is widely described
inaccurately — including in FOG's own documentation.

### Two certificates, routinely conflated

`Authentication.HandShake()` downloads `/management/other/ssl/srvpublic.crt` and
checks `RSA.IsFromCA(RSA.ServerCertificate(), certificate)`. That certificate is
the **client-communication leaf** — FOG's own PKI zone, signed directly by the
root, with `.srvprivate.key` as the half PHP reads on every handshake. It is an
application-layer check on a certificate FOG issues to itself.

The **TLS** layer is a completely separate check, in
`CertificatePolicy.CertValidationCallback`, whose first line is
`if (polerrors == None) return true`.

So: **a Let's Encrypt certificate on the Apache vhost is accepted outright**,
because LE's roots are already in the OS trust store and the callback
short-circuits. What Let's Encrypt cannot be is `srvpublic.crt` — and it should
not be, because that is a zone FOG generates for itself and has no business
outsourcing.

`fog-docs`' `external-ca-lets-encrypt.md` says otherwise: that you cannot drop an
LE certificate onto the vhost and expect clients to keep working, and that the
recommended answer is to run an internal ACME CA such as step-ca. **That describes
the pre-1.6 layout**, where `.srvprivate.key` was simultaneously the web server's
TLS key *and* the key decrypting every fog-client handshake — the exact coupling
the PKI zone split removed, and which `pki-zones.md` records as the reason for the
split. The server-side fix already shipped; the documentation has not caught up.
Correcting that page is follow-up work, and the residual client-side problem is
narrower than it claims.

None of which makes the current design *right*. It makes the diagnosis different.

## The governing constraint

**A client credential that lives on the disk gets cloned by the product's own core
function.**

FOG's entire purpose is capturing a disk and writing it to hundreds of machines.
Every design below is an answer to "where does the credential live, and who is
allowed to mint one", and every design that fails does so because it forgot to
ask. This is the same shape of constraint that `fos`' ADR-0009 records as "trust
cannot bootstrap itself", and it deserves the same prominence.

## Three walls

### Wall 1 — a MAC address is not an identity

`FOGBase::getHostItem()` resolves a host from the `mac=` query parameter, which
the client self-reports from `Configuration.MACAddresses()`. The server's own
source comment in `authorize()` states the problem: *"authorize() is reachable
before login and resolves the host from a request 'mac' alone, which is spoofable
by design on an imaging LAN."*

The obvious fix was already tried and abandoned. `fogbase.class.php` carries a
commented-out `sysuuid` resolution path, disabled because MSI boards ship
duplicate SMBIOS UUIDs. **Record this, or a reviewer will propose it again.** It
also forecloses the naive version of Wall 3's mitigation.

Alongside the MAC there is a bearer token in `token.dat`, rotated each handshake
with one generation of grace. It is symmetric, lives in one place, and is copied
verbatim by any disk clone.

### Wall 2 — the client holds one notion of "the FOG certificate" for two unrelated jobs

Per the Context above, this is not "pinning blocks ACME". Pinning and ACME
currently coexist only because `polerrors == None` short-circuits before the pin
is ever consulted. The defect is that the client cannot distinguish *"is this the
right server"* (a transport property, which the platform is good at) from *"is
this a certificate FOG issued"* (an application property), and uses one mechanism
for both.

**This is a live design constraint on the security fix, not just on the
architecture.** The naive correction to `CertValidationCallback` — always require
the pinned CA in the TLS chain — would break **every** deployment currently
serving Let's Encrypt or a corporate CA on the vhost, all of which work today by
accident of the short-circuit. The fix must validate the vhost against the **OS
trust store with the FOG root as an additional anchor**, never against the FOG
root alone. That is also the argument for installing `ca.cert.der` into the Linux
trust store, which the client does not do today — on Windows it goes into
`LocalMachine\Root`, and on Linux it is left as a bare file and read directly.

### Wall 3 — the pin is bypassed anyway

`CertValidationCallback` has a path on which the pin is never consulted.
Details, impact and the fix are in the private advisory, not here. It matters for
this ADR only as evidence for the general point: **hand-rolled certificate path
validation is the category of code that produces exactly this class of bug**, and
FOG has been maintaining a copy of it — carrying two unaddressed TODOs in its own
docblock, for revocation and for intermediate-CA support — for years.

That is the case for deleting it rather than repairing it. A correct
implementation is not the goal; *not having one to get wrong* is.

## Decision

An **Ed25519 (or P-256) per-client signing keypair** for identity, plus ordinary
**platform TLS validation** for server trust, with **join-token authorized
enrollment**. Delete the bespoke certificate-validation code rather than fix it.

The one-sentence framing: **stop replacing the platform's certificate validation
and start contributing to it.** The FOG root becomes one more anchor in the OS
trust store rather than the sole permitted anchor enforced by bespoke code, and
client identity moves out of the certificate layer entirely — where it never
belonged, because it is not a transport property.

### Rejected: per-client X.509 and mTLS

This is what "issued certs per client" naturally suggests, and it is the option to
reject explicitly and for stated reasons rather than for vague complexity.

It requires an **online issuing CA with a live key on the FOG server** — directly
reversing the direction 1.6 took in response to
[GHSA-94p8-jg9j-99v4](https://github.com/FOGProject/fogproject/security/advisories/GHSA-94p8-jg9j-99v4),
which is the advisory that caused the root key to be taken offline in the first
place. Reintroducing an online signing key months later needs a better
justification than "certificates are standard".

Beyond that: CSR handling in PHP, serial management, a certificate database.
**Renewal** — certificates expire, and a machine powered off for the summer comes
back with an expired cert and no way to renew; the natural grace mechanism is a
bearer token, which is where we started. **Revocation** — CRL and OCSP are real
infrastructure, and the realistic FOG answer is a DB-backed allowlist keyed on
serial, at which point the X.509 is decorative and this is the recommended option
with extra steps. And **`SSLVerifyClient` lives in Apache, not PHP**, while FOG
mixes authenticated client traffic, unauthenticated FOS traffic and browser
traffic on one vhost; untangling that is a configuration project across every
supported distro, and reverse proxies in front of FOG break mTLS unless header
forwarding is configured exactly right.

The request was for *less* PKI. This is more PKI, in the place where PKI is most
expensive.

### Why the keypair wins

One column on `hosts` for the public key, plus algorithm and enrolled-at. **No CA,
no chain building, no expiry, no renewal, no CRL, no OCSP, no Apache
configuration.** Verification is one call — `sodium_crypto_sign_verify_detached`,
core PHP since 7.2, no extension to install — against roughly two hundred lines of
`RSA.cs` that it replaces.

Revocation is `UPDATE hosts SET hostSignKey='' WHERE id=?`. That is *precisely*
what the existing **"Reset Encryption Data"** button already does to
`pub_key`/`sec_tok` via `FOGPage::clearAES()`, so the UI hook, its group-level
variant for bulk re-enrollment after a lab rebuild, and the administrator's mental
model all already exist.

And the `hosts` table stops containing anything worth stealing. Today `hostPubKey`
holds — despite its name — a **live AES session key in plaintext**, so one
read-only database compromise (SQLi, a stolen backup, an exposed phpMyAdmin)
yields every session key and every token on the fleet. With a public key there,
a database read yields public keys. A server compromise also stops being able to
*mint* client credentials, because the server never holds a signing key.

### The large deletion is the point, not a side effect

Once TLS is real and the client can sign, the entire application-layer envelope
goes: `certEncrypt`/`certDecrypt`/`aesencrypt`, `AES.cs`,
`Authentication.Decrypt`, the `#!en=` and legacy `#!enkey=` flags,
`hostPubKey`/`hostSecToken`/`hostSecTokenPrev`/`hostSecTime`, the thirty-minute
rotation, and the grace-token logic with its long apologetic comments about
clients stranded on tokens the server already discarded.

All of it exists to build a secure channel over an insecure one. TLS does that
better, and has been reviewed by more people. This is a **net deletion of the most
fragile code in the system**, which is a far easier proposal to accept than a net
addition.

The stated cost: **HTTPS becomes mandatory for the new protocol.** Many FOG
installs run plain HTTP today. Take that trade explicitly — it is a new client
against a new server, 1.6 already installs a CA and anchors it in the server's own
trust store, and requiring TLS is a reasonable 2026 baseline. If the maintainers
decline, the fallback is an AEAD envelope (AES-256-GCM or ChaCha20-Poly1305 via
`sodium_crypto_aead_*`) with the session key established *by* the signing key
rather than RSA-wrapped — but that is a compromise, and should be written as one.

## Migration: thousands of hosts, nobody touches a machine

This is the part that decides whether the proposal is accepted, so it gets its own
section.

**The key observation: an already-registered host already holds a server-issued
shared secret.** `hostSecToken` is weak, but it is proof of prior enrollment. Use
it exactly once, to bootstrap the strong credential.

1. **Server (1.6).** Add `hostSignKey`, `hostSignAlgo`, `hostSignEnrolled`,
   `hostSignBinding` to `hosts` via the existing `ALTER TABLE` pattern in
   `schema.php`, plus the field maps in `host.class.php` and `hostmanager.class.php`.
   Add `/service/enroll.php`. Advertise support in the existing
   `requestClientInfo` response so the client need not probe-and-fail. `authorize()`
   keeps working byte-for-byte.
2. **Client.** On startup, if there is no local keypair and the server advertises
   support, generate one and POST the public half to `/service/enroll.php`,
   authenticated by **the existing `token.dat` value**. The server verifies it
   against `hostSecToken`/`hostSecTokenPrev` using the same `hash_equals` logic
   already in `authorize()`, stores the public key, and **clears `hostSecToken` in
   the same transaction** — so the weak credential is *spent*, not left lying
   around as a parallel authentication path. Zero admin action; the fleet
   self-heals as clients update.
3. **Enforce.** `FOG_CLIENT_REQUIRE_SIGNED_AUTH`, defaulting off. Off, both paths
   are accepted; on, MAC-plus-token is refused. Add a column to the host list and
   a counter to the dashboard showing enrolled versus not. **A security setting
   only ever gets turned on if the administrator can see when it is safe to turn
   it on** — this is the part that makes the flag more than decoration.
4. **UI.** "Reset Encryption Data" becomes "Reset Client Identity": clears
   `hostSignKey` and mints a single-use join token, displayed once.

On first contact with a dual-mode server the client: signs if it has a key;
enrolls using its token if it has one; looks for a deploy-time join blob if it has
neither; otherwise falls back to the legacy handshake and logs a deprecation
warning naming the server setting. Note that "the server says it does not support
signing" is itself downgrade-attackable, **which is exactly why step 3's
server-side enforcement flag is not optional.**

## Narrowing the bootstrap

TOFU cannot be eliminated — trust cannot bootstrap itself — but it can be reduced
to a decision an administrator makes once, deliberately, over a channel they
already trust. FOG has two such channels and currently uses neither.

**Ship the CA inside the installer.** `client/download.php` already serves the
client package from the FOG server. Have the server embed `ca.cert.der` in the
MSI/deb/rpm it hands out, and delete `PinServerCertPreset`'s network fetch
entirely. The trust decision then collapses to "did I get this installer from my
own FOG server" — which an administrator can meaningfully answer, once, and which
GPO/MDM distribution answers cleanly.

**Offer an out-of-band check.** An installer flag `--ca-fingerprint <sha256>` that
the admin copies from the Certificates page — which already displays exactly that
fingerprint for exactly that purpose. It makes a value the UI currently shows for
diagnosis actionable for verification.

**For FOG-imaged machines, the netboot channel *is* the out-of-band channel**, and
this is the elegant part. A machine that PXE-booted from the FOG server has
already made the trust decision, at a layer below the OS, and the CA is already
compiled into the iPXE binary. FOS can plant the anchor into the deployed
filesystem during post-download. **There is no TOFU for FOG-deployed machines at
all** — the trust was established when the administrator decided this machine may
netboot from this server, which is the trust root of the entire product.

## The imaging problem

This is where per-client-credential designs die, so it gets the longest section.

**The trap.** An admin enrolls a golden machine, installs the client, syspreps,
and captures. The image now contains that machine's private key. Deploy to two
hundred machines and all two hundred present the same credential — with two
hundred different MACs. Either the server rejects 199 of them (fleet-wide
breakage, and the admin blames the new authentication), or it accepts them, in
which case the credential is worth exactly what the old shared token was and the
whole exercise was theatre.

Worth noting how the *current* design survives this: it survives **because it is
weak.** Nothing clears `token.dat` at capture — `UniversalInstaller.cs` explicitly
*preserves* it across upgrades — and cloned tokens work out only because identity
is MAC-first and the token is effectively advisory. That is not a property to
preserve.

Three defences. The first two are required; the third is detection.

**1. Do not let it into the image.** `/images/postdownloadscripts/fog.postdownload`
is an existing, documented, sourced hook, and FOS already mounts and modifies the
deployed filesystem. Extend that to unconditionally delete the client's identity
directory — key, `token.dat`, and any host binding in `settings.json` — from every
deployed image, every time. **This is strictly better than documenting a FogPrep
step, because it does not depend on the administrator remembering**, and it is
idempotent and harmless when there is nothing to delete. Update the FogPrep
documentation too, as belt and braces rather than as the mechanism.

**2. Have the deploy task carry a single-use enrollment blob.** This is the answer
to "a freshly imaged machine that nobody will ever log into". The task already
knows which host it is deploying to. During post-download, FOS writes a small blob
— `{host_id, nonce, expiry}` — into the target filesystem at a well-known path. On
first boot the client (running as LocalSystem, no user required) finds it,
generates a keypair, POSTs the public key with the blob to `/service/enroll.php`,
and deletes the blob. The server validates the nonce as single-use, binds the key
to that host row, and burns it.

FOG already has this pattern: `hostInfoKey` plus `hostInfoLock` is a single-use,
lock-protected, server-issued, task-scoped token that `hostinfo.php` rotates on
consumption.

**Be honest about what this leans on, in the same breath.** The blob is only as
trustworthy as the FOS-to-server channel, and that channel is currently
unauthenticated: `status/hostgetkey.php` is MAC-resolved with no authentication —
the maintainers' own in-source comment labels it "Aisle 016" and notes that its
`FOG_HOSTKEY_ALLOWED_SOURCES` mitigation **defaults to empty** — and FOS fetches
with `curl -Lks` (`-k`, no verification) in `bin/fog` and four places in
`funcs.sh`, despite the CA already being compiled into iPXE and anchored in the
server's own trust store. A LAN attacker who spoofs a MAC during a deploy window
can race for the blob.

Two things make that acceptable, and both belong in the text rather than in a
footnote. First, it is **not a new exposure**: the same attacker already harvests
plaintext AD credentials and Windows product keys from `hostinfo.php` today.
Second, it is **bounded** — a single-use nonce, short expiry, one host, visible in
the audit log — where the status quo is unbounded, permanent, silent
impersonation. But name the dependency plainly: **fixing the FOS channel
(`--cacert` instead of `-k`) is a prerequisite for this to be worth much**, and
`FOG_HOSTKEY_ALLOWED_SOURCES` should stop defaulting to empty.

**3. Bind and detect.** Record a fingerprint at enrollment — MAC set, SMBIOS UUID,
machine SID. If a signature verifies but the presenting machine's fingerprint does
not match the binding, refuse, mark the host, and surface it in the UI as a
possible cloned credential. **This is detection, not prevention**, and explicitly
*not* identity — Wall 1 is why. It catches the sloppy-capture case loudly instead
of silently, which is the failure mode administrators will actually hit.

## TPM: a pluggable provider, not a baseline

State the threat model first, or this cannot be judged. FOG's deployment reality
is a school, a college lab, or an SMB: a flat or lightly segmented LAN, the
imaging VLAN often the same VLAN the machines live on afterwards, and physical
access by untrusted people as the norm rather than the exception. **The primary
adversary is an unprivileged LAN participant** — a student who can ARP-spoof, run
a rogue DHCP server, set any MAC, and reach any HTTP endpoint, but cannot get code
onto the FOG server. Secondary: an unprivileged local user on a managed endpoint,
and a compromised web process (which 1.6 already treats as in scope — that is what
taking the root key offline is *for*). Out of scope: TPM chip-level attacks, a
hostile FOG administrator, nation-state adversaries.

Against the primary adversary, **a TPM buys nothing that a `0600` key file does
not.** That is the honest assessment and it should be stated rather than skipped.

But against the **cloned-image failure** — which is not an attacker at all, but an
operational mistake every administrator eventually makes — a key created in the
TPM with export withheld is the only mechanism that makes cloning *impossible*
rather than merely detectable. It is not on the disk, so capture cannot copy it.
That is the argument for it, and it is a much better argument than "hardware
security is good".

So: **design the credential store as a pluggable provider interface**, with a
software-file implementation as the required baseline and a Windows CNG /
Microsoft Platform Crypto Provider implementation as an optional profile, default
where available. Doing the interface up front costs almost nothing; retrofitting
it later costs a protocol revision. And require that the protocol **never
transmits the private key and never assumes it is extractable** — sign-only, never
encrypt-to. That single constraint is what keeps the TPM path open.

There is no Linux equivalent worth having today: TPM2 means shelling out to
`tpm2_*` or taking a native dependency, availability across FOG's supported
distros is inconsistent, and a large share of Linux FOG clients are VMs with no
vTPM. The realistic Linux answer is far more modest and far better value per unit
of effort — a `0600 root:root` key file, and **actually chmod-ing it**, which is a
one-line fix that closes a live issue today.

## Consequences

- HTTPS becomes mandatory for the new client protocol.
- Ed25519 is not free on the client: .NET Framework 4.5.2 has no Ed25519 and
  Mono's `ECDsaCng` is unusable, so this means either a `Portable.BouncyCastle`
  dependency or the .NET move in
  [ADR-0002](0002-runtime-if-a-compiled-agent-survives.md). Server-side there is
  no such constraint — libsodium is core PHP.
- Net deletion of `RSA.cs`, `AES.cs`, `certEncrypt`/`certDecrypt`, and four
  `hosts` columns.
- New schema, one new endpoint, FOS post-download changes, and a rewrite of
  `external-ca-lets-encrypt.md` — most of which becomes unnecessary, **which is
  the headline benefit for operators and should be said in those words**.
- The deploy-time channel (`hostgetkey.php`, FOS's `curl -k`) becomes a
  prerequisite rather than an unrelated weakness.

## References

- The private security advisory covering the validation bypass and the immediate fixes (filed per `fogproject`'s `SECURITY.md`)
- [GHSA-94p8-jg9j-99v4](https://github.com/FOGProject/fogproject/security/advisories/GHSA-94p8-jg9j-99v4) — the advisory behind the PKI zone split
- `FOGProject/fos` `docs/adr/0009` — "trust cannot bootstrap itself", the same shape of constraint
- `fogproject` `docs/PKI_ZONES.md` and `fog-docs` `docs/kb/reference/pki-zones.md`
- The "Aisle" findings referenced in `fogproject` source comments (016, and 050 / 2.7.3 in `authorize()`)
