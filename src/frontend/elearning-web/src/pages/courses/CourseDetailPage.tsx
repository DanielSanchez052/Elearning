import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useCourseDetail } from '@/hooks/useCourses';
import { useCourseExam, useCourseExamResults } from '@/hooks/quizzes';
import { useCourseProgress, useEnrollInCourse, useMyEnrollments } from '@/hooks/useEnrollments';
import { useDownloadCertificate, useMyCertificates } from '@/hooks/useCertificates';
import { Alert, Avatar, Badge, Button, Card, ProgressBar, Skeleton, StateBlock } from '@/components/ui';
import {
  ArrowLeftIcon,
  BookOpenIcon,
  ClipboardCheckIcon,
  DownloadIcon,
  GlobeIcon,
  GraduationCapIcon,
  LockIcon,
  PlayCircleIcon,
  ShieldIcon,
  TrophyIcon,
  XCircleIcon,
} from '@/components/ui/icons';
import { ButtonLink } from '@/components/ButtonLink';
import { CourseThumbnail } from '@/components/courses/CourseThumbnail';
import { LessonRow, type LessonRowStatus } from '@/components/courses/LessonRow';
import LessonPlayer from '@/components/courses/LessonPlayer';
import { getApiErrorMessage } from '@/lib/axios';
import { enrollmentStatusKey, requiredLessonsLabel } from '@/lib/enrollment';
import { formatDate } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { CourseDetailDto, LessonDto } from '@/types/course.types';

const httpStatus = (error: unknown) => (isAxiosError(error) ? error.response?.status : undefined);

function CourseDetailSkeleton() {
  return (
    <div className="grid gap-6 md:grid-cols-3" aria-busy="true">
      <Skeleton className="h-48 rounded-2xl md:col-span-2" />
      <Skeleton className="h-48 rounded-2xl" />
      <div className="space-y-3 md:col-span-2">
        {Array.from({ length: 4 }, (_, i) => (
          <Skeleton key={i} className="h-16 rounded-xl" />
        ))}
      </div>
      <Skeleton className="h-40 rounded-2xl" />
    </div>
  );
}

/** Contenido del curso ya cargado: inscripción, lecciones, examen final y reproductor. */
function CourseDetailContent({ course }: { course: CourseDetailDto }) {
  const navigate = useNavigate();
  const examQuery = useCourseExam(course.id);
  const progressQuery = useCourseProgress(course.id);
  const examResultsQuery = useCourseExamResults(course.id);
  const enrollmentsQuery = useMyEnrollments();
  const certificatesQuery = useMyCertificates();
  const certificateDownload = useDownloadCertificate();
  const enrollMutation = useEnrollInCourse();
  const [activeLesson, setActiveLesson] = useState<LessonDto | null>(null);

  const progress = progressQuery.data;
  const enrolled = Boolean(progress);
  const notEnrolled = progressQuery.isError && httpStatus(progressQuery.error) === 404;

  const lessons = [...course.lessons].sort((a, b) => a.orderIndex - b.orderIndex);
  const completed = new Set(progress?.lessons.filter((l) => l.isCompleted).map((l) => l.lessonId) ?? []);
  const completedLessonIds = [...completed];
  const requiredCount = lessons.filter((l) => l.isRequired).length;

  const statusOf = (lesson: LessonDto): LessonRowStatus => {
    if (!enrolled) return 'locked';
    if (completed.has(lesson.id)) return 'done';
    const missingRequiredBefore = lessons.some(
      (l) => l.isRequired && l.orderIndex < lesson.orderIndex && !completed.has(l.id)
    );
    return lesson.type === 'quiz' && missingRequiredBefore ? 'locked' : 'pending';
  };
  const currentLesson = enrolled ? lessons.find((l) => statusOf(l) === 'pending') : undefined;

  const openLesson = (lesson: LessonDto) => {
    if (lesson.type === 'quiz') navigate(`/courses/${course.id}/lessons/${lesson.id}/quiz`);
    else setActiveLesson(lesson);
  };

  const enrollmentSummary = enrollmentsQuery.data?.find((e) => e.courseId === course.id);
  const isCompleted = progress ? enrollmentStatusKey(progress.status) === 'completed' : false;

  // Examen final: el endpoint responde 403 mientras faltan lecciones requeridas.
  const examLocked = enrolled && httpStatus(examQuery.error) === 403;
  const hasExam = (examQuery.data?.length ?? 0) > 0 || examLocked;
  const examQuestions = examQuery.data ?? [];
  const passScore = examQuestions[0]?.passScore;
  const maxAttempts = examQuestions[0]?.maxAttempts;
  const attempts = [...(examResultsQuery.data ?? [])].sort((a, b) => a.attemptNumber - b.attemptNumber);
  const passed = attempts.find((a) => a.isPassed);
  const lastAttempt = attempts[attempts.length - 1];
  const attemptsLeft = maxAttempts ? Math.max(maxAttempts - attempts.length, 0) : undefined;
  const examState: 'locked' | 'ready' | 'passed' | 'failed' = !enrolled || examLocked
    ? 'locked'
    : passed
      ? 'passed'
      : lastAttempt
        ? 'failed'
        : 'ready';
  const certificate = certificatesQuery.data?.find((c) => c.courseId === course.id);

  const examChip = {
    locked: ['bg-secondary text-subtle-foreground', LockIcon],
    ready: ['bg-brand/10 text-brand-foreground', ClipboardCheckIcon],
    passed: ['bg-success/10 text-success-foreground', TrophyIcon],
    failed: ['bg-destructive/10 text-destructive-foreground', XCircleIcon],
  } as const;
  const [chipClass, ExamIcon] = examChip[examState];

  return (
    <div className="grid gap-6 md:grid-cols-3">
      {/* Encabezado del curso */}
      <Card padding="none" className="overflow-hidden md:col-span-2">
        <div className="flex h-full">
          <CourseThumbnail
            src={course.thumbnailUrl}
            title={course.title}
            className="hidden w-56 shrink-0 sm:block"
          />
          <div className="flex min-w-0 flex-1 flex-col gap-3 p-6">
            <div className="flex flex-wrap gap-2">
              {course.isGlobal ? (
                <Badge tone="brand"><GlobeIcon /> Global</Badge>
              ) : (
                <Badge><ShieldIcon /> {course.countries.map((c) => c.name).join(', ') || 'Por país'}</Badge>
              )}
              {enrolled && (
                <Badge tone={isCompleted ? 'success' : 'info'}>{isCompleted ? 'Completado' : 'Inscrito'}</Badge>
              )}
            </div>
            <h1 className="text-2xl font-semibold text-foreground">{course.title}</h1>
            {course.description && <p className="text-sm text-foreground-soft">{course.description}</p>}
            <div className="flex-1" />
            <div className="flex flex-wrap items-center gap-x-5 gap-y-2 pt-2 text-sm text-muted-foreground">
              <span className="flex items-center gap-2">
                <Avatar name={course.instructorName} size="xs" tone="neutral" />
                {course.instructorName}
              </span>
              <span className="flex items-center gap-1.5">
                <BookOpenIcon className="size-4" aria-hidden="true" />
                {lessons.length} {lessons.length === 1 ? 'lección' : 'lecciones'} · {requiredCount}{' '}
                {requiredCount === 1 ? 'requerida' : 'requeridas'}
              </span>
            </div>
          </div>
        </div>
      </Card>

      {/* Inscripción o progreso */}
      {progressQuery.isLoading ? (
        <Skeleton className="h-48 rounded-2xl" />
      ) : enrolled && progress ? (
        <Card className="flex flex-col justify-center gap-4">
          <div>
            <p className="text-base font-semibold text-foreground">Tu progreso</p>
            <p className="mt-1 text-xs text-subtle-foreground">
              Inscrito el {formatDate(progress.enrolledAt)}
              {enrollmentSummary?.deadlineAt && ` · Fecha límite: ${formatDate(enrollmentSummary.deadlineAt)}`}
            </p>
          </div>
          <ProgressBar
            value={Math.round(progress.progressPercent)}
            showValue
            tone="success"
            label={`${requiredLessonsLabel(progress.completedLessons, progress.requiredLessons)} completadas`}
          />
          {currentLesson && (
            <Button fullWidth onClick={() => openLesson(currentLesson)}>
              <PlayCircleIcon /> Continuar lección
            </Button>
          )}
        </Card>
      ) : notEnrolled ? (
        <Card className="flex flex-col justify-center gap-4">
          <span className="flex size-10 items-center justify-center rounded-xl bg-brand/10 text-brand-foreground">
            <GraduationCapIcon className="size-5" aria-hidden="true" />
          </span>
          <div>
            <p className="text-base font-semibold text-foreground">Aún no estás inscrito</p>
            <p className="mt-1 text-sm text-muted-foreground">Inscríbete para desbloquear las lecciones y el examen final.</p>
          </div>
          {enrollMutation.isError && <Alert tone="danger">{getApiErrorMessage(enrollMutation.error)}</Alert>}
          <Button size="lg" fullWidth onClick={() => enrollMutation.mutate(course.id)} disabled={enrollMutation.isPending}>
            {enrollMutation.isPending ? 'Inscribiendo…' : 'Inscribirme al curso'}
          </Button>
        </Card>
      ) : (
        <StateBlock
          tipo="error"
          compact
          title="No pudimos verificar tu inscripción"
          text="Inténtalo de nuevo en unos segundos."
          onAction={() => progressQuery.refetch()}
        />
      )}

      {/* Lecciones */}
      <Card padding="none" className="overflow-hidden md:col-span-2">
        <div className="flex items-center justify-between gap-3 border-b border-border px-6 py-4">
          <h2 className="text-lg font-semibold text-foreground">Lecciones</h2>
          <span className="text-sm text-muted-foreground">
            {enrolled
              ? `${completed.size} de ${lessons.length} completadas`
              : `${lessons.length} ${lessons.length === 1 ? 'lección' : 'lecciones'}`}
          </span>
        </div>
        {lessons.length === 0 ? (
          <StateBlock bordered={false} compact title="Este curso aún no tiene lecciones" text="El instructor todavía está preparando el contenido." />
        ) : (
          <ol className="divide-y divide-border">
            {lessons.map((lesson, index) => {
              const status = statusOf(lesson);
              return (
                <li key={lesson.id}>
                  <LessonRow
                    index={index + 1}
                    title={lesson.title}
                    type={lesson.type}
                    isRequired={lesson.isRequired}
                    status={status}
                    current={lesson.id === currentLesson?.id}
                    lockReason={
                      enrolled
                        ? 'Se desbloquea al completar las lecciones requeridas anteriores.'
                        : undefined
                    }
                    onClick={() => openLesson(lesson)}
                  />
                </li>
              );
            })}
          </ol>
        )}
      </Card>

      {/* Examen final */}
      <div>
        <Card>
          <div className="flex items-center gap-3">
            <span className={cn('flex size-10 shrink-0 items-center justify-center rounded-xl', hasExam ? chipClass : 'bg-secondary text-subtle-foreground')}>
              <ExamIcon className="size-5" aria-hidden="true" />
            </span>
            <div>
              <h2 className="text-base font-semibold text-foreground">Examen final</h2>
              {examQuestions.length > 0 && (
                <p className="text-xs text-subtle-foreground">
                  {examQuestions.length} {examQuestions.length === 1 ? 'pregunta' : 'preguntas'}
                  {passScore !== undefined && ` · Mínimo para aprobar: ${passScore}%`}
                  {maxAttempts !== undefined && ` · ${maxAttempts} ${maxAttempts === 1 ? 'intento' : 'intentos'}`}
                </p>
              )}
            </div>
          </div>

          {examQuery.isLoading && enrolled ? (
            <Skeleton className="mt-4 h-20 rounded-xl" />
          ) : !hasExam ? (
            <p className="mt-4 text-sm text-muted-foreground">
              Este curso no tiene examen final. Se completa al terminar las lecciones requeridas.
            </p>
          ) : examState === 'locked' ? (
            <>
              <p className="mt-4 text-sm text-muted-foreground">
                {enrolled
                  ? 'Completa las lecciones requeridas para habilitar el examen final.'
                  : 'Inscríbete y completa las lecciones requeridas para habilitar el examen final.'}
              </p>
              <Button variant="secondary" fullWidth disabled className="mt-4">
                <LockIcon /> Iniciar examen
              </Button>
            </>
          ) : examState === 'ready' ? (
            <>
              <p className="mt-4 text-sm text-muted-foreground">
                Completaste las lecciones requeridas. Al aprobar obtendrás tu certificado.
              </p>
              <ButtonLink to={`/courses/${course.id}/exam`} fullWidth className="mt-4">
                Iniciar examen
              </ButtonLink>
            </>
          ) : examState === 'passed' && passed ? (
            <>
              <div className="mt-4 flex items-center justify-between rounded-xl bg-success/10 px-4 py-3">
                <span className="text-sm font-medium text-success-foreground">Aprobado</span>
                <span className="text-2xl font-semibold text-foreground tabular-nums">{Math.round(passed.score)}%</span>
              </div>
              <p className="mt-2 text-xs text-subtle-foreground">
                Intento {passed.attemptNumber}
                {maxAttempts ? ` de ${maxAttempts}` : ''} · {formatDate(passed.completedAt)}
              </p>
              {certificate && (
                <Button
                  variant="outline"
                  fullWidth
                  className="mt-4"
                  onClick={() => certificateDownload.download(certificate)}
                  disabled={certificateDownload.downloadingId === certificate.id}
                >
                  <DownloadIcon /> {certificateDownload.downloadingId === certificate.id ? 'Descargando…' : 'Descargar certificado'}
                </Button>
              )}
              {certificateDownload.error && (
                <p className="mt-2 text-xs text-destructive-foreground" role="alert">{certificateDownload.error}</p>
              )}
            </>
          ) : lastAttempt ? (
            <>
              <div className="mt-4 flex items-center justify-between rounded-xl bg-destructive/10 px-4 py-3">
                <span className="text-sm font-medium text-destructive-foreground">No aprobado</span>
                <span className="text-2xl font-semibold text-foreground tabular-nums">{Math.round(lastAttempt.score)}%</span>
              </div>
              <p className="mt-2 text-xs text-subtle-foreground">
                Intento {lastAttempt.attemptNumber}
                {maxAttempts ? ` de ${maxAttempts}` : ''}
                {attemptsLeft !== undefined &&
                  ` · ${attemptsLeft === 0 ? 'Sin intentos disponibles' : `Te ${attemptsLeft === 1 ? 'queda 1 intento' : `quedan ${attemptsLeft} intentos`}`}`}
              </p>
              {attemptsLeft !== 0 && (
                <ButtonLink to={`/courses/${course.id}/exam`} fullWidth className="mt-4">
                  Reintentar examen
                </ButtonLink>
              )}
            </>
          ) : null}
        </Card>
      </div>

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
    </div>
  );
}

export default function CourseDetailPage() {
  const { id = '' } = useParams<{ id: string }>();
  const courseQuery = useCourseDetail(id);
  const course = courseQuery.data;

  return (
    <main className="mx-auto flex w-full max-w-7xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link to="/courses" className="flex w-fit items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeftIcon className="size-4" aria-hidden="true" /> Cursos
      </Link>

      {courseQuery.isLoading ? (
        <CourseDetailSkeleton />
      ) : courseQuery.isError || !course ? (
        <StateBlock
          tipo="error"
          title="No pudimos cargar este curso"
          text="Puede que el enlace ya no sea válido o que haya un problema de conexión."
          onAction={() => courseQuery.refetch()}
        />
      ) : (
        <CourseDetailContent course={course} />
      )}
    </main>
  );
}
