import { useEffect, useRef, useState } from 'react';
import type { LessonDto } from '@/types/course.types';
import { useMarkLessonComplete } from '@/hooks/useEnrollments';
import { Alert, Badge, Button, Card, ProgressBar, StateBlock } from '@/components/ui';
import {
  ArrowLeftIcon,
  CheckCircleIcon,
  CheckIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ClockIcon,
  DownloadIcon,
  FileTextIcon,
  InfoIcon,
  ListChecksIcon,
  LockIcon,
  PlayCircleIcon,
  VideoIcon,
} from '@/components/ui/icons';
import { requiredLessonsLabel } from '@/lib/enrollment';
import { cn } from '@/lib/utils';
import type { LessonRowStatus } from './LessonRow';

const typeMeta: Record<string, { label: string; Icon: typeof VideoIcon }> = {
  video: { label: 'Video', Icon: VideoIcon },
  pdf: { label: 'PDF', Icon: FileTextIcon },
  quiz: { label: 'Quiz', Icon: ListChecksIcon },
};

interface LessonPlayerProps {
  courseId: string;
  courseTitle: string;
  lesson: LessonDto;
  /** Lecciones del curso ya ordenadas. */
  lessons: LessonDto[];
  completedLessonIds: string[];
  completedRequired: number;
  totalRequired: number;
  statusOf: (lesson: LessonDto) => LessonRowStatus;
  onClose: () => void;
  /** Abre una lección: el reproductor para video/PDF o la evaluación para un quiz. */
  onOpenLesson: (lesson: LessonDto) => void;
}

/**
 * Reproductor de lecciones (Reproductor del rediseño): contenido (video MP4 o PDF), datos de la lección,
 * marcar como completada, anterior/siguiente y la lista «Contenido del curso» con el progreso.
 */
export default function LessonPlayer({
  courseId,
  courseTitle,
  lesson,
  lessons,
  completedLessonIds,
  completedRequired,
  totalRequired,
  statusOf,
  onClose,
  onOpenLesson,
}: LessonPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const markLessonComplete = useMarkLessonComplete();
  // El aviso pertenece a una lección: al cambiar de lección deja de mostrarse sin resetear estado en un efecto.
  const [feedback, setFeedback] = useState<{ lessonId: string; ok: boolean; text: string } | null>(null);
  const [listOpen, setListOpen] = useState(false);

  const isCompleted = completedLessonIds.includes(lesson.id);
  const index = lessons.findIndex((l) => l.id === lesson.id);
  const prev = index > 0 ? lessons[index - 1] : null;
  const next = index >= 0 && index < lessons.length - 1 ? lessons[index + 1] : null;
  const canGoNext = next ? statusOf(next) !== 'locked' : false;
  const meta = typeMeta[lesson.type] ?? typeMeta.video;
  const hasContent = Boolean(lesson.contentUrl) && (lesson.type === 'video' || lesson.type === 'pdf');
  const shownFeedback = feedback?.lessonId === lesson.id ? feedback : null;

  const handleMarkComplete = async () => {
    if (lesson.type === 'quiz' || isCompleted || markLessonComplete.isPending) return;
    try {
      const result = await markLessonComplete.mutateAsync({ courseId, lessonId: lesson.id });
      setFeedback({
        lessonId: lesson.id,
        ok: true,
        text: result.data.courseCompleted ? '¡Completaste todas las lecciones requeridas del curso!' : 'Lección marcada como completada.',
      });
    } catch {
      setFeedback({ lessonId: lesson.id, ok: false, text: 'No se pudo marcar la lección. Intenta nuevamente.' });
    }
  };

  // Reiniciar el video y subir al inicio al cambiar de lección.
  useEffect(() => {
    window.scrollTo({ top: 0 });
    if (videoRef.current) {
      videoRef.current.load();
      videoRef.current.play().catch(() => {});
    }
  }, [lesson.id]);

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center gap-2 text-sm">
        <button type="button" onClick={onClose} className="flex items-center gap-1.5 text-muted-foreground hover:text-foreground">
          <ArrowLeftIcon className="size-4" aria-hidden="true" /> {courseTitle}
        </button>
        <span className="text-subtle-foreground" aria-hidden="true">/</span>
        <span className="text-foreground-soft">
          Lección {index + 1} de {lessons.length}
        </span>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="flex min-w-0 flex-col gap-5 md:col-span-2">
          {!hasContent ? (
            <StateBlock
              icon={meta.Icon}
              title="Esta lección aún no tiene contenido"
              text="El instructor todavía no ha subido el archivo. Puedes continuar con la siguiente lección."
              action={next && canGoNext ? 'Siguiente lección' : undefined}
              actionVariant="secondary"
              onAction={() => next && onOpenLesson(next)}
            />
          ) : lesson.type === 'video' ? (
            <div className="overflow-hidden rounded-2xl border border-border bg-sunken">
              <video
                ref={videoRef}
                controls
                onEnded={handleMarkComplete}
                className="aspect-video w-full bg-sunken"
              >
                <source src={lesson.contentUrl ?? undefined} type="video/mp4" />
                Tu navegador no soporta la reproducción de video.
              </video>
            </div>
          ) : (
            <div className="overflow-hidden rounded-2xl border border-border bg-sunken">
              <div className="flex items-center gap-2 border-b border-border bg-card px-4 py-2">
                <span className="text-xs text-foreground-soft">Documento PDF</span>
                <div className="flex-1" />
                <a
                  href={lesson.contentUrl ?? undefined}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-2 rounded-lg px-3 py-1.5 text-xs font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground"
                >
                  <DownloadIcon className="size-4" aria-hidden="true" /> Descargar
                </a>
              </div>
              <iframe
                src={`${lesson.contentUrl}#toolbar=1&navpanes=0`}
                title={`PDF: ${lesson.title}`}
                className="block h-[70vh] w-full border-0 bg-sunken"
              />
            </div>
          )}

          <div className="flex flex-col gap-2">
            <div className="flex flex-wrap items-center gap-2">
              <Badge>
                <meta.Icon /> {meta.label}
              </Badge>
              <Badge>{lesson.isRequired ? 'Requerida' : 'Opcional'}</Badge>
              {isCompleted && (
                <Badge tone="success">
                  <CheckCircleIcon /> Completada
                </Badge>
              )}
            </div>
            <h1 className="text-2xl font-semibold text-foreground">{lesson.title}</h1>
          </div>

          {shownFeedback && <Alert tone={shownFeedback.ok ? 'success' : 'danger'}>{shownFeedback.text}</Alert>}

          <div className="flex flex-wrap items-center gap-3 border-t border-border pt-5">
            {hasContent && !isCompleted && lesson.type === 'video' && (
              <p className="flex items-center gap-2 text-sm text-muted-foreground">
                <InfoIcon className="size-4" aria-hidden="true" /> Se marcará como completada al terminar el video.
              </p>
            )}
            {hasContent && !isCompleted && (
              <Button variant="success" onClick={handleMarkComplete} disabled={markLessonComplete.isPending}>
                <CheckIcon /> {markLessonComplete.isPending ? 'Guardando…' : 'Marcar completada'}
              </Button>
            )}
            <div className="flex-1" />
            <div className="flex gap-2">
              <Button variant="outline" onClick={() => prev && onOpenLesson(prev)} disabled={!prev}>
                <ChevronLeftIcon /> Anterior
              </Button>
              <Button variant="secondary" onClick={() => next && onOpenLesson(next)} disabled={!canGoNext}>
                Siguiente <ChevronRightIcon />
              </Button>
            </div>
          </div>
        </div>

        <Card padding="none" className="self-start overflow-hidden">
          <div className="space-y-3 border-b border-border px-5 py-4">
            <button
              type="button"
              onClick={() => setListOpen((o) => !o)}
              aria-expanded={listOpen}
              aria-controls="lesson-player-list"
              className="flex w-full items-center justify-between gap-2 text-left md:pointer-events-none"
            >
              <span className="text-sm font-semibold text-foreground">Contenido del curso</span>
              <span className="flex items-center gap-2 text-xs text-muted-foreground tabular-nums">
                {index + 1} de {lessons.length}
                <ChevronDownIcon className={cn('size-4 transition-transform md:hidden', listOpen && 'rotate-180')} aria-hidden="true" />
              </span>
            </button>
            <ProgressBar
              value={totalRequired ? Math.round((completedRequired / totalRequired) * 100) : 0}
              size="sm"
              tone="success"
              label={requiredLessonsLabel(completedRequired, totalRequired)}
            />
          </div>
          <ol id="lesson-player-list" className={cn('divide-y divide-border md:block', !listOpen && 'hidden')}>
            {lessons.map((l) => {
              const status = statusOf(l);
              const isCurrent = l.id === lesson.id;
              const locked = status === 'locked';
              const Icon = status === 'done' ? CheckCircleIcon : locked ? LockIcon : isCurrent ? PlayCircleIcon : ClockIcon;
              const typeLabel = typeMeta[l.type]?.label ?? l.type;
              return (
                <li key={l.id}>
                  <button
                    type="button"
                    disabled={locked || isCurrent}
                    onClick={() => onOpenLesson(l)}
                    aria-current={isCurrent ? 'step' : undefined}
                    className={cn(
                      'flex w-full items-center gap-3 px-5 py-3 text-left transition-colors disabled:cursor-default',
                      isCurrent ? 'bg-brand/10' : !locked && 'hover:bg-secondary'
                    )}
                  >
                    <Icon
                      className={cn(
                        'size-4 shrink-0',
                        status === 'done' ? 'text-success-foreground' : isCurrent ? 'text-brand-foreground' : 'text-subtle-foreground'
                      )}
                      aria-hidden="true"
                    />
                    <span className="min-w-0 flex-1">
                      <span
                        className={cn(
                          'block truncate text-sm',
                          isCurrent ? 'font-medium text-brand-foreground' : locked ? 'text-muted-foreground' : 'text-foreground-soft'
                        )}
                      >
                        {l.title}
                      </span>
                      <span className="block text-xs text-subtle-foreground">
                        {typeLabel}
                        {isCurrent ? ' · Lección actual' : locked ? ' · Bloqueada' : ''}
                      </span>
                    </span>
                  </button>
                </li>
              );
            })}
          </ol>
        </Card>
      </div>
    </div>
  );
}
