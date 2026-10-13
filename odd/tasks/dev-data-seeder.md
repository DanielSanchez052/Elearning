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
  (`student@`, `instructor@`, `admin@`, `superadmin@elearning.com`, password
  `Admin123*`).
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

Seed content to reproduce (from `ELearning.API.http`, verbatim):

> `student@elearning.com` / `Admin123*`
> `instructor@elearning.com` / `Admin123*`
> `superadmin@elearning.com` / `Admin123*`
> `admin@elearning.com` / `Admin123*`

S1. A Development-only seeder must create the base countries and the 4 dev
    accounts when the database is empty, so that a fresh clone + `dotnet ef
    database update` + `dotnet run` yields a usable API.
S2. The seeder must not run and must not write anything outside Development.
S3. The seeder must be idempotent: running it against an already-seeded
    database must not duplicate rows and must not throw.
S4. The seeded passwords must be hashed at runtime with the existing
    `IPasswordHasherService` — no password hash literal may be committed.
S5. After seeding, `POST /api/auth/login` with `student@elearning.com` /
    `Admin123*` must return 200 with a JWT, and the 4 accounts must carry
    their documented roles (student, instructor, admin, superadmin).
S6. Seeded accounts must be usable without the email-verification step (they
    existed and worked before, so `IsEmailVerified` must be true).

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
- Changing the `Jwt.SecretKey` or the connection-string secret currently
  committed in `appsettings.json`. Flagged separately as a security follow-up,
  not part of this feature.

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
- [x] T3 (S1) Wiring: `DevDataSeeder` registered as scoped in
      `AddInfrastructure`, invoked from `ELearning.API/Program.cs` inside a
      service scope, passing `app.Environment.EnvironmentName`.
- [x] T4 (S2–S6) Tests: 12 tests across `DevDataSeederTests` and
      `DevDataSeederEnvironmentGuardTests`.
- [x] T5 (S5) Live verification: fresh database, migrations applied, app
      started, all 4 accounts logged in.

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
- The `Jwt.SecretKey` and the database password are still committed in
  `appsettings.json`. Not part of this feature; flagged as a security
  follow-up.
- No Engram mirror was written: no memory tooling is available in this
  session. `odd/tasks/dev-data-seeder.md` is the source of truth.
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
match `appsettings.json`, so it was reset to `Admin123*` via a temporary
`pg_hba.conf` `trust` window (backed up first, reverted immediately, and both
directions verified: a wrong password is rejected and `Admin123*` succeeds).
`pg_hba.conf` left at `scram-sha-256`.

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
