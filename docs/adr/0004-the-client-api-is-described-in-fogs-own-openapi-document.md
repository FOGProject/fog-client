# 0004 — The client API is described in FOG's own OpenAPI document, and the endpoint client is generated from it

## Status

**Proposed, and independent of all three.** It does not depend on
[ADR-0001](0001-how-much-of-fog-belongs-on-the-endpoint.md)'s endpoint shape —
a generated client is equally useful to a compiled agent and to a signed script,
because both make the same HTTP calls. It does not depend on
[ADR-0002](0002-runtime-if-a-compiled-agent-survives.md)'s runtime. It is a
natural companion to [ADR-0003](0003-client-identity-in-the-application-layer.md)
but decidable separately: you could adopt keypair identity and still hand-write
the client, or generate the client and keep the old identity scheme.

Nothing is implemented.

## Context

This ADR exists because of something the other three did not know about.

**`FOGProject/fog-sdk` was created on 2026-08-29** — after the exploration
behind ADR-0001 to 0003 — and it changes the arithmetic on one question those
ADRs left as hand-written work: how the endpoint talks to the server.

What it is, from its own README: *"Generated API clients for FOG Project
(PowerShell and Python), plus the hand-written authentication layer they need."*
It pins a snapshot of FOG's OpenAPI document and generates clients from it with
AutoRest (PowerShell) and openapi-generator (Python). The generated output is
deliberately **not committed** — it is reproducible from `spec/` plus a pinned
generator version.

The important fact is what it is generated *from*. FOG 1.6 serves its own
OpenAPI document, live, from `GET {webroot}/system/openapi` and
`/swagger.json`, produced by `OpenAPI::document()`. The snapshot in
`spec/openapi/fog-1.6.json` records what that contains today:

| | |
|---|---|
| OpenAPI version | 3.0.3 |
| Title / version | FOG Project API, `1.6.0-beta.4219` |
| Paths | 380 |
| Operations | 528 |
| Schemas | 162 |
| Route classes | 51 |

**The pipeline already exists and already works.** That is the whole basis of
this ADR: FOG does not need to build a spec, a generator, or a release process
for client SDKs. It has all three.

### Why the SDK cannot simply be used on endpoints

This needs stating plainly, because "we have an SDK now, use it" is the obvious
next thought and it is wrong for a specific and instructive reason.

**The document describes the administrative API, and nothing else.** All 380
paths are the `Route::$validClasses` CRUD surface — `/host`, `/snapin`,
`/snapinjob`, `/snapintask/{id}/cancel`, and so on. There is **no host check-in
surface in it at all**: no `requestClientInfo`, no `snapins.checkin`, no
registration, no enrollment. Those live outside the REST API, under
`/service/*` and `/management/index.php`, in a hand-rolled `#!`-prefixed
protocol the OpenAPI document has never described.

Even the apparent snapin overlap is illusory. `/snapin` and `/snapinjob` are
*administrative* operations — create a snapin, associate it, cancel a job. What
an endpoint needs is "what is assigned to me" and "here is my exit code", which
is a different and much smaller surface.

**And the security schemes are all user credentials.** The document declares
three, and every one of them authenticates a *person*:

- `bearerAuth` — an API token from a user's API tab, `fog_`-prefixed, hashed at
  rest, shown once
- `fogApiToken` + `fogUserToken` — the legacy header pair
- `fogApiToken` + `basicAuth`

Per the SDK's README these tokens **act with their owner's roles**, carry no
scope of their own, **do not expire**, and are revocable only in the UI —
FOG deliberately exposes no token-management REST surface, *"so that one API
credential cannot mint another."*

Every one of those properties is correct for an integration and disqualifying
for an endpoint. Shipping such a token to a thousand workstations means a
fleet-wide administrative credential on every lab machine, unrotatable from the
endpoint, handed over entire by one stolen laptop. That is the exact inverse of
what ADR-0003 argues for — per-host, revocable in one row, worthless to whoever
steals it.

**So the SDK is not the answer. The pipeline behind it is.**

## Decision

**Define the client protocol as real REST operations inside FOG's existing
OpenAPI document, under a new host-scoped security scheme, and generate the
endpoint's HTTP layer from it with the pipeline `fog-sdk` already uses.**

Three parts:

1. **The operations live in the same document.** The replacement for
   `requestClientInfo`, the snapin check-in, and ADR-0003's enrollment endpoint
   are described in `OpenAPI::document()` alongside everything else, and appear
   at `/system/openapi` like the rest. They are not a second spec and not an
   undocumented sidecar.
2. **A separate security scheme, `hostAuth`.** Host operations declare it;
   administrative operations do not. Whatever ADR-0003 settles on becomes its
   definition. This is the part that keeps the SDK safe to publish: a generated
   admin client cannot accidentally acquire host operations, and a generated
   host client cannot acquire admin ones, because the schemes do not overlap.
3. **The endpoint's transport is generated, not written.** Same spec, same
   pinned generator, same reproducible build. What stays hand-written is the
   thing `fog-sdk` already keeps hand-written: the credential layer.

## Why this is worth doing

**It removes a category of work rather than a quantity of it.** Request
building, response parsing, model shapes and error mapping stop being code
anyone maintains. `fog-sdk`'s structure already demonstrates the split — a
generated surface plus a small hand-written auth layer — and it is the right
split here too.

**It is the strongest available argument for making the client protocol proper
REST.** ADR-0003 proposes retiring the `#!en=` envelope and the `#!ih`/`#!ihc`/
`#!ist` sentinel codes, and the case there is about security. This is a second,
independent case: a protocol with real paths, real status codes and real schemas
can be *described*, and a described protocol can be *generated*. A fourth
bespoke dialect cannot be. If the client API is going to be rewritten once, this
is the argument for rewriting it into something a machine can read.

**It makes the endpoint contract reviewable, and the enforcement already
exists.** A change to what the client sends becomes a diff in a spec file,
visible in a pull request, rather than a change in C# that a PHP reviewer will
not read — and vice versa. `fogproject` already carries a standing rule that a
route change without a corresponding OpenAPI change must justify itself, and
`tests/openapi-route-coverage.test.php` enforces it in both directions, with an
exclusion list where every entry has to carry a written reason.

**That test makes this ADR's argument on its own**, which is the strongest
evidence available that the project already reasons this way:

> *"The document is generated per request, which reads as 'it keeps itself
> current' and is only half true. … a route that is not the generic CRUD shape
> is served and undescribed unless someone edits `openapi.class.php` in the same
> breath. Nothing caught that, and two upload routes sat undescribed from May to
> August as a result.*
>
> *The cost lands on a caller, not on us: a client generated from the document
> simply has no method for the missing endpoint, and the reasonable conclusion
> is that the server cannot do it."*

The host protocol is the largest body of routes FOG serves that is *entirely*
outside that guarantee — not one undescribed operation, but a whole undescribed
surface. Bringing it into the document puts it under a test that already runs,
rather than requiring a new one.

**It gives ADR-0001 one less thing to argue about.** Both futures need an HTTP
layer. Under Future B it is generated into whatever ADR-0002 picks; under
Future A the same spec generates the small amount of request-shaping a signed
script needs, or at minimum documents it precisely enough to write by hand
without guessing. The endpoint-shape debate stops carrying the transport
question with it.

## What this does not decide

- **It does not decide the credential.** That is ADR-0003. This ADR only says
  the scheme is declared separately from the administrative ones.
- **It does not make the endpoint import the SDK.** The generated *host* client
  is a different artifact from the published admin SDK, sharing a spec and a
  build, not a package.
- **It does not require the endpoint to carry a generated client at all.** If
  ADR-0001 lands on signed scripts, the spec's value is the contract and the
  server-side tooling, and the script can still be a handful of `Invoke-RestMethod`
  calls written against a document that actually describes them.

## Consequences

- **`OpenAPI::document()` grows a second audience.** It is currently written for
  administrators and integrations; host operations have different needs
  (unauthenticated enrollment, one endpoint reachable before credentials exist).
  Expect the `_fixedPaths()`/`_classPaths()` split to need a third bucket.
- **The spec becomes a compatibility surface.** Once an endpoint client is
  generated from it, changing a host operation breaks deployed endpoints in a
  way that changing an admin operation does not — administrators upgrade their
  tooling deliberately; a thousand lab machines do not. Host operations need a
  versioning discipline the admin surface has not so far needed.
- **`fog-sdk` gains a dependency it does not have today.** Its snapshot is taken
  from a `working-1.6` commit and pinned with a provenance record. Adding host
  operations means that snapshot now also governs endpoint behaviour, which
  raises the stakes on keeping `PROVENANCE.json` honest — the file's own comment
  already warns that a stale provenance record is worse than none.
- **The document is OAS 3.0.3, deliberately.** `fogproject`'s CLAUDE.md records
  that this is a decision to revisit rather than quietly extend — `nullable:
  true`, not `type: [x, 'null']`. Host operations follow the same constraint.
- **This is additive and can wait.** Nothing here blocks ADR-0003; a hand-written
  client against a well-specified protocol is a perfectly good first
  implementation, and the generation can follow once the shape has settled.

## References

- [ADR-0003](0003-client-identity-in-the-application-layer.md) — the credential this scheme would carry
- [ADR-0001](0001-how-much-of-fog-belongs-on-the-endpoint.md) — why this is useful under either future
- `FOGProject/fog-sdk` (`dev`) — the pipeline, and `spec/openapi/PROVENANCE.json` for the snapshot's terms
- `fogproject` `packages/web/src/Router/OpenAPI.php` — `OpenAPI::document()`, served at `/system/openapi`
- `fogproject` `tests/openapi-route-coverage.test.php` — the both-directions coverage test host routes would fall under
- `fogproject` ADR-0027 — why the admin token is shaped the way it is, and why that shape is wrong for a host
