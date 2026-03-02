# E-Learning Multipaís

Plataforma de e-learning multi-país con sistema de gamificación, quizzes y seguimiento de progreso.

## Stack Tecnológico

- **Backend:** ASP.NET Core 10 + Entity Framework Core
- **Frontend:** Next.js 14 (pendiente)
- **Base de datos:** PostgreSQL 16
- **Caché:** Redis
- **Almacenamiento:** S3/Azure Blob

## Arquitectura

Clean Architecture + CQRS Manual (sin dependencias comerciales)

```
src/backend/
├── ELearning.Domain/          # Entidades, interfaces, reglas de negocio
├── ELearning.Application/     # Commands, Queries, DTOs
├── ELearning.Infrastructure/  # EF Core, repositorios, servicios externos
├── ELearning.API/             # API REST principal
├── ELearning.Gamification/   # Microservicio de badges y nivel Móvil
└── ELearning.Tests/          # Tests unitarios e integración
```

## Estructura de la Base de Datos

12 tablas: users, courses, lessons, quiz_questions, quiz_options, course_enrollments, user_lesson_progress, badges, user_badges, notifications, countries, course_countries

Ver `docs/schema.sql` para el script completo.

## Roles de Usuario

- **super_admin:** Acceso total
- **admin:** Gestión por país
- **instructor:** Creación de cursos
- **student:** Acceso al catálogo

## Primeros Pasos

### Requisitos

- .NET 10 SDK
- PostgreSQL 16
- Redis (opcional para caché)

### Configuración

1. Clonar el repositorio
2. Crear la base de datos: `docs/schema.sql`
3. Configurar `appsettings.json` en ELearning.API
4. Ejecutar: `dotnet run --project src/backend/ELearning.API`

## MVP Fases

1. **MVP 1:** Base (registro, login, países, catálogo)
2. **MVP 2:** Aprendizaje (quizzes, progreso, certificados)
3. **MVP 3:** Gamificación (badges, nivel Móvil, notificaciones)
4. **MVP 4:** Reportes (panel admin, leaderboards)

## Documentación

- `docs/arquitectura_elearning_v2.md` - Arquitectura detallada
- `docs/alcance_elearning.md` - Alcance del proyecto
- `docs/schema.sql` - Script de base de datos
- `docs/erd_diagram.html` - Diagrama ERD visual
