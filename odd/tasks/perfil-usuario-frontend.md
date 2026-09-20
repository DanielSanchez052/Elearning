# Feature: Perfil de Usuario (MVP2 2.7) — wire real data

## Objective
Close MVP2 item 2.7. `Profile.tsx` today is a static placeholder — "Badges" and "Certificados" sections are hardcoded to "No hay ___ aún", not connected to any hook/API. Backend for certificates (2.6) and enrollments/progress (2.1) is already implemented and tested; this task is pure frontend wiring.

## Scope
1. `src/api/certificates.ts` (new) — `getMyCertificates()` → `GET /certificates/me`, matching the pattern in `src/api/enrollments.ts`. Backend DTO: `CertificateDto { id, courseId, courseName, completedAt, certificateUrl }` (camelCase on the wire). `certificateUrl` already points at the download endpoint (`GET /certificates/courses/{courseId}/download`, confirmed by `GetMyCertificatesHandlerTests.HandleAsync_CertificateUrl_PointsToDownloadEndpoint` on the backend) — no need to construct the download URL manually, just use the DTO field, but confirm whether it's a relative or absolute path and handle the axios baseURL accordingly (check `src/lib/axios.ts` for baseURL config).
2. `src/hooks/useCertificates.ts` (new) — `useMyCertificates()` React Query hook, same shape as `useMyEnrollments` in `src/hooks/useEnrollments.ts` (query key, staleTime ~60s).
3. `src/pages/Profile.tsx` (rewrite) — replace the two hardcoded placeholder sections:
   - **Cursos completados**: use the existing `useMyEnrollments()` hook (`src/hooks/useEnrollments.ts`), filter client-side for `status === 'Completed'` (or `=== 1`, the enum is `EnrollmentStatus.Completed` in `src/types/enrollment.types.ts` — the API can return either the numeric code or the string depending on JSON serializer config, handle both like `CourseDetailPage.tsx`'s existing `getEnrollmentStatusLabel` helper already does, don't assume just one). Show course title, thumbnail, completedAt date.
   - **Certificados**: use the new `useMyCertificates()` hook. Show course name, completedAt date, and a download link/button using `certificateUrl`.
   - **Badges**: leave as-is, still a placeholder — gamification (MVP3) hasn't started, this is intentionally out of scope, don't build fake badge logic.
   - Add loading and empty states for both sections (loading skeleton or simple "Cargando..." text is fine, match whatever minimal pattern the rest of the app uses — check `CourseDetailPage.tsx` or `CatalogPage.tsx` for the existing loading-state convention rather than inventing a new one).

## Constraints / conventions
- React + TanStack Query + Tailwind, matching `src/pages/courses/CourseDetailPage.tsx` and `src/hooks/useEnrollments.ts` conventions exactly (query key structure, `axios` import from `@/lib/axios`, error/loading handling style).
- No backend changes — everything needed already exists and is tested (Certificates: 26 tests; Enrollments: covered).
- No new test runner — this frontend has none configured (`package.json` only has `dev`/`build`/`lint`/`preview`); verify correctness via `npx tsc -b --noEmit` (must stay clean) and manual review of the JSX logic, don't invent a test framework for one page.

## Delivery strategy
Single work unit, one commit, Conventional Commits, no AI attribution. Branch from `main`: `feature/profile-page-real-data`.

## Tasks
- [x] 1. `src/api/certificates.ts`
- [x] 2. `src/hooks/useCertificates.ts`
- [x] 3. `src/pages/Profile.tsx` rewired (completed courses + certificates sections; badges untouched)
- [x] 4. `npx tsc -b --noEmit` clean

## Progress

Implemented:
- `src/frontend/elearning-web/src/types/certificate.types.ts` (new) — `CertificateDto` matching the backend record `{ id, courseId, courseName, completedAt, certificateUrl }` (camelCase on the wire). Not re-exported from `types/index.ts`, matching the existing direct-import convention used by `quiz.types.ts`/`course.types.ts`.
- `src/frontend/elearning-web/src/api/certificates.ts` (new) — `certificatesApi.getMyCertificates()` → `GET /certificates/me`, and `certificatesApi.downloadCertificate(certificateUrl)` for the PDF download (see ambiguity notes below).
- `src/frontend/elearning-web/src/hooks/useCertificates.ts` (new) — `useMyCertificates()`, same query-key/staleTime shape as `useMyEnrollments`.
- `src/frontend/elearning-web/src/pages/Profile.tsx` (rewritten) — "Información Personal" untouched. Added "Cursos completados" (from `useMyEnrollments()`, filtered client-side) and wired "Certificados" (from `useMyCertificates()`) with a real download button. "Badges" left exactly as the static placeholder, no fake data/logic added.

Verification:
- `npx tsc -b --noEmit` — clean, no output/errors.
- `npm run lint` — 14 pre-existing errors / 5 warnings, all in files untouched by this change (`LessonPlayer.tsx`, `CourseList.tsx`, `CourseDetailPage.tsx`, `QuizSessionPage.tsx`, `AdminCourseFormPage.tsx`, `RegisterForm.tsx`); confirmed via `git status` that these files aren't part of this diff. Zero lint findings in the 4 new/changed files.

Ambiguity resolutions:
1. **Enrollment status representation** — `EnrollmentSummaryDto.status` is typed as `EnrollmentStatusValue` (`0|1|2 | 'Active'|'Completed'|'Abandoned'`), and the backend's actual JSON serializer config isn't pinned down on the frontend side. Added a local `isCompletedStatus(status)` helper in `Profile.tsx` that accepts both `1` and `'Completed'`, mirroring the existing (non-exported) `getEnrollmentStatusLabel` helper in `CourseDetailPage.tsx` rather than importing/refactoring it (kept the diff scoped to Profile.tsx per the single-work-unit delivery strategy).
2. **Certificate URL construction** — traced the actual value: `GetMyCertificatesQuery.cs` builds `CertificateUrl: $"/api/certificates/courses/{courseId}/download"` — a root-relative path that **already includes the `/api` prefix**. The frontend's axios instance (`src/lib/axios.ts`) has `baseURL: '.../api'` (also includes `/api`), so passing `certificateUrl` straight into `axios.get()` would double up to `/api/api/certificates/...`. `certificatesApi.downloadCertificate()` strips the leading `/api` before the axios call so it resolves to the same URL the DTO points at. Also discovered the download route is behind class-level `[Authorize]` on `CertificatesController`, and this app's auth is Bearer-token-in-localStorage (no auth cookie) — a plain `<a href>` to that URL would 401 since the browser wouldn't attach the JWT. Implemented the download via `axios.get(..., { responseType: 'blob' })` (so the existing request interceptor attaches the Bearer token), then built a temporary blob URL + synthetic `<a download>` click to trigger the browser save, revoking the object URL afterward.

Other notes:
- Loading/empty states use plain conditional text (`Cargando...` / `No hay ... aún`), matching `Profile.tsx`'s own existing simple-text convention (not the dark-theme skeleton pattern used in `CourseDetailPage.tsx`/`CatalogPage.tsx`, which belongs to a different visual theme entirely) — kept consistent with the surrounding page rather than mixing styles.
- No backend files were touched.
