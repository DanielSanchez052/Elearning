# MVP 2 — Hoja de Ruta Revisada

**Plataforma E-Learning Multipaís**
Stack: ASP.NET Core · EF Core · React + Vite · TypeScript · PostgreSQL

---

| # | Tarea | Descripción | Stack | Estado | Dependencias |
|---|---|---|---|---|---|
| 2.0 | Fase B — Gestión de Cursos (Frontend) | Drag & drop para reordenar lecciones, barra de progreso mejorada en uploads de archivos grandes. Deuda pendiente del MVP 1. | React · Vite · TypeScript | ✅ Hecho | MVP 1 completo |
| 2.1 | Módulo de Inscripciones + Progreso (Backend) | Endpoints de inscripción (`POST /courses/:id/enroll`, `GET /enrollments/me`), validaciones (no duplicados, solo cursos activos del país), registro de progreso por lección, marcación automática de curso completado, timestamps. | ASP.NET Core · EF Core · PostgreSQL | ✅ Hecho | MVP 1 completo |
| 2.2 | Módulo de Evaluaciones (Backend) | CRUD de preguntas, endpoint de envío de respuestas, cálculo de puntaje y aprobación. | ASP.NET Core · EF Core · C# | ✅ Hecho | 2.1 |
| 2.3 | Gestión de Preguntas de Quiz (Frontend) | Panel para que instructores creen, editen y eliminen preguntas de quiz desde la gestión del curso. | React · Vite · TypeScript | ✅ Hecho | 2.2 |
| 2.4 | Módulo de Evaluaciones (Frontend) | Pantalla de quiz con temporizador, resultado inmediato, opción de reintento si falla. | React · Vite · Zustand | ✅ Hecho | 2.2 · 2.3 |
| 2.5 | Inscripción + Progreso Visual (Frontend) | Botón "Inscribirse" en detalle del curso, estado "ya inscrito", barra de progreso por curso, checklist de lecciones completadas. | React · Vite · Tailwind | ✅ Hecho | 2.1 |
| 2.6 | Generación de Certificados PDF | Plantilla de certificado, generación dinámica con nombre/curso/fecha, endpoint de descarga. | C# · QuestPDF | 🔶 Backend hecho, falta frontend | 2.1 |
| 2.7 | Perfil de Usuario | Página de perfil: cursos completados, certificados descargables, medallas (placeholder). | React · Vite · TypeScript | ⏳ Pendiente | 2.5 · 2.6 |

---

## Hito MVP 2
> ✅ Plataforma con flujo completo de aprendizaje: inscripción → lecciones → quiz → certificado → perfil

---

## Notas

- **2.0** es deuda del MVP 1. No es bloqueante pero afecta la experiencia de instructores en producción.
- **2.1** unifica inscripciones y progreso porque la inscripción es prerequisito directo del progreso — separarlos genera trabajo duplicado en repositorios y migraciones.
- **2.3** es una tarea nueva no contemplada originalmente. Sin ella, los quizzes existen en la BD pero no hay forma de cargarlos desde la UI.
- **2.6** usar **QuestPDF** — iTextSharp está prácticamente en desuso en el ecosistema .NET actual.
- **(2026-09-20)** Estado verificado contra el código real, no contra suposiciones: 2.0 a 2.5 confirmados hechos (backend con tests de handler + frontend wired, no placeholders). 2.6 tiene el backend completo (`QuestPdfCertificateService`, handlers con 26 tests) pero ningún componente de frontend llama todavía a esos endpoints. 2.7 (`Profile.tsx`) es un placeholder estático — badges y certificados están hardcodeados como "No hay ___ aún", sin conectar a ningún hook/API. Cobertura de tests backend de Quizzes pasó de 0 handlers testeados a 120 tests (`test/quizzes-notifications-reports-coverage`); Notifications y Reports quedaron identificados como features sin implementar (`NotImplementedException` en los 4+2 handlers), no como hueco de tests — fuera de este roadmap hasta que se decida construirlas.
