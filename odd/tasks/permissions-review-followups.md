# Revisión de permisos — pendientes

Todo el trabajo de permisos va en la rama `fix/permissions-review`.

## Reglas acordadas (2026-10-14)

Centralizadas en `ELearning.Application/Common/Authorization/CourseAccess.cs` y aplicadas en cursos, lecciones y quizzes del panel de administración:

| Rol | Puede gestionar un curso (editar, publicar, borrar, lecciones, quizzes) | Crear / marcar como global | Asignar países |
|---|---|---|---|
| **superadmin** | Cualquiera, incluidos los globales | ✅ | Cualquier cambio |
| **admin** | No global y asignado a su país (aunque esté compartido con otros) | ❌ | Solo añadir o quitar su propio país |
| **instructor** | No global y creado por él | ❌ | Solo añadir o quitar su propio país |

Los cursos se crean asignados al país del creador (`country_id` del JWT).

## Ya resuelto
- **Quizzes sin control de acceso**: los 8 endpoints de `AdminQuizzesController` dejaban a cualquier instructor leer (incluido `IsCorrect`), crear, editar y borrar preguntas y opciones de cualquier curso. Ahora siguen las reglas de `CourseAccess` a través de `QuizAuthoringAccess`.
- **Helpers de rol con el nombre equivocado**: `ClaimsPrincipalExtensions.IsSuperAdmin()` / `IsAdmin()` comparaban con `"super_admin"`, pero el JWT emite `"superadmin"`.
- **Alcance de países del instructor**: ya no puede crear cursos globales ni asignar otros países.
- **Admin limitado a su país**: ya no puede modificar cursos de otros países ni cursos globales.
- **Frontend alineado** (`src/frontend/elearning-web/src/lib/courseAccess.ts` replica las reglas):
  - Formulario de curso: la opción "Global" solo la ve el superadmin; admin e instructor ven los países del curso más el suyo, y solo pueden marcar o desmarcar el suyo. Al crear, el curso queda en el país del creador sin llamadas extra.
  - El instructor vuelve a tener el botón "Nuevo curso" (antes estaba oculto) y ya no pide `/admin/countries`, que le daba 403.
  - Listado: los cursos que el usuario no puede gestionar (los globales para el admin) se muestran como "Solo lectura".
  - Editar un curso o sus quizzes sin permiso muestra un aviso en lugar del editor, y las preguntas (con respuestas) no se piden.

## Pendientes

### 1. Detalle de curso sin restricciones
- `GET /api/courses/{id}` (`GetCourseDetailQuery`) solo exige estar autenticado: cualquier usuario ve cursos inactivos o de otro país, con los `ContentUrl` de todas sus lecciones.
- El panel de administración probablemente use este endpoint para editar; revisar si conviene separar un detalle admin y restringir el público a cursos activos y disponibles en el país del usuario.

### 2. Inscripción sin control de país
- `EnrollInCourseCommand` solo comprueba `course.IsActive`, no que el curso esté disponible en el país del usuario. Tampoco hay restricción de rol (un instructor o admin puede inscribirse como estudiante; puede ser intencional).

### 3. Limpieza menor
- `AdminCoursesController.GetAdminList` compara `requesterRole == "instructor"` en vez de usar `User.IsInstructor()`.
- `AuthorizationExtensions.AddAuthorizationPolicies` tiene todas las políticas comentadas (con roles `Teacher` que no existen). Decidir si se eliminan o si se pasa a políticas con nombre (`StaffOnly`, `AdminOnly`) en lugar de cadenas `Roles = "..."` repetidas.
- Los roles están como cadenas sueltas en `[Authorize(Roles = ...)]`, `ChangeUserRoleValidator`, etc. Valorar constantes compartidas para evitar otro desajuste como el de `super_admin`.
- El `country_id` viene del JWT: si un superadmin cambia el país de un usuario, las reglas usan el país antiguo hasta que el usuario vuelva a iniciar sesión.

### 4. Siguientes partes de la revisión
- Mismo repaso para **admin** y **superadmin** en usuarios y países.
- Comprobar que el resto del **frontend** (rutas protegidas en `App.tsx`) aplica las mismas reglas que el backend y maneja bien los 403.
