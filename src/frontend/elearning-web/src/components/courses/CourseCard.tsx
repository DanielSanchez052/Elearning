import { type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Avatar, Badge, Card } from '@/components/ui';
import { BookOpenIcon, GlobeIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';
import { CourseThumbnail } from './CourseThumbnail';

type CourseCardProps = {
  to: string;
  title: string;
  description: string | null;
  thumbnailUrl: string | null;
  isGlobal: boolean;
  instructorName: string;
  lessonCount: number;
  /** Badges extra sobre la miniatura junto al de alcance (p. ej. «Inscrito»). */
  badges?: ReactNode;
  /** Pie opcional bajo los metadatos (progreso, acción). */
  footer?: ReactNode;
  className?: string;
};

/**
 * Tarjeta única de curso del rediseño: miniatura 16:9 con fallback, alcance como Badge (Global = brand,
 * Por país = neutral), descripción en dos líneas, instructor y número de lecciones. Toda la tarjeta es un enlace.
 */
export function CourseCard({
  to,
  title,
  description,
  thumbnailUrl,
  isGlobal,
  instructorName,
  lessonCount,
  badges,
  footer,
  className,
}: CourseCardProps) {
  return (
    <Card
      variant="interactive"
      padding="none"
      className={cn(
        'group relative flex flex-col overflow-hidden focus-within:ring-2 focus-within:ring-ring',
        className
      )}
    >
      <CourseThumbnail
        src={thumbnailUrl}
        title={title}
        showTitleOnFallback
        overlay={
          <>
            {isGlobal ? (
              <Badge tone="brand"><GlobeIcon /> Global</Badge>
            ) : (
              <Badge>Por país</Badge>
            )}
            {badges}
          </>
        }
      />
      <div className="flex flex-1 flex-col gap-3 p-5">
        <div className="space-y-1">
          <h3 className="line-clamp-2 text-base font-semibold text-foreground">
            {/* El enlace cubre toda la tarjeta con ::after para que sea un solo destino accesible. */}
            <Link to={to} className="after:absolute after:inset-0 focus-visible:outline-none">
              {title}
            </Link>
          </h3>
          {description && <p className="line-clamp-2 text-sm text-muted-foreground">{description}</p>}
        </div>
        <div className="mt-auto flex flex-wrap items-center gap-x-4 gap-y-2 text-xs text-subtle-foreground">
          <span className="flex items-center gap-2">
            <Avatar name={instructorName} size="xs" tone="neutral" />
            {instructorName}
          </span>
          <span className="flex items-center gap-1.5">
            <BookOpenIcon className="size-3.5" aria-hidden="true" />
            {lessonCount} {lessonCount === 1 ? 'lección' : 'lecciones'}
          </span>
        </div>
        {footer && <div className="relative z-10">{footer}</div>}
      </div>
    </Card>
  );
}
