import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import AppHeader from '@/components/layout/AppHeader';
import { useCourseDetail } from '@/hooks/useCourses';
import { useCourseExam, useCourseExamResults } from '@/hooks/quizzes';
import { useCourseProgress, useEnrollInCourse } from '@/hooks/useEnrollments';
import type { LessonDto } from '@/types/course.types';
import LessonPlayer from '@/components/courses/LessonPlayer';

export default function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: course, isLoading, isError } = useCourseDetail(id ?? '');
  const examQuery = useCourseExam(id ?? '', Boolean(id));
  const courseProgressQuery = useCourseProgress(id ?? '', Boolean(id));
  const examResultsQuery = useCourseExamResults(id ?? '', Boolean(id));
  const enrollMutation = useEnrollInCourse();
  const [activeLesson, setActiveLesson] = useState<LessonDto | null>(null);

  const progressStatus = (courseProgressQuery.error as any)?.response?.status;
  const isNotEnrolled = courseProgressQuery.isError && progressStatus === 404;
  const progress = courseProgressQuery.data;
  const canAccessLearning = Boolean(progress);
  const examStatus = (examQuery.error as any)?.response?.status;
  const examErrorMessage =
    (examQuery.error as any)?.response?.data?.message ||
    (examQuery.error as any)?.response?.data?.error ||
    '';
  const examLockedByProgress = canAccessLearning && examStatus === 403;
  const hasFinalExamConfigured =
    (examQuery.data?.length ?? 0) > 0 || examLockedByProgress;
  const examAttempts = examResultsQuery.data ?? [];
  const passedFinalExam = examAttempts.some((attempt) => attempt.isPassed);
  const passedAttempt = examAttempts.find((attempt) => attempt.isPassed) ?? null;

  const handleEnroll = async () => {
    if (!id) return;
    try {
      await enrollMutation.mutateAsync(id);
    } catch {
      // handled by mutation state
    }
  };

  if (isLoading) return <CourseDetailSkeleton />;

  if (isError || !course) {
    return (
      <div className="min-h-screen bg-[#0a0a0f] flex items-center justify-center">
        <div className="text-center space-y-3">
          <p className="text-zinc-400 text-sm">Curso no encontrado.</p>
          <Link to="/courses" className="text-indigo-400 text-sm hover:text-indigo-300 transition-colors">
            ← Volver al catálogo
          </Link>
        </div>
      </div>
    );
  }

  const orderedLessons = [...course.lessons].sort((a, b) => a.orderIndex - b.orderIndex);
  const quizLessons = orderedLessons.filter((l) => l.type === 'quiz');
  const completedLessonIds = progress?.lessons
    .filter((l) => l.isCompleted)
    .map((l) => l.lessonId) ?? [];
  const completedSet = new Set(completedLessonIds);

  return (
    <>
      <div className="min-h-screen bg-[#0a0a0f] text-white">
        <AppHeader />

        {/* Breadcrumb */}
        <div className="border-b border-white/[0.06]">
          <div className="max-w-5xl mx-auto px-6 py-3 flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm text-zinc-500">
              <Link to="/courses" className="hover:text-zinc-300 transition-colors">Catálogo</Link>
              <span>/</span>
              <span className="text-zinc-300 truncate max-w-xs">{course.title}</span>
            </div>
          </div>
        </div>

        <div className="max-w-5xl mx-auto px-6 py-10 space-y-10">

          {/* Hero del curso */}
          <div className="flex gap-8 items-start">
            {/* Thumbnail */}
            <div className="hidden md:block w-56 h-36 rounded-2xl overflow-hidden flex-shrink-0 bg-gradient-to-br from-indigo-900/40 to-zinc-900">
              {course.thumbnailUrl ? (
                <img src={course.thumbnailUrl} alt={course.title} className="w-full h-full object-cover" />
              ) : (
                <div className="w-full h-full flex items-center justify-center">
                  <svg className="w-10 h-10 text-indigo-800/60" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                  </svg>
                </div>
              )}
            </div>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-3">
                {course.isGlobal && (
                  <span className="px-2 py-0.5 rounded-full bg-indigo-500/20 border border-indigo-500/30 text-indigo-300 text-xs font-medium">
                    Global
                  </span>
                )}
                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${course.isActive
                  ? 'bg-emerald-500/10 border border-emerald-500/20 text-emerald-400'
                  : 'bg-zinc-500/10 border border-zinc-500/20 text-zinc-400'
                  }`}>
                  {course.isActive ? 'Publicado' : 'Borrador'}
                </span>
              </div>

              <h1 className="text-2xl font-semibold text-white leading-tight">{course.title}</h1>

              {course.description && (
                <p className="mt-3 text-zinc-400 text-sm leading-relaxed">{course.description}</p>
              )}

              <div className="mt-4 flex flex-wrap items-center gap-4 text-sm text-zinc-500">
                <span className="flex items-center gap-1.5">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  {course.instructorName}
                </span>
                <span className="flex items-center gap-1.5">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                  </svg>
                  {course.lessons.length} lecciones
                </span>
                {course.countries.length > 0 && !course.isGlobal && (
                  <span className="flex items-center gap-1.5">
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064" />
                    </svg>
                    {course.countries.map((c) => c.name).join(', ')}
                  </span>
                )}
              </div>
            </div>
          </div>

          <div className="rounded-2xl border border-white/[0.08] bg-[#111118] p-5">
            {courseProgressQuery.isLoading ? (
              <p className="text-sm text-zinc-400">Cargando estado de inscripción...</p>
            ) : canAccessLearning && progress ? (
              <div className="space-y-3">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <p className="text-xs uppercase tracking-wider text-zinc-500">Tu progreso</p>
                    <p className="text-sm text-zinc-300 mt-1">
                      {progress.completedLessons} / {progress.requiredLessons} lecciones requeridas completadas
                    </p>
                  </div>
                  <span className="rounded-full border border-emerald-500/30 bg-emerald-500/10 px-3 py-1 text-xs text-emerald-300">
                    Inscrito
                  </span>
                </div>

                <div className="h-2.5 w-full overflow-hidden rounded-full bg-white/[0.08]">
                  <div
                    className="h-full rounded-full bg-emerald-500 transition-all duration-300"
                    style={{ width: `${Math.min(Math.max(progress.progressPercent, 0), 100)}%` }}
                  />
                </div>

                <div className="flex items-center justify-between text-xs text-zinc-500">
                  <span>{progress.progressPercent}% completado</span>
                  <span>Estado: {getEnrollmentStatusLabel(progress.status)}</span>
                </div>

                <div className="rounded-lg border border-white/[0.08] bg-white/[0.02] px-3 py-2 text-xs text-zinc-400">
                  {hasFinalExamConfigured
                    ? progress.progressPercent >= 100
                      ? 'Ya completaste las lecciones requeridas. El curso se completa al aprobar el examen final.'
                      : 'Completa las lecciones requeridas para desbloquear el examen final del curso.'
                    : 'Este curso no tiene examen final. Se completará al terminar las lecciones requeridas.'}
                </div>
              </div>
            ) : isNotEnrolled ? (
              <div className="space-y-3">
                <p className="text-sm text-zinc-300">
                  Inscríbete para desbloquear las lecciones, evaluaciones y el examen final.
                </p>
                <button
                  onClick={handleEnroll}
                  disabled={enrollMutation.isPending}
                  className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-60 transition"
                >
                  {enrollMutation.isPending ? 'Inscribiendo...' : 'Inscribirme al curso'}
                </button>
                {enrollMutation.isError && (
                  <p className="text-xs text-red-400">
                    {(enrollMutation.error as any)?.response?.data?.message ||
                      'No se pudo completar la inscripción. Intenta nuevamente.'}
                  </p>
                )}
              </div>
            ) : (
              <p className="text-sm text-zinc-400">
                No se pudo verificar tu inscripción en este momento.
              </p>
            )}
          </div>

          {/* Lecciones */}
          {course.lessons.length === 0 ? (
            <div className="text-center py-12 rounded-2xl bg-[#111118] border border-white/[0.06]">
              <p className="text-zinc-500 text-sm">Este curso aún no tiene lecciones.</p>
            </div>
          ) : (
            <div className="space-y-8">
              <div>
                <div className="flex items-center justify-between gap-3 mb-3">
                  <h2 className="text-sm font-semibold text-zinc-300 uppercase tracking-wider">
                    Ruta del curso
                  </h2>
                  <span className="text-xs text-zinc-500">Ordenada por secuencia pedagógica</span>
                </div>

                <div className="rounded-2xl border border-white/[0.06] bg-[#0f0f16] p-3 space-y-2">
                  {orderedLessons.map((lesson, index) => {
                    const isQuiz = lesson.type === 'quiz';
                    const isCompleted = completedSet.has(lesson.id);
                    const missingRequiredBefore = orderedLessons
                      .filter((l) => l.isRequired && l.orderIndex < lesson.orderIndex)
                      .some((l) => !completedSet.has(l.id));

                    const lockedByEnrollment = !canAccessLearning;
                    const lockedByPrereq = canAccessLearning && isQuiz && missingRequiredBefore;
                    const isLocked = lockedByEnrollment || lockedByPrereq;

                    const rowClass = isLocked
                      ? 'border-white/[0.06] bg-white/[0.01] opacity-65'
                      : isCompleted
                        ? 'border-emerald-500/25 bg-emerald-500/5 hover:bg-emerald-500/10'
                        : 'border-white/[0.06] bg-[#111118] hover:border-indigo-500/30 hover:bg-indigo-500/5';

                    const content = (
                      <>
                        <span className={`w-8 h-8 rounded-lg border flex items-center justify-center text-xs font-semibold flex-shrink-0 ${
                          isCompleted
                            ? 'border-emerald-500/30 bg-emerald-500/15 text-emerald-300'
                            : 'border-white/[0.08] bg-white/[0.03] text-zinc-400'
                        }`}>
                          {index + 1}
                        </span>

                        <span className="text-zinc-400 flex-shrink-0">{getLessonIcon(lesson.type)}</span>

                        <div className="flex-1 min-w-0">
                          <p className={`text-sm font-medium ${isCompleted ? 'text-emerald-200' : 'text-zinc-200'}`}>
                            {lesson.title}
                          </p>
                          <div className="mt-1 flex flex-wrap items-center gap-2 text-xs">
                            <span className="text-zinc-500 capitalize">{lesson.type}</span>
                            {lesson.isRequired && (
                              <span className="rounded-full border border-amber-500/25 bg-amber-500/10 px-2 py-0.5 text-amber-300">
                                Requerida
                              </span>
                            )}
                            {isCompleted && (
                              <span className="rounded-full border border-emerald-500/25 bg-emerald-500/10 px-2 py-0.5 text-emerald-300">
                                Completada
                              </span>
                            )}
                            {lockedByPrereq && (
                              <span className="rounded-full border border-indigo-500/30 bg-indigo-500/10 px-2 py-0.5 text-indigo-300">
                                Completa requeridas previas
                              </span>
                            )}
                            {lockedByEnrollment && (
                              <span className="rounded-full border border-zinc-500/30 bg-zinc-500/10 px-2 py-0.5 text-zinc-300">
                                Inscripción requerida
                              </span>
                            )}
                          </div>
                        </div>

                        <span className="text-xs text-zinc-500 flex-shrink-0">
                          {isLocked
                            ? 'Bloqueada'
                            : isQuiz
                              ? 'Iniciar evaluación'
                              : 'Abrir lección'}
                        </span>
                      </>
                    );

                    if (isQuiz) {
                      return isLocked ? (
                        <div
                          key={lesson.id}
                          className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl border transition-all ${rowClass}`}
                        >
                          {content}
                        </div>
                      ) : (
                        <Link
                          key={lesson.id}
                          to={`/courses/${course.id}/lessons/${lesson.id}/quiz`}
                          className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl border transition-all ${rowClass}`}
                        >
                          {content}
                        </Link>
                      );
                    }

                    return (
                      <button
                        key={lesson.id}
                        onClick={() => !isLocked && setActiveLesson(lesson)}
                        disabled={isLocked}
                        className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl border transition-all text-left ${rowClass}`}
                      >
                        {content}
                      </button>
                    );
                  })}
                </div>
              </div>

              {canAccessLearning && quizLessons.length > 0 && (
                <p className="-mt-4 text-xs text-zinc-500">
                  Nota: las evaluaciones respetan el orden del curso y pueden requerir completar lecciones previas.
                </p>
              )}

              {hasFinalExamConfigured && (
                <div className="rounded-2xl border border-indigo-500/30 bg-indigo-500/10 p-5">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div>
                      <h3 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">
                        Examen final del curso
                      </h3>
                      {passedFinalExam ? (
                        <p className="text-xs text-emerald-300 mt-1">
                          Examen aprobado{passedAttempt?.completedAt
                            ? ` el ${new Date(passedAttempt.completedAt).toLocaleDateString()}`
                            : ''}. El curso ya está completado.
                        </p>
                      ) : examLockedByProgress ? (
                        <p className="text-xs text-indigo-300/80 mt-1">
                          {examErrorMessage ||
                            'Completa las lecciones requeridas para habilitar el examen final.'}
                        </p>
                      ) : (
                        <p className="text-xs text-indigo-300/80 mt-1">
                          {examQuery.data?.length ?? 0} preguntas listas para evaluar tu progreso.
                        </p>
                      )}
                    </div>
                    <Link
                      to={`/courses/${course.id}/exam`}
                      className={`rounded-lg px-4 py-2 text-sm font-medium transition ${
                        canAccessLearning && !examLockedByProgress && !passedFinalExam
                          ? 'bg-indigo-600 text-white hover:bg-indigo-500'
                          : 'pointer-events-none bg-zinc-700 text-zinc-400'
                      }`}
                    >
                      {canAccessLearning
                        ? passedFinalExam
                          ? 'Examen completado'
                          : examLockedByProgress
                          ? 'Completa requeridas para habilitar'
                          : 'Iniciar examen'
                        : 'Inscríbete para habilitar'}
                    </Link>
                  </div>
                </div>
              )}

            </div>
          )}
        </div>
      </div>

      {/* Reproductor en pantalla completa */}
      {activeLesson && (
        <LessonPlayer
          courseId={course.id}
          lesson={activeLesson}
          courseTitle={course.title}
          allLessons={course.lessons}
          completedLessonIds={completedLessonIds}
          onClose={() => setActiveLesson(null)}
          onNavigate={setActiveLesson}
        />
      )}
    </>
  );
}

function getLessonIcon(type: LessonDto['type']) {
  if (type === 'video') {
    return (
      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
    );
  }

  if (type === 'pdf') {
    return (
      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
    );
  }

  return (
    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function CourseDetailSkeleton() {
  return (
    <div className="min-h-screen bg-[#0a0a0f]">
      <div className="border-b border-white/[0.06] h-10" />
      <div className="max-w-5xl mx-auto px-6 py-10 space-y-8 animate-pulse">
        <div className="flex gap-8">
          <div className="hidden md:block w-56 h-36 rounded-2xl bg-white/[0.04]" />
          <div className="flex-1 space-y-3">
            <div className="h-7 bg-white/[0.04] rounded-lg w-2/3" />
            <div className="h-4 bg-white/[0.04] rounded-lg w-full" />
            <div className="h-4 bg-white/[0.04] rounded-lg w-3/4" />
          </div>
        </div>
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="h-14 rounded-xl bg-white/[0.04]" />
          ))}
        </div>
      </div>
    </div>
  );
}

function getEnrollmentStatusLabel(status: unknown) {
  if (status === 0 || status === 'Active') return 'Activo';
  if (status === 1 || status === 'Completed') return 'Completado';
  if (status === 2 || status === 'Abandoned') return 'Abandonado';
  return 'Activo';
}
