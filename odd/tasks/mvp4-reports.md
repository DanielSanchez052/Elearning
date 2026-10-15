# MVP 4 — Reportes y administración

> Seguimiento por fases. Fase 1 en revisión (rama `feature/mvp4-f1-reports-overview`). Datos de prueba: `odd/dev-data/`.

## Contexto
El alcance (`docs/alcance_elearning.md` §3.6 y §5) define el MVP 4 con cuatro piezas:
- panel de reportes;
- rankings / leaderboards;
- exportación a Excel y PDF;
- panel admin por país.

Hoy solo existe un esqueleto sin usar:
- `Features/Reports/Queries/ReportQueries.cs`: dos handlers que lanzan `NotImplementedException`;
- `Features/Reports/DTOs/ReportDtos.cs`;
- el dashboard admin muestra métricas de relleno ("se muestran como placeholders").

El ranking "por nivel móvil" depende del **Nivel Móvil** (§3.4.2), que quedó pendiente en el MVP 3. La parte "admin por país" ya está cubierta en gran parte por las reglas de permisos (PR #23 y #25): falta llevarla a los reportes.

### Decisiones tomadas (2026-10-15)
- **Nivel Móvil completo:**
  - fórmula: cursos completados ÷ cursos activos disponibles en su país × 100;
  - rangos: Bronce ≤ 50 %, Plata ≤ 90 %, Oro > 90 %;
  - se calcula al vuelo, sin Redis;
  - se muestra en el perfil del estudiante y alimenta el ranking.
- **Quién ve los reportes:**
  - **superadmin:** todo, con filtro por país;
  - **admin:** solo su país;
  - **instructor:** los cursos que tiene asignados (resumen por curso, alumnos de cada curso, resultados de evaluaciones y exportación).
- **Rankings** (medallas, cursos completados, Nivel Móvil); solo cuentan estudiantes:
  - en el panel admin: nombres completos;
  - el estudiante ve el **top 10 de su país y su puesto**, con los demás como "Nombre I.".
- **Exportación en el backend:** Excel con **ClosedXML** (MIT, dependencia nueva) y PDF con **QuestPDF** (ya se usa en certificados). Mismos permisos y filtros que la pantalla.
- **Visualización:** KPIs (`StatCard`), tablas y barras con los componentes del sistema de diseño. Sin librería de gráficas.
- **Periodo:** selector de 7, 30 y 90 días o "Todo". "Usuarios activos" = inicio de sesión (`User.LastLoginAt`) dentro del periodo. Inscripciones y completaciones, las del periodo.
- **"Tiempo promedio"** = días promedio entre la inscripción y la finalización de los cursos completados.
- **Entrega en 5 fases, un PR cada una**, en ramas desde `main`.

## Arquitectura común (fase 1, reutilizada después)
- **Periodo:** `Features/Reports/Common/ReportPeriod.cs`, enum `Last7Days | Last30Days | Last90Days | AllTime` con `From(DateTime now)`. Va en query string (`?period=30d`).
- **Alcance:** `Common/Authorization/ReportAccess.cs`, junto a `CourseAccess` y `UserAccess`. Resuelve un `ReportScope(int? CountryId, Guid? InstructorId)`:
  - superadmin: `countryId` opcional;
  - admin: siempre su país;
  - instructor: solo sus cursos asignados (`Course.InstructorId`);
  - estudiante: sin acceso.
- **Lectura agregada:** `Domain/Interfaces/Repositories/IReportRepository.cs` con *read models* en `Domain/ReadModels/Reports/`, siguiendo el patrón de `ICertificatePdfService` con sus *records* en Domain. La implementación `Infrastructure/Repositories/ReportRepository.cs` usa consultas EF con `GroupBy` y `CountAsync` (Npgsql las traduce a SQL), sin cargar entidades.
- **Reglas puras y testeables** en Application, separadas del SQL:
  - tasa de completación, días promedio y rango del Nivel Móvil (`Features/Reports/Common/MobileLevel.cs`);
  - posición en el ranking, con empates "1, 1, 3";
  - nombre abreviado "Nombre I.".
- **Endpoints:** `ReportsController` en `api/admin/reports` (`RoleNames.Staff`). Cada acción comprueba su rol con `ReportAccess`, y las que no son para el instructor devuelven 403. Se sustituye el esqueleto vacío de `ReportQueries.cs` y `ReportDtos.cs`.
- **Frontend:**
  - ruta `/admin/reports` con pestañas (`SegmentedTabs`) y un selector de periodo común;
  - nueva entrada "Reportes" en `AdminLayout`, para admin, superadmin e instructor;
  - el instructor entra directamente en "Cursos".

## Fase 1 — Base de reportes, resumen y panel por país
- **Backend:** `GET api/admin/reports/overview?period=&countryId=` (admin y superadmin), que devuelve:
  - KPIs: estudiantes, usuarios activos, inscripciones del periodo, completaciones del periodo, tasa de completación y días promedio hasta completar;
  - cursos más populares (top 5 por inscripciones del periodo, con su tasa);
  - **comparativa por país** (solo superadmin sin filtro): una fila por país activo con las mismas métricas.
- **Frontend:**
  - pestaña "Resumen" con `StatCard`, tabla de cursos populares y tabla por país con barras de tasa (`ProgressBar`);
  - `AdminDashboard.tsx`: las métricas de relleno pasan a ser KPIs reales de los últimos 30 días, enlazados a Reportes.
- **Tests:**
  - `ReportAccess` (alcance por rol);
  - `ReportPeriod`;
  - cálculo de tasas y días promedio;
  - handler del resumen con el repositorio simulado (respeta el país del admin y rechaza al instructor).

## Fase 2 — Progreso por usuario y Nivel Móvil
- **Backend:**
  - `GET api/admin/reports/users?period=&countryId=&search=&page=`: por estudiante, nombre, país, inscritos, completados, en curso, días promedio, medallas, Nivel Móvil (% y rango) y último acceso. Paginado como `PagedResult`.
  - `GET api/admin/reports/users/{id}`: el detalle con cada curso (estado, lecciones completadas sobre el total, fechas) y sus medallas. El admin solo ve usuarios de su país (`UserAccess.CanView`, 404 si no).
  - `GET api/progress/level` (estudiante autenticado): su Nivel Móvil (completados, total, %, rango).
- **Frontend:**
  - pestaña "Usuarios" con buscador y detalle en un `Drawer`;
  - tarjeta "Nivel Móvil" en `pages/Profile.tsx`, con barra y rango;
  - eliminar `MobileLevelIndicator.tsx`, que no se usa y no encaja con el modelo (ver `arquitectura_elearning_v2.md`).
- **Docs:** marcar el Nivel Móvil como implementado en `alcance_elearning.md` §3.4.2 y en `arquitectura_elearning_v2.md`.
- **Tests:**
  - rangos del Nivel Móvil en sus límites (50 %, 90 %, 0 cursos activos);
  - handler del nivel;
  - alcance por país en el detalle de usuario.

## Fase 3 — Rankings
- **Backend:**
  - `GET api/admin/reports/rankings?type=badges|completed|level&countryId=&top=` (admin y superadmin): nombre completo y país, solo estudiantes.
  - `GET api/rankings?type=` (estudiante): top 10 de su país y su propia fila con posición, aunque esté fuera del top. Los nombres de los demás se muestran abreviados.
- **Frontend:**
  - pestaña "Rankings" en el panel;
  - página `/ranking` en el área del estudiante (enlace en `AppHeader`), con pestañas por tipo y su fila destacada.
- **Tests:**
  - empates de posición;
  - nombre abreviado;
  - la fila propia fuera del top 10;
  - solo cuentan estudiantes;
  - el estudiante no ve otros países.

## Fase 4 — Reportes del instructor (también disponibles para admin y superadmin)
- **Backend** (`RoleNames.Staff`, cada curso validado con `CourseAccess.CanManage`, 404 si no):
  - `GET api/admin/reports/courses?period=`: por curso, inscritos, completados, tasa y días promedio. El instructor ve solo los suyos; el admin, los de su país.
  - `GET api/admin/reports/courses/{id}/students`: alumnos con lecciones completadas sobre el total, estado y fechas.
  - `GET api/admin/reports/courses/{id}/assessments`: por quiz y por examen final, intentos, nota media y porcentaje de aprobados (sobre `UserQuizResult`).
- **Frontend:**
  - pestaña "Cursos" con el detalle del curso (alumnos y evaluaciones);
  - un acceso "Estadísticas" desde el listado de cursos para quien lo gestiona.
- **Tests:**
  - el instructor solo ve sus cursos;
  - el admin no ve cursos de otro país;
  - cálculos de nota media y porcentaje de aprobados.

## Fase 5 — Exportación a Excel y PDF
- **Domain:** `Interfaces/Services/IReportExporter.cs` con un modelo tabular genérico (`ReportTable`: título, subtítulo con filtros y periodo, columnas y filas) y `byte[] ToExcel(ReportTable)` / `byte[] ToPdf(ReportTable)`.
- **Infrastructure:**
  - `ClosedXmlReportExporter` (paquete `ClosedXML`);
  - `QuestPdfReportExporter`, que reutiliza la configuración de licencia de `QuestPdfCertificateService`;
  - un único `ReportExporter` que delega en ambos según el formato.
- **API:** `GET .../export?format=xlsx|pdf` en cada reporte (resumen, usuarios, rankings del panel, cursos, alumnos y evaluaciones). Reutiliza el handler de cada reporte y lo transforma en `ReportTable`. Devuelve `File(...)` como `CertificatesController`, con un nombre de archivo que incluye fecha y periodo.
- **Frontend:** un botón "Exportar" (menú Excel/PDF) en cada pestaña, descargado como blob igual que el certificado.
- **Tests:**
  - cada reporte → `ReportTable` (columnas y filas esperadas);
  - los exportadores generan un archivo válido y no vacío: `.xlsx` legible con ClosedXML y PDF con cabecera `%PDF`;
  - el instructor solo exporta sus cursos.

## Verificación (en cada fase)
1. `dotnet test ELearning.Tests` en verde; en el frontend, `npx tsc -b --noEmit` y `npm run lint`.
2. Prueba en vivo con una copia del backend y del frontend en otros puertos (5299 y 5174), con los usuarios demo y Edge headless, como en los PR anteriores:
   - comprobar las cifras contra consultas directas a la base de desarrollo (las agregaciones SQL no tienen test unitario);
   - comprobar el alcance por rol: el admin solo ve su país, el instructor solo sus cursos y el estudiante solo su ranking;
   - en la fase 5, descargar los archivos y abrirlos.
3. Si hace falta, crear datos de prueba (inscripciones y completaciones) y anotar en el PR qué se dejó en la base de desarrollo. No detener los servidores del usuario.
