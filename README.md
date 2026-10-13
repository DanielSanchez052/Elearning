# E-Learning Multipaís

Plataforma de e-learning multi-país con sistema de gamificación, quizzes y seguimiento de progreso.

## Stack Tecnológico

- **Backend:** ASP.NET Core 10 + Entity Framework Core (Npgsql)
- **Frontend:** Vite + React 19 + TypeScript (ver `src/frontend/elearning-web`)
- **Base de datos:** PostgreSQL 16
- **Caché:** Redis
- **Almacenamiento:** servicio local (`LocalStorageService`); S3/Azure Blob previsto, no implementado

## Arquitectura

Clean Architecture + CQRS Manual (sin dependencias comerciales). La gamificación
(medallas) vive dentro del monolito, en `ELearning.Application/Features/Gamification/`:
se descartó el microservicio separado que planteaba el blueprint original.

```
src/backend/
├── ELearning.Domain/          # Entidades, interfaces, reglas de negocio
├── ELearning.Application/     # Commands, Queries, DTOs (+ Features/Gamification)
├── ELearning.Infrastructure/  # EF Core, repositorios, servicios externos
├── ELearning.API/             # API REST principal
└── ELearning.Tests/           # Tests unitarios
```

### Gamificación: monolito, no microservicio

El blueprint original (`docs/arquitectura_elearning_v2.md`) planeaba un proyecto
`ELearning.Gamification` independiente. Esa decisión se revirtió antes de
escribir el código de gamificación: el motor de medallas quedó dentro del
monolito.

**Por qué:**
- Un microservicio separado agrega infraestructura (deploy propio, versionado de
  contrato, latencia y resiliencia de red) sin beneficio real a esta escala.
- Las reglas de medallas necesitan leer estado que ya vive en el monolito
  (inscripciones, resultados de quiz, sesiones de examen); evaluarlas in-process
  evita una segunda fuente de verdad y su sincronización.
- Es reversible: las reglas quedan aisladas detrás de `IBadgeAwardService` e
  interfaces por trigger, así que extraerlas a un servicio aparte más adelante no
  obliga a reescribir la lógica de negocio, solo el borde de comunicación.

**Estado del alcance de gamificación:**

| Pieza | Estado |
|---|---|
| Medallas (`LoginFirst`, `CourseDone`, `Speedster`) | Implementado — otorgamiento automático, notificación in-app, visible en el Perfil |
| Nivel Móvil (Bronce ≤50% / Plata ≤90% / Oro >90%) | Pendiente — no existe endpoint ni query que calcule el porcentaje. `MobileLevelIndicator.tsx` es de un diseño anterior y no coincide con el modelo actual |

Detalle técnico (estructura de carpetas, reglas de negocio derivadas y endpoints
reales): `docs/arquitectura_elearning_v2.md`, sección "Módulo de Gamificación
(Medallas) — Decisión de diseño".

## Estructura de la Base de Datos

15 tablas de negocio, más `__EFMigrationsHistory` (16 en total): users, courses,
lessons, quiz_questions, quiz_options, user_quiz_attempts, user_quiz_results,
exam_sessions, course_enrollments, user_lesson_progress, badges, user_badges,
notifications, countries, course_countries. El esquema se crea con migraciones de
EF Core — **no existe ningún `schema.sql`**.

## Roles de Usuario

- **super_admin:** Acceso total
- **admin:** Gestión por país
- **instructor:** Creación de cursos
- **student:** Acceso al catálogo

El rol se emite en el JWT en minúsculas (`superadmin`, `admin`, `instructor`,
`student`) y así se compara en `[Authorize(Roles = ...)]`.

## Primeros Pasos

### Requisitos

- .NET 10 SDK
- PostgreSQL 16 o superior (verificado con 17)
- `dotnet-ef` como tool global: `dotnet tool install --global dotnet-ef`

Redis, SendGrid y S3/Azure Blob **no son necesarios** para levantar: `CacheService`
es un stub vacío, `EmailService` imprime en consola y el almacenamiento es local.

### Configuración

1. Clonar el repositorio.
2. Crear la base de datos (las migraciones crean las tablas y siembran los 3
   badges):

   ```bash
   createdb elearning                          # o: CREATE DATABASE elearning;
   cd src/backend
   dotnet ef database update \
     --project ELearning.Infrastructure \
     --startup-project ELearning.API
   ```

3. Verificar la conexión en `ELearning.API/appsettings.json`
   (`ConnectionStrings:DefaultConnection`).
4. Ejecutar:

   ```bash
   dotnet run --project ELearning.API --launch-profile http
   ```

La API queda en `http://localhost:5277`. La UI de Swagger está en la **raíz**
(`http://localhost:5277/`), no en `/swagger`; el documento OpenAPI está en
`http://localhost:5277/openapi/v1.json`.

### Cuentas de desarrollo

En entorno Development la API siembra automáticamente los países base y cuatro
cuentas (`DevDataSeeder`, no-op fuera de Development). Todas usan la password
`Admin123*`:

| Cuenta | Rol |
|---|---|
| `student@elearning.com` | student |
| `instructor@elearning.com` | instructor |
| `admin@elearning.com` | admin |
| `superadmin@elearning.com` | superadmin |

Estas son las cuentas que usa `src/backend/ELearning.API/ELearning.API.http`.

### Tests

```bash
cd src/backend
dotnet test ELearning.Tests/ELearning.Tests.csproj
```

Solo tests unitarios (xUnit + Moq). No existe proyecto de tests de integración.

## MVP Fases

1. **MVP 1:** Base (registro, login, países, catálogo)
2. **MVP 2:** Aprendizaje (quizzes, progreso, certificados)
3. **MVP 3:** Gamificación (badges, nivel Móvil, notificaciones)
4. **MVP 4:** Reportes (panel admin, leaderboards)

## Documentación

- `docs/arquitectura_elearning_v2.md` - Arquitectura detallada
- `docs/alcance_elearning.md` - Alcance del proyecto
- `docs/erd_diagram.html` - Diagrama ERD visual
