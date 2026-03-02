**DOCUMENTO DE ARQUITECTURA**

**Plataforma E-Learning Multipaís**

*Clean Architecture · CQRS Manual · Sin dependencias comerciales ·
Monorepo*

Versión 1.0 \| 2025

**1. Introducción y Justificación**

Este documento define la arquitectura del proyecto, la estructura de
carpetas del monorepo y las convenciones de organización del código que
se aplicarán a lo largo de todo el desarrollo. Su objetivo es que
cualquier decisión de dónde colocar un archivo nuevo sea obvia, sin
necesidad de discutirlo.

**1.1 Patrón elegido: Clean Architecture + CQRS Manual**

Se adopta Clean Architecture como patrón principal del backend por tres
razones concretas para este proyecto:

-   El código queda organizado por funcionalidad de negocio
    (autenticación, cursos, gamificación) en lugar de por tipo de
    archivo (todos los controladores juntos, todos los servicios
    juntos). Esto hace que encontrar código sea inmediato.

-   La capa de dominio no depende de ninguna librería externa. Si en el
    futuro cambias de EF Core a Dapper, o de PostgreSQL a otro motor, el
    núcleo del negocio no se toca.

-   CQRS manual elimina la necesidad de inyectar múltiples servicios en
    cada controlador. Cada operación tiene su propio Handler registrado
    en el DI nativo de .NET, sin intermediarios de terceros.

**1.2 Por qué no usamos MediatR ni FluentValidation**

MediatR pasó a licencia comercial desde la versión 12: proyectos de uso
comercial requieren una licencia paga. FluentValidation es Apache 2.0,
pero dado que ya eliminamos MediatR, optar por validación nativa
mantiene el stack libre de dependencias externas no esenciales.

Sus reemplazos son 100% nativos de .NET y cubren exactamente los mismos
casos de uso:

  -----------------------------------------------------------------------
  **Librería reemplazada**    **Reemplazo nativo en .NET**
  --------------------------- -------------------------------------------
  MediatR (IRequest / Send)   Interfaces propias ICommandHandler\<T\> /
                              IQueryHandler\<T,R\> + DI nativo de
                              Microsoft.Extensions.DependencyInjection

  MediatR (IPipelineBehavior) Decoradores manuales registrados en DI:
                              LoggingDecorator\<T\>,
                              ValidationDecorator\<T\>

  FluentValidation            Data Annotations (\[Required\],
                              \[MaxLength\]) para casos simples + clases
                              Validator\<T\> propias para reglas
                              complejas

  AutoMapper                  Métodos de extensión estáticos ToDto() /
                              ToDomain() en cada entidad. Cero librerías
                              externas.
  -----------------------------------------------------------------------

  -----------------------------------------------------------------------
  💡 Regla de oro de Clean Architecture: las dependencias siempre apuntan
  hacia adentro. La capa Domain no conoce Application. Application no
  conoce Infrastructure. Infrastructure no conoce API. Nunca al revés.

  -----------------------------------------------------------------------

**1.2 Monorepo**

Backend y frontend viven en el mismo repositorio bajo carpetas
separadas. Esto simplifica el flujo de trabajo de una sola persona: un
solo git clone, un solo CI/CD, y los cambios que afectan a ambos lados
quedan en el mismo commit.

**2. Visión General de las Capas**

El backend se divide en cuatro proyectos .NET independientes más un
microservicio de gamificación. El frontend es un proyecto Next.js
independiente. Todos conviven en el monorepo.

  -------------------------------------------------------------------------
  **Proyecto / Capa**        **Responsabilidad**
  -------------------------- ----------------------------------------------
  ELearning.Domain           Entidades, enumeraciones, interfaces de
                             repositorio, reglas de negocio puras. Sin
                             dependencias externas.

  ELearning.Application      Casos de uso (Commands y Queries con handlers
                             manuales), DTOs, interfaces de servicios.
                             Depende solo de Domain. Sin librerías de
                             terceros.

  ELearning.Infrastructure   Implementación de EF Core, repositorios,
                             servicios externos (S3, Redis, SendGrid).
                             Depende de Application.

  ELearning.API              Controladores, middlewares, configuración de
                             DI, Swagger. Punto de entrada HTTP. Depende de
                             Application e Infrastructure.

  ELearning.Gamification     Microservicio independiente en C#. Motor de
                             medallas y nivel Móvil. Se comunica con la API
                             principal vía HTTP.

  elearning-web              Proyecto Next.js 14. Interfaz de usuario
                             responsive. Se comunica con ELearning.API y
                             Gamification vía REST.
  -------------------------------------------------------------------------

**3. Estructura Completa del Monorepo**

A continuación se muestra el árbol de carpetas completo. Las carpetas
marcadas con 📁 son directorios y las marcadas con 📄 son archivos
clave.

**3.1 Raíz del repositorio**

+-----------------------------------------------------------------------+
| 📁 elearning-platform/ ← raíz del monorepo                            |
|                                                                       |
| 📁 src/ ← todo el código fuente                                       |
|                                                                       |
| │ 📁 backend/ ← solución .NET completa                                |
|                                                                       |
| │ 📁 frontend/ ← proyecto Next.js                                     |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 docs/ ← documentación del proyecto                                 |
|                                                                       |
| │ 📄 alcance.docx                                                     |
|                                                                       |
| │ 📄 arquitectura.docx ← este documento                               |
|                                                                       |
| │ 📄 schema.sql                                                       |
|                                                                       |
| │ 📄 erd_diagram.html                                                 |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 .github/                                                           |
|                                                                       |
| │ 📁 workflows/                                                       |
|                                                                       |
| │ 📄 ci-backend.yml ← build + tests del backend                       |
|                                                                       |
| │ 📄 ci-frontend.yml ← build + lint del frontend                      |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 .gitignore                                                         |
|                                                                       |
| 📄 README.md                                                          |
+-----------------------------------------------------------------------+

**3.2 Backend --- Solución .NET (src/backend/)**

La solución .NET contiene cinco proyectos. La dependencia entre ellos
sigue la regla de Clean Architecture: siempre hacia adentro.

+-----------------------------------------------------------------------+
| 📁 src/backend/                                                       |
|                                                                       |
| 📄 ELearning.sln ← solución que agrupa todos los proyectos            |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 ELearning.Domain/ ← Capa 1: núcleo del negocio (sin deps externas) |
|                                                                       |
| 📁 ELearning.Application/ ← Capa 2: casos de uso                      |
|                                                                       |
| 📁 ELearning.Infrastructure/ ← Capa 3: implementaciones técnicas      |
|                                                                       |
| 📁 ELearning.API/ ← Capa 4: punto de entrada HTTP                     |
|                                                                       |
| 📁 ELearning.Gamification/ ← Microservicio de gamificación            |
|                                                                       |
| 📁 ELearning.Tests/ ← Tests unitarios e integración                   |
+-----------------------------------------------------------------------+

**3.2.1 ELearning.Domain**

+-----------------------------------------------------------------------+
| 📁 ELearning.Domain/                                                  |
|                                                                       |
| 📁 Entities/ ← clases que mapean a tablas de la BD                    |
|                                                                       |
| │ 📄 User.cs                                                          |
|                                                                       |
| │ 📄 Country.cs                                                       |
|                                                                       |
| │ 📄 Course.cs                                                        |
|                                                                       |
| │ 📄 CourseCountry.cs                                                 |
|                                                                       |
| │ 📄 Lesson.cs                                                        |
|                                                                       |
| │ 📄 QuizQuestion.cs                                                  |
|                                                                       |
| │ 📄 QuizOption.cs                                                    |
|                                                                       |
| │ 📄 CourseEnrollment.cs                                              |
|                                                                       |
| │ 📄 UserLessonProgress.cs                                            |
|                                                                       |
| │ 📄 Badge.cs                                                         |
|                                                                       |
| │ 📄 UserBadge.cs                                                     |
|                                                                       |
| │ 📄 Notification.cs                                                  |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Enums/ ← enumeraciones del dominio                                 |
|                                                                       |
| │ 📄 UserRole.cs                                                      |
|                                                                       |
| │ 📄 LessonType.cs                                                    |
|                                                                       |
| │ 📄 NotificationType.cs                                              |
|                                                                       |
| │ 📄 BadgeCode.cs                                                     |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Interfaces/ ← contratos que Infrastructure debe implementar        |
|                                                                       |
| │ 📁 Repositories/                                                    |
|                                                                       |
| │ │ 📄 IUserRepository.cs                                             |
|                                                                       |
| │ │ 📄 ICourseRepository.cs                                           |
|                                                                       |
| │ │ 📄 IEnrollmentRepository.cs                                       |
|                                                                       |
| │ │ 📄 IBadgeRepository.cs                                            |
|                                                                       |
| │ │ 📄 INotificationRepository.cs                                     |
|                                                                       |
| │ 📁 Services/                                                        |
|                                                                       |
| │ 📄 IStorageService.cs ← contrato para S3 / Azure Blob               |
|                                                                       |
| │ 📄 IEmailService.cs ← contrato para SendGrid / SES                  |
|                                                                       |
| │ 📄 ICacheService.cs ← contrato para Redis                           |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Exceptions/ ← excepciones de dominio tipadas                       |
|                                                                       |
| │ 📄 DomainException.cs                                               |
|                                                                       |
| │ 📄 NotFoundException.cs                                             |
|                                                                       |
| │ 📄 ForbiddenException.cs                                            |
|                                                                       |
| │ 📄 ValidationException.cs                                           |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 ELearning.Domain.csproj                                            |
+-----------------------------------------------------------------------+

**3.2.2 ELearning.Application**

Aquí vive toda la lógica de negocio organizada por feature. Cada feature
tiene su propia carpeta con Commands, Queries y DTOs. MediatR conecta
los controladores con estos handlers.

+-----------------------------------------------------------------------+
| 📁 ELearning.Application/                                             |
|                                                                       |
| 📁 Features/ ← organizado por funcionalidad de negocio                |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Auth/                                                            |
|                                                                       |
| │ │ 📁 Commands/                                                      |
|                                                                       |
| │ │ │ 📄 RegisterUserCommand.cs ← Command + Handler en el mismo       |
| archivo                                                               |
|                                                                       |
| │ │ │ 📄 LoginCommand.cs                                              |
|                                                                       |
| │ │ │ 📄 VerifyEmailCommand.cs                                        |
|                                                                       |
| │ │ │ 📄 ResetPasswordCommand.cs                                      |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ │ 📄 GetCurrentUserQuery.cs                                       |
|                                                                       |
| │ │ 📁 DTOs/                                                          |
|                                                                       |
| │ │ 📄 AuthResponseDto.cs                                             |
|                                                                       |
| │ │ 📄 LoginRequestDto.cs                                             |
|                                                                       |
| │ │ 📄 RegisterRequestDto.cs                                          |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Courses/                                                         |
|                                                                       |
| │ │ 📁 Commands/                                                      |
|                                                                       |
| │ │ │ 📄 CreateCourseCommand.cs                                       |
|                                                                       |
| │ │ │ 📄 UpdateCourseCommand.cs                                       |
|                                                                       |
| │ │ │ 📄 PublishCourseCommand.cs                                      |
|                                                                       |
| │ │ │ 📄 AssignCourseToCountriesCommand.cs                            |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ │ 📄 GetCourseByIdQuery.cs                                        |
|                                                                       |
| │ │ │ 📄 GetCourseCatalogQuery.cs ← filtra por país del usuario       |
|                                                                       |
| │ │ 📁 DTOs/                                                          |
|                                                                       |
| │ │ 📄 CourseDto.cs                                                   |
|                                                                       |
| │ │ 📄 CourseSummaryDto.cs                                            |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Lessons/                                                         |
|                                                                       |
| │ │ 📁 Commands/                                                      |
|                                                                       |
| │ │ │ 📄 CreateLessonCommand.cs                                       |
|                                                                       |
| │ │ │ 📄 UploadLessonMediaCommand.cs                                  |
|                                                                       |
| │ │ │ 📄 CompleteLessonCommand.cs                                     |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ 📄 GetLessonsByCoursQuery.cs                                      |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Quizzes/                                                         |
|                                                                       |
| │ │ 📁 Commands/                                                      |
|                                                                       |
| │ │ │ 📄 CreateQuizQuestionCommand.cs                                 |
|                                                                       |
| │ │ │ 📄 SubmitQuizAnswerCommand.cs                                   |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ │ 📄 GetQuizByLessonQuery.cs                                      |
|                                                                       |
| │ │ 📁 DTOs/                                                          |
|                                                                       |
| │ │ 📄 QuizSubmitDto.cs                                               |
|                                                                       |
| │ │ 📄 QuizResultDto.cs                                               |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Enrollments/                                                     |
|                                                                       |
| │ │ 📁 Commands/                                                      |
|                                                                       |
| │ │ │ 📄 EnrollUserCommand.cs                                         |
|                                                                       |
| │ │ │ 📄 CompleteCourseCommand.cs                                     |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ 📄 GetUserEnrollmentsQuery.cs                                     |
|                                                                       |
| │ │ 📄 GetCourseProgressQuery.cs                                      |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Certificates/                                                    |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ 📄 GenerateCertificateQuery.cs                                    |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Notifications/                                                   |
|                                                                       |
| │ │ 📁 Commands/                                                      |
|                                                                       |
| │ │ │ 📄 CreateNotificationCommand.cs                                 |
|                                                                       |
| │ │ │ 📄 MarkNotificationReadCommand.cs                               |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ 📄 GetUserNotificationsQuery.cs                                   |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Reports/                                                         |
|                                                                       |
| │ │ 📁 Queries/                                                       |
|                                                                       |
| │ │ │ 📄 GetUserProgressReportQuery.cs                                |
|                                                                       |
| │ │ │ 📄 GetCountryReportQuery.cs                                     |
|                                                                       |
| │ │ │ 📄 GetLeaderboardQuery.cs                                       |
|                                                                       |
| │ │ 📁 DTOs/                                                          |
|                                                                       |
| │ │ 📄 UserProgressReportDto.cs                                       |
|                                                                       |
| │ │ 📄 CountryReportDto.cs                                            |
|                                                                       |
| │ │ 📄 LeaderboardEntryDto.cs                                         |
|                                                                       |
| │                                                                     |
|                                                                       |
| │ 📁 Admin/                                                           |
|                                                                       |
| │ 📁 Commands/                                                        |
|                                                                       |
| │ │ 📄 CreateCountryCommand.cs                                        |
|                                                                       |
| │ │ 📄 AssignAdminToCountryCommand.cs                                 |
|                                                                       |
| │ 📁 Queries/                                                         |
|                                                                       |
| │ 📄 GetUsersQuery.cs                                                 |
|                                                                       |
| │ 📄 GetCountryStatsQuery.cs                                          |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Common/ ← utilidades compartidas por todos los features            |
|                                                                       |
| │ 📁 Abstractions/ ← interfaces del patrón CQRS manual                |
|                                                                       |
| │ │ 📄 ICommandHandler.cs ← interface ICommandHandler\<TCommand\>     |
|                                                                       |
| │ │ 📄 IQueryHandler.cs ← interface IQueryHandler\<TQuery, TResult\>  |
|                                                                       |
| │ │ 📄 ICommand.cs ← marker interface para Commands                   |
|                                                                       |
| │ │ 📄 IQuery.cs ← marker interface para Queries                      |
|                                                                       |
| │ 📁 Decorators/ ← equivalente a IPipelineBehavior de MediatR         |
|                                                                       |
| │ │ 📄 LoggingDecorator.cs ← loguea automáticamente cada operación    |
|                                                                       |
| │ │ 📄 ValidationDecorator.cs ← valida antes de ejecutar el handler   |
|                                                                       |
| │ 📁 Validators/ ← validadores propios (sin FluentValidation)         |
|                                                                       |
| │ │ 📄 IValidator.cs ← interface IValidator\<T\>                      |
|                                                                       |
| │ │ 📄 ValidationResult.cs ← resultado con lista de errores           |
|                                                                       |
| │ │ 📄 RegisterUserValidator.cs                                       |
|                                                                       |
| │ │ 📄 CreateCourseValidator.cs                                       |
|                                                                       |
| │ 📁 Mappings/ ← métodos de extensión ToDto() / ToDomain()            |
|                                                                       |
| │ 📄 UserMappings.cs                                                  |
|                                                                       |
| │ 📄 CourseMappings.cs                                                |
|                                                                       |
| │ 📄 EnrollmentMappings.cs                                            |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 DependencyInjection.cs ← registra todos los handlers en el DI      |
| nativo                                                                |
|                                                                       |
| 📄 ELearning.Application.csproj                                       |
+-----------------------------------------------------------------------+

**3.2.3 ELearning.Infrastructure**

+-----------------------------------------------------------------------+
| 📁 ELearning.Infrastructure/                                          |
|                                                                       |
| 📁 Persistence/                                                       |
|                                                                       |
| │ 📁 Configurations/ ← configuraciones de EF Core por entidad         |
|                                                                       |
| │ │ 📄 UserConfiguration.cs                                           |
|                                                                       |
| │ │ 📄 CourseConfiguration.cs                                         |
|                                                                       |
| │ │ 📄 LessonConfiguration.cs                                         |
|                                                                       |
| │ │ 📄 EnrollmentConfiguration.cs                                     |
|                                                                       |
| │ │ ⚙ \... una por cada entidad                                       |
|                                                                       |
| │ 📁 Migrations/ ← generadas con dotnet ef migrations add             |
|                                                                       |
| │ 📁 Repositories/ ← implementan las interfaces de Domain             |
|                                                                       |
| │ │ 📄 UserRepository.cs                                              |
|                                                                       |
| │ │ 📄 CourseRepository.cs                                            |
|                                                                       |
| │ │ 📄 EnrollmentRepository.cs                                        |
|                                                                       |
| │ │ 📄 BadgeRepository.cs                                             |
|                                                                       |
| │ │ 📄 NotificationRepository.cs                                      |
|                                                                       |
| │ 📄 AppDbContext.cs ← DbContext principal con todos los DbSets       |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Services/ ← implementan las interfaces de Domain                   |
|                                                                       |
| │ 📄 S3StorageService.cs ← implementa IStorageService                 |
|                                                                       |
| │ 📄 SendGridEmailService.cs ← implementa IEmailService               |
|                                                                       |
| │ 📄 RedisCacheService.cs ← implementa ICacheService                  |
|                                                                       |
| │ 📄 JwtTokenService.cs                                               |
|                                                                       |
| │ 📄 CertificatePdfService.cs ← genera PDFs con QuestPDF              |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 DependencyInjection.cs ← registra EF Core, Redis, S3, etc.         |
|                                                                       |
| 📄 ELearning.Infrastructure.csproj                                    |
+-----------------------------------------------------------------------+

**3.2.4 ELearning.API**

+-----------------------------------------------------------------------+
| 📁 ELearning.API/                                                     |
|                                                                       |
| 📁 Controllers/ ← delgados: reciben, invocan el handler vía DI,       |
| devuelven                                                             |
|                                                                       |
| │ 📄 AuthController.cs                                                |
|                                                                       |
| │ 📄 CoursesController.cs                                             |
|                                                                       |
| │ 📄 LessonsController.cs                                             |
|                                                                       |
| │ 📄 QuizzesController.cs                                             |
|                                                                       |
| │ 📄 EnrollmentsController.cs                                         |
|                                                                       |
| │ 📄 CertificatesController.cs                                        |
|                                                                       |
| │ 📄 NotificationsController.cs                                       |
|                                                                       |
| │ 📄 ReportsController.cs                                             |
|                                                                       |
| │ 📄 AdminController.cs                                               |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Hubs/ ← SignalR hubs para notificaciones en tiempo real            |
|                                                                       |
| │ 📄 NotificationHub.cs                                               |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Middleware/ ← middlewares globales de la API                       |
|                                                                       |
| │ 📄 ExceptionHandlingMiddleware.cs ← captura todas las excepciones   |
| del dominio                                                           |
|                                                                       |
| │ 📄 RequestLoggingMiddleware.cs                                      |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Extensions/ ← métodos de extensión de IServiceCollection           |
|                                                                       |
| │ 📄 SwaggerExtensions.cs                                             |
|                                                                       |
| │ 📄 AuthExtensions.cs                                                |
|                                                                       |
| │ 📄 CorsExtensions.cs                                                |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 Program.cs ← entry point, registra todo el DI                      |
|                                                                       |
| 📄 appsettings.json                                                   |
|                                                                       |
| 📄 appsettings.Development.json ← secrets locales (en .gitignore)     |
|                                                                       |
| 📄 ELearning.API.csproj                                               |
+-----------------------------------------------------------------------+

**3.2.5 ELearning.Gamification (Microservicio)**

+-----------------------------------------------------------------------+
| 📁 ELearning.Gamification/                                            |
|                                                                       |
| 📁 Controllers/                                                       |
|                                                                       |
| │ 📄 BadgesController.cs ← endpoints internos llamados por la API     |
| principal                                                             |
|                                                                       |
| │ 📄 MobileLevelController.cs                                         |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Rules/ ← una clase por regla de medalla                            |
|                                                                       |
| │ 📄 IBadgeRule.cs ← interfaz que todas las reglas implementan        |
|                                                                       |
| │ 📄 FirstLoginRule.cs                                                |
|                                                                       |
| │ 📄 LoginStreakRule.cs                                               |
|                                                                       |
| │ 📄 CourseCompletedRule.cs                                           |
|                                                                       |
| │ 📄 SpeedsterRule.cs                                                 |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Services/                                                          |
|                                                                       |
| │ 📄 BadgeEvaluationService.cs ← evalúa todas las reglas y otorga     |
| medallas                                                              |
|                                                                       |
| │ 📄 MobileLevelService.cs ← calcula % con apoyo de Redis             |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 Program.cs                                                         |
|                                                                       |
| 📄 appsettings.json                                                   |
|                                                                       |
| 📄 ELearning.Gamification.csproj                                      |
+-----------------------------------------------------------------------+

**3.2.6 ELearning.Tests**

+-----------------------------------------------------------------------+
| 📁 ELearning.Tests/                                                   |
|                                                                       |
| 📁 Unit/                                                              |
|                                                                       |
| │ 📁 Application/ ← tests de handlers de MediatR (sin BD)             |
|                                                                       |
| │ │ 📄 RegisterUserHandlerTests.cs                                    |
|                                                                       |
| │ │ 📄 CreateCourseHandlerTests.cs                                    |
|                                                                       |
| │ │ 📄 SubmitQuizHandlerTests.cs                                      |
|                                                                       |
| │ 📁 Domain/ ← tests de lógica pura del dominio                       |
|                                                                       |
| │ 📄 SpeedsterRuleTests.cs                                            |
|                                                                       |
| │ 📄 MobileLevelTests.cs                                              |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 Integration/ ← tests con BD real en memoria o TestContainers       |
|                                                                       |
| │ 📄 CoursesApiTests.cs                                               |
|                                                                       |
| │ 📄 AuthApiTests.cs                                                  |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 ELearning.Tests.csproj                                             |
+-----------------------------------------------------------------------+

**3.3 Frontend --- Next.js (src/frontend/)**

+-----------------------------------------------------------------------+
| 📁 src/frontend/ ← proyecto Next.js 14 con App Router                 |
|                                                                       |
| 📁 app/ ← rutas de Next.js (App Router)                               |
|                                                                       |
| │ 📁 (public)/ ← rutas sin autenticación                              |
|                                                                       |
| │ │ 📁 login/                                                         |
|                                                                       |
| │ │ │ 📄 page.tsx                                                     |
|                                                                       |
| │ │ 📁 register/                                                      |
|                                                                       |
| │ │ 📄 page.tsx                                                       |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📁 (protected)/ ← rutas que requieren login                         |
|                                                                       |
| │ │ 📁 dashboard/                                                     |
|                                                                       |
| │ │ │ 📄 page.tsx                                                     |
|                                                                       |
| │ │ 📁 courses/                                                       |
|                                                                       |
| │ │ │ 📄 page.tsx ← catálogo                                          |
|                                                                       |
| │ │ │ 📁 \[courseId\]/                                                |
|                                                                       |
| │ │ │ 📄 page.tsx ← detalle del curso                                 |
|                                                                       |
| │ │ │ 📁 lessons/                                                     |
|                                                                       |
| │ │ │ 📁 \[lessonId\]/                                                |
|                                                                       |
| │ │ │ 📄 page.tsx ← lección individual                                |
|                                                                       |
| │ │ 📁 profile/                                                       |
|                                                                       |
| │ │ │ 📄 page.tsx ← perfil + medallas + certificados                  |
|                                                                       |
| │ │ 📁 notifications/                                                 |
|                                                                       |
| │ │ 📄 page.tsx                                                       |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📁 (admin)/ ← rutas exclusivas de admin/superadmin                  |
|                                                                       |
| │ │ 📁 admin/                                                         |
|                                                                       |
| │ │ │ 📁 courses/                                                     |
|                                                                       |
| │ │ │ │ 📄 page.tsx ← gestión de cursos                               |
|                                                                       |
| │ │ │ 📁 users/                                                       |
|                                                                       |
| │ │ │ │ 📄 page.tsx                                                   |
|                                                                       |
| │ │ │ 📁 reports/                                                     |
|                                                                       |
| │ │ │ 📄 page.tsx                                                     |
|                                                                       |
| │ │ 📄 layout.tsx ← layout con guard de rol admin                     |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📄 layout.tsx ← layout raíz (fuentes, providers globales)           |
|                                                                       |
| │ 📄 not-found.tsx                                                    |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 components/ ← componentes reutilizables                            |
|                                                                       |
| │ 📁 ui/ ← componentes base sin lógica de negocio                     |
|                                                                       |
| │ │ 📄 Button.tsx                                                     |
|                                                                       |
| │ │ 📄 Card.tsx                                                       |
|                                                                       |
| │ │ 📄 Modal.tsx                                                      |
|                                                                       |
| │ │ 📄 ProgressBar.tsx                                                |
|                                                                       |
| │ │ 📄 Badge.tsx                                                      |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📁 courses/ ← componentes específicos de cursos                     |
|                                                                       |
| │ │ 📄 CourseCard.tsx                                                 |
|                                                                       |
| │ │ 📄 CourseCatalog.tsx                                              |
|                                                                       |
| │ │ 📄 VideoPlayer.tsx                                                |
|                                                                       |
| │ │ 📄 PdfViewer.tsx                                                  |
|                                                                       |
| │ │ 📄 LessonSidebar.tsx                                              |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📁 quiz/                                                            |
|                                                                       |
| │ │ 📄 QuizQuestion.tsx                                               |
|                                                                       |
| │ │ 📄 QuizResult.tsx                                                 |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📁 gamification/                                                    |
|                                                                       |
| │ │ 📄 BadgeCard.tsx                                                  |
|                                                                       |
| │ │ 📄 MobileLevelIndicator.tsx                                       |
|                                                                       |
| │ │                                                                   |
|                                                                       |
| │ 📁 layout/                                                          |
|                                                                       |
| │ 📄 Navbar.tsx                                                       |
|                                                                       |
| │ 📄 Sidebar.tsx                                                      |
|                                                                       |
| │ 📄 NotificationBell.tsx                                             |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 lib/ ← lógica compartida del frontend                              |
|                                                                       |
| │ 📄 api.ts ← cliente Axios con baseURL e interceptores               |
|                                                                       |
| │ 📄 auth.ts ← helpers de JWT (leer, guardar, expiración)             |
|                                                                       |
| │ 📄 utils.ts                                                         |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 hooks/ ← custom hooks de React                                     |
|                                                                       |
| │ 📄 useAuth.ts                                                       |
|                                                                       |
| │ 📄 useCourseProgress.ts                                             |
|                                                                       |
| │ 📄 useNotifications.ts                                              |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 store/ ← estado global con Zustand                                 |
|                                                                       |
| │ 📄 authStore.ts ← usuario, token, rol                               |
|                                                                       |
| │ 📄 notificationStore.ts                                             |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📁 types/ ← tipos TypeScript compartidos                              |
|                                                                       |
| │ 📄 course.types.ts                                                  |
|                                                                       |
| │ 📄 user.types.ts                                                    |
|                                                                       |
| │ 📄 quiz.types.ts                                                    |
|                                                                       |
| │                                                                     |
|                                                                       |
| 📄 next.config.ts                                                     |
|                                                                       |
| 📄 tailwind.config.ts                                                 |
|                                                                       |
| 📄 tsconfig.json                                                      |
|                                                                       |
| 📄 package.json                                                       |
+-----------------------------------------------------------------------+

**4. Convenciones de Código**

**4.1 Anatomía de un Command (CQRS Manual)**

Cada operación de escritura es un Command. El Command, su Validator y su
Handler viven en el mismo archivo para mantener cohesión. No se usa
ninguna librería externa: todo el patrón está definido con interfaces
propias y el DI nativo de .NET.

+-----------------------------------------------------------------------+
| \# Ejemplo: Features/Courses/Commands/CreateCourseCommand.cs          |
|                                                                       |
| \# 1. El Record que define los datos de entrada (implementa ICommand) |
|                                                                       |
| public record CreateCourseCommand(                                    |
|                                                                       |
| string Title, string Description,                                     |
|                                                                       |
| bool IsGlobal, int\[\] CountryIds, Guid CreatedBy) : ICommand;        |
|                                                                       |
| \# 2. El Validator propio (implementa IValidator\<T\>, sin            |
| FluentValidation)                                                     |
|                                                                       |
| public class CreateCourseValidator :                                  |
| IValidator\<CreateCourseCommand\>                                     |
|                                                                       |
| {                                                                     |
|                                                                       |
| public ValidationResult Validate(CreateCourseCommand cmd)             |
|                                                                       |
| {                                                                     |
|                                                                       |
| var errors = new List\<string\>();                                    |
|                                                                       |
| if (string.IsNullOrWhiteSpace(cmd.Title))                             |
|                                                                       |
| errors.Add(\'Title es requerido\');                                   |
|                                                                       |
| if (cmd.Title?.Length \> 200)                                         |
|                                                                       |
| errors.Add(\'Title no puede superar 200 caracteres\');                |
|                                                                       |
| return new ValidationResult(errors);                                  |
|                                                                       |
| }                                                                     |
|                                                                       |
| }                                                                     |
|                                                                       |
| \# 3. El Handler que ejecuta la lógica (implementa                    |
| ICommandHandler\<T\>)                                                 |
|                                                                       |
| public class CreateCourseHandler :                                    |
| ICommandHandler\<CreateCourseCommand, Guid\>                          |
|                                                                       |
| {                                                                     |
|                                                                       |
| private readonly ICourseRepository \_courses;                         |
|                                                                       |
| public CreateCourseHandler(ICourseRepository courses) =\> \_courses = |
| courses;                                                              |
|                                                                       |
| public async Task\<Guid\> HandleAsync(CreateCourseCommand cmd,        |
|                                                                       |
| CancellationToken ct)                                                 |
|                                                                       |
| {                                                                     |
|                                                                       |
| var course = new Course(cmd.Title, cmd.Description, cmd.CreatedBy);   |
|                                                                       |
| await \_courses.CreateAsync(course, ct);                              |
|                                                                       |
| return course.Id;                                                     |
|                                                                       |
| }                                                                     |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

**4.2 Cómo se conecta el Handler al Controlador**

En lugar de MediatR como intermediario, el controlador inyecta el
handler directamente a través de la interface ICommandHandler\<T\>. El
DI de .NET resuelve la implementación concreta en runtime. El
ValidationDecorator se registra como wrapper del handler real, por lo
que la validación ocurre de forma transparente antes de llegar al
handler.

+-----------------------------------------------------------------------+
| \# DependencyInjection.cs (en Application)                            |
|                                                                       |
| services.AddScoped\<ICommandHandler\<CreateCourseCommand, Guid\>,     |
|                                                                       |
| ValidationDecorator\<CreateCourseCommand, Guid\>\>();                 |
|                                                                       |
| \# El decorator envuelve al handler real:                             |
|                                                                       |
| services.AddScoped\<CreateCourseHandler\>();                          |
|                                                                       |
| services.AddScoped\<IValidator\<CreateCourseCommand\>,                |
| CreateCourseValidator\>();                                            |
|                                                                       |
| \# Controllers/CoursesController.cs                                   |
|                                                                       |
| \[ApiController\]                                                     |
|                                                                       |
| \[Route(\'api/\[controller\]\')\]                                     |
|                                                                       |
| public class CoursesController(                                       |
|                                                                       |
| ICommandHandler\<CreateCourseCommand, Guid\> createHandler,           |
|                                                                       |
| IQueryHandler\<GetCourseByIdQuery, CourseDto\> getByIdHandler         |
|                                                                       |
| ) : ControllerBase                                                    |
|                                                                       |
| {                                                                     |
|                                                                       |
| \[HttpPost\]                                                          |
|                                                                       |
| \[Authorize(Roles = \'instructor,admin,super_admin\')\]               |
|                                                                       |
| public async Task\<IActionResult\> Create(\[FromBody\]                |
| CreateCourseCommand cmd)                                              |
|                                                                       |
| =\> Ok(await createHandler.HandleAsync(cmd,                           |
| HttpContext.RequestAborted));                                         |
|                                                                       |
| \[HttpGet(\'{id}\')\]                                                 |
|                                                                       |
| public async Task\<IActionResult\> GetById(Guid id)                   |
|                                                                       |
| =\> Ok(await getByIdHandler.HandleAsync(                              |
|                                                                       |
| new GetCourseByIdQuery(id), HttpContext.RequestAborted));             |
|                                                                       |
| }                                                                     |
+-----------------------------------------------------------------------+

**4.3 Naming Conventions**

  -----------------------------------------------------------------------
  **Elemento**             **Convención**
  ------------------------ ----------------------------------------------
  Commands                 Verbo + Sustantivo + Command →
                           CreateCourseCommand, EnrollUserCommand

  Queries                  Get + Sustantivo + Query → GetCourseByIdQuery,
                           GetLeaderboardQuery

  DTOs                     Sustantivo + Dto → CourseDto,
                           UserProgressReportDto

  Handlers                 Mismo nombre que el Command/Query con sufijo
                           Handler

  Repositorios (iface)     I + Sustantivo + Repository →
                           ICourseRepository

  Repositorios (impl)      Sustantivo + Repository → CourseRepository

  Servicios (iface)        I + Sustantivo + Service → IEmailService,
                           IStorageService

  Componentes React        PascalCase → CourseCard.tsx, VideoPlayer.tsx

  Hooks React              use + PascalCase → useAuth, useCourseProgress

  Stores Zustand           camelCase + Store → authStore.ts

  Variables / props        camelCase en TypeScript y C#

  Ramas Git                feature/nombre-corto, fix/nombre-corto,
                           chore/nombre-corto
  -----------------------------------------------------------------------

**4.4 Regla de dependencias entre proyectos .NET**

  -----------------------------------------------------------------------
  Domain ← Application ← Infrastructure ← API Domain ← Application ←
  Gamification La flecha indica \'depende de\'. Domain NO puede
  referenciar ningún otro proyecto. Application solo puede referenciar
  Domain. Infrastructure puede referenciar Application y Domain. API
  puede referenciar todos.

  -----------------------------------------------------------------------

**5. Flujo Completo de una Request**

Para entender cómo encajan las capas en la práctica, aquí está el
recorrido de una request de \'inscribirse a un curso\':

1.  El usuario hace POST /api/enrollments desde el frontend.

2.  EnrollmentsController recibe el request. Tiene inyectado
    ICommandHandler\<EnrollUserCommand\> y llama a HandleAsync(cmd).

3.  El DI resuelve ValidationDecorator\<EnrollUserCommand\>, que ejecuta
    primero el IValidator\<EnrollUserCommand\>. Si falla, lanza
    ValidationException.

4.  Si la validación pasa, el ValidationDecorator llama al
    LoggingDecorator, que registra la operación y luego delega al
    handler real.

5.  El EnrollUserHandler en Application verifica que el curso exista
    (vía ICourseRepository) y que el usuario no esté ya inscrito.

6.  El Handler llama a IEnrollmentRepository.CreateAsync() que en
    Infrastructure escribe en PostgreSQL vía EF Core.

7.  El Handler llama al Gamification microservice vía HTTP para
    recalcular el nivel Móvil.

8.  El controlador devuelve 201 Created. El ExceptionHandlingMiddleware
    atrapa cualquier excepción (ValidationException, NotFoundException,
    etc.) y la convierte en respuesta HTTP apropiada.

**6. Comandos para Crear la Estructura Inicial**

Una vez leído este documento, estos son los comandos exactos para crear
el monorepo y la solución .NET desde cero:

**Crear el monorepo y proyectos .NET:**

+-----------------------------------------------------------------------+
| \# 1. Crear la carpeta raíz del monorepo                              |
|                                                                       |
| mkdir elearning-platform && cd elearning-platform                     |
|                                                                       |
| git init                                                              |
|                                                                       |
| \# 2. Crear estructura de carpetas                                    |
|                                                                       |
| mkdir -p src/backend src/frontend docs .github/workflows              |
|                                                                       |
| \# 3. Crear la solución .NET                                          |
|                                                                       |
| cd src/backend                                                        |
|                                                                       |
| dotnet new sln -n ELearning                                           |
|                                                                       |
| \# 4. Crear los proyectos                                             |
|                                                                       |
| dotnet new classlib -n ELearning.Domain -o ELearning.Domain           |
|                                                                       |
| dotnet new classlib -n ELearning.Application -o ELearning.Application |
|                                                                       |
| dotnet new classlib -n ELearning.Infrastructure -o                    |
| ELearning.Infrastructure                                              |
|                                                                       |
| dotnet new webapi -n ELearning.API -o ELearning.API                   |
|                                                                       |
| dotnet new webapi -n ELearning.Gamification -o ELearning.Gamification |
|                                                                       |
| dotnet new xunit -n ELearning.Tests -o ELearning.Tests                |
|                                                                       |
| \# 5. Agregar proyectos a la solución                                 |
|                                                                       |
| dotnet sln add ELearning.Domain/ELearning.Domain.csproj               |
|                                                                       |
| dotnet sln add ELearning.Application/ELearning.Application.csproj     |
|                                                                       |
| dotnet sln add                                                        |
| ELearning.Infrastructure/ELearning.Infrastructure.csproj              |
|                                                                       |
| dotnet sln add ELearning.API/ELearning.API.csproj                     |
|                                                                       |
| dotnet sln add ELearning.Gamification/ELearning.Gamification.csproj   |
|                                                                       |
| dotnet sln add ELearning.Tests/ELearning.Tests.csproj                 |
|                                                                       |
| \# 6. Establecer referencias entre proyectos (regla de dependencias)  |
|                                                                       |
| dotnet add ELearning.Application/ELearning.Application.csproj         |
| reference \\                                                          |
|                                                                       |
| ELearning.Domain/ELearning.Domain.csproj                              |
|                                                                       |
| dotnet add ELearning.Infrastructure/ELearning.Infrastructure.csproj   |
| reference \\                                                          |
|                                                                       |
| ELearning.Application/ELearning.Application.csproj                    |
|                                                                       |
| dotnet add ELearning.API/ELearning.API.csproj reference \\            |
|                                                                       |
| ELearning.Application/ELearning.Application.csproj \\                 |
|                                                                       |
| ELearning.Infrastructure/ELearning.Infrastructure.csproj              |
|                                                                       |
| dotnet add ELearning.Gamification/ELearning.Gamification.csproj       |
| reference \\                                                          |
|                                                                       |
| ELearning.Application/ELearning.Application.csproj                    |
|                                                                       |
| dotnet add ELearning.Tests/ELearning.Tests.csproj reference \\        |
|                                                                       |
| ELearning.Application/ELearning.Application.csproj                    |
+-----------------------------------------------------------------------+

**Paquetes NuGet esenciales por proyecto:**

+-----------------------------------------------------------------------+
| \# ELearning.Application (cero librerías de terceros)                 |
|                                                                       |
| \# ─ Solo usa Microsoft.Extensions.DependencyInjection que ya viene   |
| con .NET                                                              |
|                                                                       |
| \# ─ Todas las interfaces (ICommandHandler, IQueryHandler,            |
| IValidator) son propias                                               |
|                                                                       |
| \# ELearning.Infrastructure                                           |
|                                                                       |
| dotnet add ELearning.Infrastructure package                           |
| Microsoft.EntityFrameworkCore                                         |
|                                                                       |
| dotnet add ELearning.Infrastructure package                           |
| Npgsql.EntityFrameworkCore.PostgreSQL                                 |
|                                                                       |
| dotnet add ELearning.Infrastructure package                           |
| Microsoft.EntityFrameworkCore.Design                                  |
|                                                                       |
| dotnet add ELearning.Infrastructure package StackExchange.Redis       |
|                                                                       |
| dotnet add ELearning.Infrastructure package QuestPDF                  |
|                                                                       |
| dotnet add ELearning.Infrastructure package ClosedXML                 |
|                                                                       |
| dotnet add ELearning.Infrastructure package SendGrid                  |
|                                                                       |
| dotnet add ELearning.Infrastructure package AWSSDK.S3                 |
|                                                                       |
| \# Nota de licencias:                                                 |
|                                                                       |
| \# ✅ Npgsql → MIT                                                    |
|                                                                       |
| \# ✅ StackExchange.Redis → MIT                                       |
|                                                                       |
| \# ✅ QuestPDF → MIT (Community license, libre para uso comercial)    |
|                                                                       |
| \# ✅ ClosedXML → MIT                                                 |
|                                                                       |
| \# ✅ SendGrid → MIT                                                  |
|                                                                       |
| \# ✅ AWSSDK → Apache 2.0                                             |
|                                                                       |
| \# ELearning.API                                                      |
|                                                                       |
| dotnet add ELearning.API package                                      |
| Microsoft.AspNetCore.Authentication.JwtBearer                         |
|                                                                       |
| dotnet add ELearning.API package Swashbuckle.AspNetCore               |
|                                                                       |
| dotnet add ELearning.API package Microsoft.AspNetCore.SignalR         |
|                                                                       |
| \# Nota: JwtBearer y SignalR son paquetes oficiales de Microsoft      |
| (MIT)                                                                 |
|                                                                       |
| \# Swashbuckle → MIT                                                  |
|                                                                       |
| \# ELearning.Tests                                                    |
|                                                                       |
| dotnet add ELearning.Tests package Moq                                |
|                                                                       |
| dotnet add ELearning.Tests package FluentAssertions                   |
|                                                                       |
| \# Nota: Moq → BSD-3 (libre). FluentAssertions → Apache 2.0 (libre)   |
+-----------------------------------------------------------------------+

**Crear el proyecto Next.js:**

+-----------------------------------------------------------------------+
| \# Desde la raíz del monorepo                                         |
|                                                                       |
| cd src/frontend                                                       |
|                                                                       |
| npx create-next-app@latest . \\                                       |
|                                                                       |
| \--typescript \\                                                      |
|                                                                       |
| \--tailwind \\                                                        |
|                                                                       |
| \--eslint \\                                                          |
|                                                                       |
| \--app \\                                                             |
|                                                                       |
| \--no-src-dir \\                                                      |
|                                                                       |
| \--import-alias \'@/\*\'                                              |
|                                                                       |
| \# Paquetes adicionales                                               |
|                                                                       |
| npm install axios zustand react-hook-form zod                         |
|                                                                       |
| npm install video.js react-pdf recharts                               |
|                                                                       |
| npm install \@microsoft/signalr                                       |
+-----------------------------------------------------------------------+

*Con este documento y la estructura definida, el siguiente paso es
ejecutar los comandos de la Sección 6 y crear las entidades del Domain
(Tarea 0.4).*
