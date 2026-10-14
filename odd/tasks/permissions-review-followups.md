# Revisión de permisos — pendientes (2026-10-14)

## Contexto
Primera parte de la revisión de permisos por rol, con foco en lo que alcanza el **instructor** desde el backend en el módulo de cursos del panel de administración.

Ya resuelto en la rama `fix/instructor-quiz-ownership`:
- **Quizzes sin control de propiedad**: los 8 endpoints de `AdminQuizzesController` (`/api/admin/quizzes`) dejaban a cualquier instructor leer (incluido `IsCorrect`), crear, editar y borrar preguntas y opciones de cualquier curso. Ahora pasan `RequesterId` / `RequesterRole` y aplican `QuizAuthoringAccess` (`ELearning.Application/Features/Quizzes/Common/QuizAuthoringAccess.cs`): admin y superadmin gestionan todo; el instructor solo los cursos que creó; cualquier otro rol → 403.
- **Helpers de rol con el nombre equivocado**: `ClaimsPrincipalExtensions.IsSuperAdmin()` / `IsAdmin()` comparaban con `"super_admin"`, pero el JWT emite `"superadmin"` (`JwtService.cs:37`). Corregido; los dos usos (`AdminCoursesController`, `AdminUsersController`) mantienen el mismo comportamiento.

## Pendientes por decidir / revisar

### 1. Alcance de países del instructor
- `POST /api/admin/courses`: el instructor puede crear cursos con `IsGlobal = true` (visibles en todos los países).
- `PUT /api/admin/courses/{id}`: puede cambiar `IsGlobal` en sus cursos.
- `PUT /api/admin/courses/{id}/countries` (`AssignCourseCountriesCommand`): comprueba que el curso sea suyo, pero deja asignar **cualquier** país activo, no solo el suyo.
- **Decidir**: ¿publicar globalmente o en otros países debe quedar solo para admin/superadmin?

### 2. El admin no está limitado a su país al modificar cursos
- `GET /api/admin/courses` filtra por el `country_id` del admin, pero las mutaciones (`UpdateCourse`, `ToggleCourseStatus`, `DeleteCourse`, `AssignCourseCountries`, lecciones y ahora quizzes) solo comprueban la propiedad cuando el rol es `Instructor`. Un admin del país A puede editar, publicar o borrar un curso exclusivo del país B si conoce su id.
- **Decidir**: ¿el admin solo gestiona cursos de su país (o globales)? Si es así, extender la regla en los handlers de cursos/lecciones y en `QuizAuthoringAccess`.

### 3. Detalle de curso sin restricciones
- `GET /api/courses/{id}` (`GetCourseDetailQuery`) solo exige estar autenticado: cualquier usuario ve cursos inactivos o de otro país, con los `ContentUrl` de todas sus lecciones.
- El panel de administración probablemente use este endpoint para editar; revisar si conviene separar un detalle admin y restringir el público a cursos activos y disponibles en el país del usuario.

### 4. Inscripción sin control de país
- `EnrollInCourseCommand` solo comprueba `course.IsActive`, no que el curso esté disponible en el país del usuario. Tampoco hay restricción de rol (un instructor o admin puede inscribirse como estudiante; puede ser intencional).

### 5. Limpieza menor
- `AdminCoursesController.GetAdminList` compara `requesterRole == "instructor"` en vez de usar `User.IsInstructor()`.
- `AuthorizationExtensions.AddAuthorizationPolicies` tiene todas las políticas comentadas (con roles `Teacher` que no existen). Decidir si se eliminan o si se pasa a políticas con nombre (`StaffOnly`, `AdminOnly`) en lugar de cadenas `Roles = "..."` repetidas.
- Los roles están como cadenas sueltas en `[Authorize(Roles = ...)]`, `ChangeUserRoleValidator`, etc. Valorar constantes compartidas para evitar otro desajuste como el de `super_admin`.

### 6. Siguientes partes de la revisión
- Mismo repaso para **admin** y **superadmin** (usuarios, países, cursos).
- Comprobar que el **frontend** (`src/frontend/elearning-web`, rutas protegidas en `App.tsx`) aplica las mismas reglas que el backend y maneja bien los nuevos 403 de quizzes.
