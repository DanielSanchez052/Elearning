import { type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Avatar, Badge, Card } from '@/components/ui';
import { BookOpenIcon, GlobeIcon, ShieldIcon } from '@/components/ui/icons';
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
  /** Fecha ya formateada que se muestra al pie (p. ej. la de publicación). */
  date?: string;
  /** Badges extra sobre la miniatura junto al de alcance (p. ej. «Inscrito»). */
  badges?: ReactNode;
  className?: string;
};

/**
 * Tarjeta única de curso del rediseño: miniatura 16:9 con fallback, alcance como Badge (Global = brand,
 * Por país = neutral), título, descripción en dos líneas y una fila al pie con instructor, lecciones y fecha.
 * Toda la tarjeta es un enlace.
 */
export function CourseCard({
  to,
  title,
  description,
  thumbnailUrl,
  isGlobal,
  instructorName,
  lessonCount,
  date,
  badges,
  className,
}: CourseCardProps) {
  return (
    <Card
      variant="interactive"
      padding="none"
      className={cn('relative flex h-full flex-col overflow-hidden focus-within:ring-2 focus-within:ring-ring', className)}
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
              <Badge><ShieldIcon /> Por país</Badge>
            )}
            {badges}
          </>
        }
      />
      <div className="flex flex-1 flex-col gap-2 p-5">
        <h2 className="text-base font-semibold text-foreground">
          {/* El enlace cubre toda la tarjeta con ::after para que sea un solo destino accesible. */}
          <Link to={to} className="after:absolute after:inset-0 focus-visible:outline-none">
            {title}
          </Link>
        </h2>
        {description && <p className="line-clamp-2 text-sm text-muted-foreground">{description}</p>}
      </div>
      <div className="flex items-center gap-3 border-t border-border px-5 py-3">
        <Avatar name={instructorName} size="xs" tone="neutral" />
        <span className="min-w-0 flex-1 truncate text-xs text-foreground-soft">{instructorName}</span>
        <span className="flex shrink-0 items-center gap-1 text-xs text-muted-foreground">
          <BookOpenIcon className="size-3" aria-hidden="true" />
          {lessonCount} {lessonCount === 1 ? 'lección' : 'lecciones'}
        </span>
        {date && <span className="hidden shrink-0 text-xs text-subtle-foreground sm:inline">{date}</span>}
      </div>
    </Card>
  );
}
