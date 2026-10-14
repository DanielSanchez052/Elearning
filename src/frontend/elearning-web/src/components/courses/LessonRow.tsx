import { type ReactNode } from 'react';
import { Badge } from '@/components/ui';
import {
  CheckCircleIcon,
  ClockIcon,
  FileTextIcon,
  ListChecksIcon,
  LockIcon,
  VideoIcon,
} from '@/components/ui/icons';
import { cn } from '@/lib/utils';

export type LessonRowStatus = 'done' | 'pending' | 'locked';

const types: Record<string, { label: string; Icon: typeof VideoIcon }> = {
  video: { label: 'Video', Icon: VideoIcon },
  pdf: { label: 'PDF', Icon: FileTextIcon },
  quiz: { label: 'Quiz', Icon: ListChecksIcon },
};

const statuses = {
  done: { label: 'Completada', tone: 'success', Icon: CheckCircleIcon },
  pending: { label: 'Pendiente', tone: 'warning', Icon: ClockIcon },
  locked: { label: 'Bloqueada', tone: 'neutral', Icon: LockIcon },
} as const;

type LessonRowProps = {
  /** Posición visible (1, 2, 3…). */
  index: number;
  title: string;
  type: string;
  isRequired: boolean;
  /** Sin estado (p. ej. en el editor admin) no se muestra el Badge. */
  status?: LessonRowStatus;
  /** Lección en curso: resalta la fila con bg-brand/10. */
  current?: boolean;
  /** Explicación bajo el título cuando está bloqueada. */
  lockReason?: string;
  /** Clic en la fila (se ignora si está bloqueada). */
  onClick?: () => void;
  /** Asa de arrastre a la izquierda (editor de curso). */
  dragHandle?: ReactNode;
  /** Acciones a la derecha (editar, eliminar). */
  actions?: ReactNode;
  className?: string;
};

/**
 * Fila de lección del rediseño (detalle del curso, barra lateral del reproductor y editor admin):
 * número, chip con el tipo, título, «Tipo · Requerida/Opcional», y Badge de estado.
 */
export function LessonRow({
  index,
  title,
  type,
  isRequired,
  status,
  current = false,
  lockReason,
  onClick,
  dragHandle,
  actions,
  className,
}: LessonRowProps) {
  const locked = status === 'locked';
  const t = types[type] ?? { label: type, Icon: FileTextIcon };
  const s = status ? statuses[status] : undefined;
  const clickable = !!onClick && !locked;

  const content = (
    <>
      {dragHandle}
      <span className="w-6 shrink-0 text-center text-xs font-medium tabular-nums text-subtle-foreground">{index}</span>
      <span
        className={cn(
          'flex size-8 shrink-0 items-center justify-center rounded-lg',
          locked
            ? 'bg-secondary text-subtle-foreground'
            : status === 'done'
              ? 'bg-success/10 text-success-foreground'
              : 'bg-brand/10 text-brand-foreground'
        )}
      >
        <t.Icon className="size-4" aria-hidden="true" />
      </span>
      <span className="flex min-w-0 flex-1 flex-col text-left">
        <span className={cn('truncate text-sm font-medium', locked ? 'text-muted-foreground' : 'text-foreground')}>
          {title}
        </span>
        <span className="mt-0.5 text-xs text-subtle-foreground">
          {t.label} · {isRequired ? 'Requerida' : 'Opcional'}
        </span>
        {locked && lockReason && <span className="mt-1 text-xs text-muted-foreground">{lockReason}</span>}
      </span>
      {s && (
        <Badge tone={s.tone}>
          <s.Icon /> {s.label}
        </Badge>
      )}
      {actions}
    </>
  );

  const rowClass = cn(
    'flex w-full items-center gap-4 px-6 py-4',
    clickable && 'cursor-pointer transition-colors hover:bg-secondary focus-visible:bg-secondary focus-visible:outline-none',
    current && 'bg-brand/10',
    className
  );

  return clickable ? (
    <button type="button" onClick={onClick} aria-current={current ? 'step' : undefined} className={rowClass}>
      {content}
    </button>
  ) : (
    <div className={rowClass} aria-disabled={locked || undefined}>
      {content}
    </div>
  );
}
