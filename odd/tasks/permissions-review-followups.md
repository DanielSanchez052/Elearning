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
- **Detalle de curso con restricciones** (`GET /api/courses/{id}`, `GetCourseDetailQuery`):
  - Quien gestiona el curso (`CourseAccess.CanManage`) lo ve en cualquier estado.
  - Quien está inscrito lo sigue viendo aunque se despublique o cambie de país.
  - El resto solo si está publicado y es global o de su país (`CourseAccess.IsAvailableIn`, la misma regla del catálogo).
  - Si no le corresponde: 404, sin revelar que existe.
  - Los `ContentUrl` de las lecciones solo van a quien gestiona el curso o está inscrito.
  - Frontend: el 404 muestra "Este curso no está disponible", y al inscribirse se vuelve a pedir el detalle para traer los enlaces.
- **Inscripción con control de país** (`EnrollInCourseCommand`): solo en cursos disponibles en el país del usuario (`CourseAccess.IsAvailableIn`); si no, 404 como el detalle (antes un curso inactivo daba 409). Cualquier rol puede inscribirse (decisión del 2026-10-14).

- **Limpieza de roles y autorización**:
  - Nombres de rol centralizados en `ELearning.Domain/Constants/RoleNames.cs` (`Student`, `Instructor`, `Admin`, `SuperAdmin`, combinaciones `Admins` y `Staff`, `ToRoleName()` y `TryParse`, que no acepta valores numéricos). Se usan en `[Authorize(Roles = ...)]`, el JWT, los DTO de usuario, `CourseAccess`, el cambio de rol y el filtro de usuarios por rol.
  - Helpers de `ClaimsPrincipal`: `IsAdmin()` ya no incluye al superadmin; se quitaron los que no se usaban (`IsStudent`, `HasAdminAccess`, `GetEmail`). `GetAdminList` usa `User.IsInstructor()`.
  - Eliminado `AuthorizationExtensions` (todo comentado y con un `AddAuthorization()` duplicado) y los controladores vacíos `LessonsController` y `ReportsController`.
  - `JwtService` leía `Jwt:ExpiresInMinutes`, pero la configuración define `Jwt:ExpirationMinutes`; funcionaba solo porque el valor por defecto coincidía (60).
  - Fuera comentarios que repetían el código, un TODO especulativo y `using` sin uso en handlers y validadores.
- **Auditoría de sincronización frontend ↔ backend** (2026-10-14). Se quitaron acciones visibles que el backend rechazaba:
  - `/admin/countries` era accesible por URL para el admin, con botones de crear y activar que daban 403. La ruta queda solo para superadmin.
  - Usuarios: "Editar" ya no aparece en la fila propia ("Tú") ni, para el admin, en las de superadmin ("Solo lectura").
  - Vista del curso: el staff que abre en vista previa un curso en borrador o de otro país ya no ve "Inscribirme" (daba 404), sino un aviso de vista previa.
  - Listado de cursos: el ojo "Ver como estudiante" solo aparece si la vista va a cargar.
  - En sentido contrario no falta nada: todas las acciones que el backend permite a cada rol tienen su lugar en la interfaz.
- **Usuarios y países (admin y superadmin)**, decisiones del 2026-10-14, centralizadas en `ELearning.Application/Common/Authorization/UserAccess.cs`:
  - El admin solo ve y modifica usuarios de su país, también en el detalle (`GET /api/admin/users/{id}`) y en el cambio de rol; los de otro país dan 404.
  - El admin solo cambia el rol de estudiantes e instructores, y solo a estudiante o instructor. Los admins y superadmins los gestiona el superadmin, que puede cambiar el rol de cualquiera menos el suyo (así siempre queda al menos uno).
  - Cambiar de país a un instructor no mueve sus cursos: siguen en su país original y él los sigue gestionando como creador.
  - Desactivar un país solo bloquea las altas nuevas: los usuarios de ese país siguen entrando y cursando. El registro ya no acepta países inactivos (antes solo lo impedía la interfaz).
  - El superadmin puede cambiar el país de cualquiera, incluido el suyo.
  - Frontend: el admin solo ve "Editar" en estudiantes e instructores.
  - De paso: cinco archivos del backend estaban en Windows-1252 en vez de UTF-8 (sus tildes se habrían roto al compilar fuera de Windows); convertidos a UTF-8.

## Limitaciones conocidas (aceptadas)

### Claims del token desactualizados — decisión del 2026-10-14: documentar y no tocar
- El rol y el país viajan en el JWT (`role`, `country_id`) y el backend los usa tal cual en cada petición, sin consultar la base de datos.
- El token dura `Jwt:ExpirationMinutes` (60). Si un superadmin cambia el rol o el país de un usuario, sus permisos siguen siendo los antiguos hasta que el token expira y vuelve a iniciar sesión. Por ejemplo, a un admin al que se le quita el rol le quedan hasta 60 minutos de acceso de admin.
- El logout es solo del lado del cliente (borra el token del navegador); en el servidor no se revoca. `LogoutHandler` existe pero no está expuesto y lanza `NotImplementedException`.
- Si en el futuro hace falta cerrar esa ventana, se valoraron:
  - releer rol y país de la base de datos al validar el token (efecto inmediato, sin migración);
  - un sello de seguridad en el usuario que invalide sus tokens al cambiar rol, país o contraseña;
  - acortar la duración del token, que sin refresh tokens obliga a iniciar sesión más a menudo.

## Pendientes

Ninguno: la revisión de permisos quedó completa el 2026-10-14. Fuera de alcance: los reportes sin implementar (`quizzes-notifications-reports-tests.md`) y la carrera al borrar las últimas preguntas u opciones (`exam-module-empty-required-quiz-guards.md`). El logout del backend está en "Limitaciones conocidas".
