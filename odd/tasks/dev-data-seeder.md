# Feature: Development data seeder (bootstrap reproducible)

## Objective
Add a reproducible, Development-only seeder that creates the base data needed to
exercise the API from a fresh database: countries + the 4 dev accounts
(student, instructor, admin, superadmin @elearning.com). Today a freshly
migrated database is completely empty and the API is unusable end to end.

## Why
Found while bringing the project up for the first time on a new machine.

`dotnet ef database update` creates the schema and seeds only the 3 `badges`
rows. It seeds **no users and no countries**. That produces a hard circular
blocker:

- `POST /api/auth/register` requires an existing `countryId`
  (`RegisterUserHandler` → `El pais con id 'X' no existe.` → 404).
- `POST /api/admin/countries` is `[Authorize(Roles = "superadmin")]` — but no
  superadmin user exists, and none can be created without a country.

Evidence that dev accounts existed but were never reproducible:
- `src/backend/ELearning.API/ELearning.API.http` references 4 logins
  (`student@`, `instructor@`, `admin@`, `superadmin@elearning.com`, sharing one
  dev password that is no longer stored in this repository).
- `odd/tasks/post-mvp3-bugfix-backlog.md` repeatedly refers to "the seeded
  student account" / "seeded dev password", and once had to redact that
  password from the doc (`R1-backlog-doc-restates-seed-credentials`).

Those accounts lived only in a previous, now-gone Postgres instance. Verified
absent from the repo: no `HasData` on `UserConfiguration`, no seeding in
`Program.cs`/DI, no `.sql` seed script anywhere. So every clone currently hits
the same wall.

## Specs
Verbatim requirement from the user:

> "primero atacaremos el riesgo 1 [...] ahora nos enfocaremos en el backend,
> ayudame primero que todo a levantar el proyecto"

> "Crear un seeder en el código (reproducible)"

> Decision recorded: **"DevDataSeeder en startup (solo Development)"** —
> "Seeder de desarrollo en startup: crea usuarios/países si faltan, solo en
> Development. Passwords hasheadas en runtime con PasswordHasherService. Cero
> secretos en el repo."

Design constraints accepted by the user for this option:

> "Mitigación del contra: guard clause estricta `if (!env.IsDevelopment())
> return;` + test que verifique que en Production no hace nada."

Seed content to reproduce (accounts taken from `ELearning.API.http`; the shared
password is deliberately **not** recorded here — see S7):

> `student@elearning.com` / `<DevSeed:Password>`
> `instructor@elearning.com` / `<DevSeed:Password>`
> `superadmin@elearning.com` / `<DevSeed:Password>`
> `admin@elearning.com` / `<DevSeed:Password>`

S1. A Development-only seeder must create the base countries, and the 4 dev
    accounts when the dev seed password is configured, so that a fresh clone +
    `dotnet ef database update` + one documented `dotnet user-secrets set`
    command + `dotnet run` yields a usable API.
S2. The seeder must not run and must not write anything outside Development.
S3. The seeder must be idempotent: running it against an already-seeded
    database must not duplicate rows and must not throw.
S4. The seeded passwords must be hashed at runtime with the existing
    `IPasswordHasherService` — neither a plaintext password literal nor a
    password hash may be committed.
S5. After seeding, `POST /api/auth/login` with `student@elearning.com` and the
    password configured in `DevSeed:Password` must return 200 with a JWT, and
    the 4 accounts must carry their documented roles (student, instructor,
    admin, superadmin).
S6. Seeded accounts must be usable without the email-verification step (they
    existed and worked before, so `IsEmailVerified` must be true).
S7. The dev seed password must come from configuration (`DevSeed:Password`;
    user-secrets, or the `DevSeed__Password` environment variable). When it is
    missing or blank the seeder must not throw, must create no user row, and
    must log a warning naming the missing key; countries are still seeded.
S8. No secret material may live in tracked configuration: `appsettings.json`
    must carry neither the database password nor `Jwt:SecretKey`, and the API
    must fail fast at startup with a message naming the missing configuration
    key and how to set it.

## Scope
- New `DevDataSeeder` in `ELearning.Infrastructure` (infrastructure concern:
  it writes to the database through `ApplicationDbContext`).
- Invocation from `ELearning.API/Program.cs` behind an environment guard.
- Seed countries: a small fixed set with deterministic codes/names. Ids are
  DB-generated (`Country.Id` is identity), so the seeder must resolve the
  created country ids, not hardcode them.
- Seed users via the existing domain factories (`Country.Create`, `User.Create`)
  and the existing `IPasswordHasherService` — respect the entity
  encapsulation, do not set private setters or insert raw rows.
- `User.Create` produces `IsEmailVerified = false`; the seeder must call the
  existing `VerifyEmail()` method to satisfy S6. (No entity change needed.)

Out of scope (explicitly not requested):
- Seeding courses, lessons, quizzes, enrollments, badges-earned or
  notifications. Not needed to unblock login/API usage.
- ~~Changing the `Jwt.SecretKey` or the connection-string secret currently
  committed in `appsettings.json`.~~ **Moved into scope by the GitGuardian
  block; see T7 / S8.** It was originally deferred as a security follow-up, but
  the scanner refuses the branch while those literals stay tracked.

## Constraints / conventions
- Architecture: seeding lives in Infrastructure; `Program.cs` only calls it.
  Domain is untouched except by using its existing factory methods.
- Tests: xUnit + Moq, mirroring `ELearning.Tests/Unit/...` layout. The
  codebase has **no integration-test project**, so the seeder tests must use
  EF Core's in-memory or SQLite-compatible provider, or a mocked
  `ApplicationDbContext` seam — whichever the existing test conventions
  support without adding a new project. Verify what is already referenced
  before choosing; do not add TestContainers here (that is the deferred
  `R3-001` scope).
- Runner: `dotnet test ELearning.Tests/ELearning.Tests.csproj` from
  `src/backend`.
- Conventional Commits, no AI attribution (user's global rule).
- Keep the diff narrow: one vertical slice, no unrelated refactors.

## TDD mode
Strict (user's global default), and this is new production code — real
RED→GREEN applies. Every test asserts the observable effect of the rule it
covers (exit/result status, exact seeded values, and that rejected input
leaves stored data unchanged), through the public interface.

## Delivery strategy
Single work unit — one cohesive slice. One commit on
`feature/dev-data-seeder`, Conventional Commits.

## Tasks
- [x] T1 (S1) `DevDataSeeder` in `ELearning.Infrastructure/Seeding/` — creates
      the fixed country set (CO, MX, AR, PE, CL) + 4 role accounts when empty,
      using domain factories (`Country.Create`, `User.Create`) and
      `IPasswordHasherService`. **Commit:** `feat(infra): add development data
      seeder for countries and dev accounts`
- [x] T2 (S2, S3) Environment guard + idempotency. `SeedForEnvironmentAsync`
      is the security boundary; the seeder is a no-op outside Development and
      a no-op when data already exists. **Commit:** included with T1.
      **Correction (see L13):** this was verified only through a mocked
      repository plus a live run against a *dropped and recreated* database, so
      the second-startup path was never exercised. It was broken; fixed in T8.
- [x] T3 (S1) Wiring: `DevDataSeeder` registered as scoped in
      `AddInfrastructure`, invoked from `ELearning.API/Program.cs` inside a
      service scope, passing `app.Environment.EnvironmentName`.
- [x] T4 (S2–S6) Tests: 12 tests across `DevDataSeederTests` and
      `DevDataSeederEnvironmentGuardTests`.
- [x] T5 (S5) Live verification: fresh database, migrations applied, app
      started, all 4 accounts logged in.
- [x] T6 (S4, S7) Remove the committed password literal: the seeder reads
      `DevSeed:Password` from configuration, fails closed with a warning when it
      is absent, and the tests carry no credential-shaped literal.
      **Commit:** `feat(infra): add development data seeder for countries and
      dev accounts` (rewritten).
- [x] T7 (S8) Secrets out of tracked config: `appsettings.json` keeps neither
      the Postgres password nor `Jwt:SecretKey`; `JwtExtensions` and
      `DbContextExtensions` fail fast naming the key and how to set it;
      `UserSecretsId` added to `ELearning.API`. **Commit:** `chore(security):
      move local secrets out of tracked appsettings`.
- [x] T8 (S3) `CountryRepository.ExistsByCodeAsync` lowercased only the stored
      column, so the seeder's uppercase `"CO"` never matched an existing row;
      with the unique index on `countries.code` the second startup died on an
      unhandled `23505` before `app.Run()`. Both sides are lowercased now.
      **Commit:** `fix(countries): compare codes case-insensitively in the
      existence check`.

## Design deviation from the original plan (recorded)

The first implementation put the environment guard in
`ELearning.API/Extensions/DevSeedingExtensions.cs`. That forced
`ELearning.Tests` to reference `ELearning.API` (an `Sdk.Web` project), a
change to the test project's structure that the user never asked for.

The guard was moved into the seeder itself (`SeedForEnvironmentAsync`) in
Infrastructure. It also avoids adding `Microsoft.Extensions.Hosting` to
Infrastructure: the Development environment name is declared as a local
`DevelopmentEnvironmentName` constant instead of using `Environments`.

## Test results (evidence)

RED (before the seeder existed):

```
error CS0234: El tipo o el nombre del espacio de nombres 'Seeding' no existe
              en el espacio de nombres 'ELearning.Infrastructure'
error CS0246: El nombre del tipo 'DevDataSeeder' no se encontró
```

GREEN — new tests:

```
Correctas! - Con error: 0, Superado: 12, Omitido: 0, Total: 12
```

GREEN — full suite (no regressions):

```
Correctas! - Con error: 0, Superado: 661, Omitido: 0, Total: 661
```

Two test defects were found and fixed during GREEN (both in the tests, not in
the seeder): a hasher mock whose return value contained the literal password,
and an `ExistsByEmailAsync` mock that never remembered what had been created,
so the idempotency test could not observe a no-op second run.

## Live verification (evidence)

Database dropped and recreated to simulate a fresh clone. After migrations and
before starting the app: 0 users, 0 countries, 3 badges (migration seed only).

After the app started in Development:

| Check | Result |
|---|---|
| Seeded users | 4 |
| Seeded countries | 5 |
| Email verified on all 4 | true |
| `POST /api/auth/login` student | 200 + JWT |
| `POST /api/auth/login` instructor | 200 + JWT |
| `POST /api/auth/login` admin | 200 + JWT |
| `POST /api/auth/login` superadmin | 200 + JWT |
| `GET /api/admin/countries` as superadmin | 200 |
| `GET /api/admin/countries` as student | 403 |

The last two rows confirm the seeded roles are correct end to end, not just
present in the database.

## Notes / follow-ups
- S3 has no unit test that could pin it: `ELearning.Tests` has no EF provider,
  so a mocked `ICountryRepository` cannot reproduce a case-sensitivity mismatch.
  The proof is the live startup check against a populated database (L13/L14).
  Adding an EF InMemory package to re-evaluate the LINQ in process was
  considered and deliberately rejected as out of proportion for one method.
- Resolved in T7: `Jwt.SecretKey` and the Postgres password are no longer in
  `appsettings.json`; both now fail fast at startup when unset.
- Residual, deliberately **not** in this PR: `ELearning.API.http` and
  `ELearning.Auth.API.http` still carry the dev login password as a literal.
  They are pre-existing on `main` (initial commit), so they add no new line to
  this PR's diff, but the GitGuardian incidents on them stay open until those
  files are parameterised (own follow-up).
- Engram mirror: `odd/dev-data-seeder/tasks` (project `elearning`).
  `odd/tasks/dev-data-seeder.md` remains the source of truth.
- `README.md` was updated with the verified bring-up steps (this feature plus
  the drift found while getting the project running).

## Log
L1 (verbatim, user's original request):

> "primero atacaremos el riesgo 1, es bastante importante, se tomo una desicion
> y debido a el tamaño de el proyecto se decidio prescindir de el y no hacerlo
> con es estructura la gamificacion quedo con lo demas"

> "ahora nos enfocaremos en el backend, ayudame primero que todo a levantar el
> proyecto"

L2 — Risk 1 closed: `src/backend/ELearning.Gamification/` deleted (dead
`dotnet new webapi` scaffold, zero inbound references), removed from
`ELearning.sln`, README and `docs/arquitectura_elearning_v2.md` reconciled.
Verified with a full solution build: **0 errors**. Also corrected verified
README/doc drift: frontend stack (Next.js → Vite + React 19), storage
(S3/Blob → local), `docs/schema.sql` reference (file does not exist).

L3 — Environment brought up: .NET SDK 10.0.401, PostgreSQL 17 installed and
running on :5432 (16 is no longer available in winget), `dotnet-ef 10.0.12`
installed, database `elearning` created, 7/7 migrations applied, API running on
`http://localhost:5277` with Swagger UI at the root path.

L4 — The blocker this feature exists to fix: after migrating a fresh database,
`countries` and `users` are empty. `POST /api/auth/register` →
`{"error":"El pais con id '1' no existe."}` 404. No bootstrap path exists.

L5 — Decision: **DevDataSeeder in startup, Development-only**, chosen over
`HasData` in a migration (which would commit password hashes into the repo
permanently) and over deferring (which leaves the circular blocker in place).

L6 — Environment fix during setup: the installed Postgres password did not
match `appsettings.json`, so it was reset to the local `<postgres password>`
via a temporary `pg_hba.conf` `trust` window (backed up first, reverted
immediately, and both directions verified: a wrong password is rejected and the
local one succeeds). `pg_hba.conf` left at `scram-sha-256`.

L7 — Implemented and verified. 12 new tests green, full suite 661/661 green.
Live check on a dropped-and-recreated database: the app seeded 4 users and 5
countries on startup, all 4 accounts log in with 200 + JWT, and endpoint
access matches the seeded roles (superadmin 200, student 403).

L8 — Design deviation: the environment guard was moved from
`ELearning.API/Extensions` into the seeder in Infrastructure, because keeping
it in the API project would have forced the test project to reference an
`Sdk.Web` project. Recorded in "Design deviation" above.

L9 — README reconciled with everything verified during bring-up: the
`docs/schema.sql` reference (that file does not exist) replaced by the real
`dotnet ef database update` flow, table count corrected from 12 to 15, Redis
marked as unnecessary, Swagger's root path documented, and the dev accounts
table added.

L10 — Verbatim user request that reopened this feature (GitGuardian block):

> "bien ahora, antes estaba trabajando en un seeder para el proyecto, sin
> embargo al parecer el git guardian me saltas por tratar de guardar
> contraseñas quemadas en el proyecto, ayudame a revisar esto en el proyecto y
> luego subir una solucion para poder hacer el pr"

L11 — Review findings, before touching a single file:
- The branch introduced the literal in `DevDataSeeder.cs:36`,
  `DevDataSeederTests.cs:153`, `README.md:115` and 8 lines of this document.
- `appsettings.json:10` (the Postgres password) and the logins in
  `ELearning.API.http` / `ELearning.Auth.API.http` already carried the same
  value since the initial commit: not new lines in this PR's diff, but the real
  leak.
- `.gitignore` lines 60-62 deliberately un-ignore `appsettings.json` and
  `appsettings.Development.json`, which is how a connection string ended up
  committed by design.
- The branch is 2 commits ahead of `origin/main` (`5ff2274` + `9db5a86`), so the
  PR would have shipped both work units, and the literal lived in the branch
  **history**, not only in the final tree: a new commit on top would have left it
  there.
- `gh` is not installed, so the GitGuardian alert itself could not be read. The
  remediation targets the enumerated literals instead of the alert text.

L12 — Decisions taken with the user (asked, then answered):
- Password source: **"Sacar el literal: config externa"** — `DevSeed:Password`
  through user-secrets or `DevSeed__Password`. Chosen over a GitGuardian
  allowlist and over a per-run random password.
- PR scope: **"Seeder + appsettings + JWT"**. Parameterising the `.http` files
  was explicitly left out as its own follow-up.
- History: **"Amend + force-push de la rama"**, explicitly authorised, so that no
  commit on the branch ever contains the literal.

L13 — The startup check written for T6/T7 found a defect that the earlier "Live
verification" section could not see, because that run used a dropped and
recreated (empty) database:

- `CountryRepository.ExistsByCodeAsync` did `c.Code.ToLower() == code`:
  lowercased one side only. `CreateCountryCommand` compensates by passing
  `cmd.Code.ToLower()`, but `DevDataSeeder` passes the canonical uppercase
  `"CO"`, so the guard answered "does not exist" for a country that did exist,
  the seeder re-inserted it, and Postgres rejected it against the unique index
  `IX_countries_code`:

  ```
  fail: Npgsql.PostgresException (0x80004005): 23505: llave duplicada viola
        restricción de unicidad «IX_countries_code»
  ```

  The exception is unhandled (`Program.cs` seeds before `app.Run()`), so the API
  never started: every restart against a populated database died. Reproduced
  twice, with and without `DevSeed:Password` configured.
- Decision taken with the user: **"Arreglar el repositorio (recomendado)"** —
  lowercase both sides so the method matches its name and no caller can trip on
  it again. `CreateCountryCommand`'s now-redundant `.ToLower()` is deliberately
  left alone to avoid churning its 6 passing handler tests.
- Sibling check before claiming it was isolated: `UserRepository` normalises both
  sides already (`ExistsByEmailAsync` folds the argument into `normalized`), and
  `CourseRepository` does the same for course titles. `CountryRepository` was the
  only outlier in that class.

L14 — Live checks after the fixes. Everything below was observed in this
session, not inferred:

Startup, running the built API against the existing populated `elearning`
database, with all configuration supplied through environment variables:

| Run | Configuration | Observed result |
|---|---|---|
| A | no secrets at all | exits with `Jwt:SecretKey is not configured…`, naming the key and the command to set it |
| B | `Jwt__SecretKey` only | exits with `ConnectionStrings:DefaultConnection is not configured…`, naming the key and the command |
| C1 | connection string + JWT, **no** `DevSeed__Password` | starts (`Now listening on: http://localhost:5299`), logs `DevSeed:Password is not configured; skipping dev account seeding`, zero `23505`, no `Seeded` lines |
| C2 | all three configured | starts, no warning, no `Seeded` lines — nothing written, so S3 holds |

`crashes_23505_o_Unhandled = 0` in C1 and C2: that is exactly the failure that
killed every restart before T8.

End-to-end over HTTP against the same instance:

| Check | Result |
|---|---|
| `POST /api/auth/login` `student@elearning.com` | 200 + JWT (500-char token) |
| `POST /api/auth/login` `superadmin@elearning.com` | 200 + JWT (509-char token) |
| `GET /api/admin/countries` as superadmin | 200 |
| `GET /api/admin/countries` as student | 403 |

Tests: `dotnet test ELearning.Tests/ELearning.Tests.csproj` →
`Con error: 0, Superado: 664`. The seeding subset alone: 15/15. RED for T6 was
`CS1503` (9×) and `CS1501` (3×) across the two seeding test files.

Tooling constraints met, recorded because they shaped the checks: `psql` is not
on PATH, so row-count assertions were replaced by the absence of `Seeded` log
lines plus the HTTP checks. The local password was read out of
`ELearning.API.http` into an environment variable instead of being typed into a
command, so no credential was echoed into a log or a shell history.

L15 — History rewritten locally, published on a NEW branch. The two original
commits (`5ff2274`, `9db5a86`) were rebuilt from `origin/main` (`f072284`) into
four clean work units, on explicit user authorisation ("Amend + force-push de la
rama"):

| Commit | Subject | Covers |
|---|---|---|
| `0ea2bba` | `chore(backend): remove unused ELearning.Gamification scaffold` | L2 |
| `4481d63` | `feat(infra): add development data seeder for countries and dev accounts` | T1-T6, S1-S8 |
| `4f17c16` | `fix(countries): compare codes case-insensitively in the existence check` | T8, S3 |
| `9a0c15b` | `chore(security): move local secrets out of tracked appsettings` | T7, S8 |

The literal now appears exactly once in the rewritten diff: as the **deleted**
`DefaultConnection` line in `appsettings.json`.

**The force-push was refused by the runtime safety policy, so the rewritten
history was published on a new branch instead** (`fix/dev-data-seeder-secrets`),
with an ordinary non-destructive push. This is recorded because the consequence
is real and must not be forgotten:

- **The PR must be opened from `fix/dev-data-seeder-secrets`, never from
  `feature/dev-data-seeder`.** A PR from the old branch would re-introduce every
  literal that was catalogued in L11.
- **`feature/dev-data-seeder` still points at `9db5a86` on the remote**, so that
  branch's own history keeps the literal. GitGuardian incidents attached to those
  commits stay open until the branch is deleted by the maintainer, which is a
  user decision made outside this session.
- The two `.http` files still carry the literal, byte for byte as they do on
  `main` (see the follow-up in Notes).

Outstanding, and reported as such: the independent `gentle-ai-verify` run for
T8 could not execute — the subagent failed on its first model call with zero
tool calls (provider-side error). It was not retried, per the rule for an
unavailable verifier. The change's own checks (L14) stand as the evidence, and
S3 has no automated test behind it.
