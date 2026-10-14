import { useState, type ReactNode } from 'react';
import { BookOpenIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';

type CourseThumbnailProps = {
  src: string | null | undefined;
  /** Título del curso: texto alternativo de la imagen y del fallback. */
  title: string;
  /** Contenido superpuesto en la esquina superior izquierda (p. ej. el Badge de alcance). */
  overlay?: ReactNode;
  /** Muestra el título dentro del fallback (tarjetas grandes); en miniaturas pequeñas solo el ícono. */
  showTitleOnFallback?: boolean;
  className?: string;
};

/**
 * Miniatura 16:9 de un curso. Si no hay imagen o falla la carga, muestra el fallback índigo
 * (ícono de libro y, opcionalmente, el título) definido en el rediseño.
 */
export function CourseThumbnail({ src, title, overlay, showTitleOnFallback = false, className }: CourseThumbnailProps) {
  const [failed, setFailed] = useState(false);
  const showImage = !!src && !failed;

  return (
    <div className={cn('relative aspect-video w-full overflow-hidden bg-brand/10', className)}>
      {showImage ? (
        <img
          src={src}
          alt={title}
          loading="lazy"
          onError={() => setFailed(true)}
          className="h-full w-full object-cover"
        />
      ) : (
        <div className="flex h-full w-full flex-col items-center justify-center gap-2 p-4 text-center">
          <BookOpenIcon className="size-6 text-brand-foreground" aria-hidden="true" />
          {showTitleOnFallback && (
            <span className="line-clamp-2 text-sm font-medium text-brand-foreground">{title}</span>
          )}
        </div>
      )}
      {overlay && <div className="absolute top-3 left-3 flex flex-wrap gap-1.5">{overlay}</div>}
    </div>
  );
}
