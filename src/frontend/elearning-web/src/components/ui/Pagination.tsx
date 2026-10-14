import { cn } from '@/lib/utils';
import { Button } from './Button';
import { ChevronLeftIcon, ChevronRightIcon } from './icons';

export type PaginationProps = {
  /** Página actual, empezando en 1. */
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  /** Texto opcional a la izquierda, p. ej. «Mostrando 1–8 de 248». */
  summary?: string;
  className?: string;
};

/** Páginas visibles: la primera, la última y las vecinas de la actual, con huecos ("…"). */
function visiblePages(page: number, totalPages: number): (number | 'gap')[] {
  const pages = Array.from({ length: totalPages }, (_, i) => i + 1).filter(
    (p) => p === 1 || p === totalPages || Math.abs(p - page) <= 1
  );
  const out: (number | 'gap')[] = [];
  pages.forEach((p, i) => {
    if (i > 0 && p - pages[i - 1] > 1) out.push('gap');
    out.push(p);
  });
  return out;
}

/**
 * Paginación numérica (‹ 1 … 4 5 6 … 20 ›) para el pie de listas y tablas. No se muestra si hay una sola página.
 * @category Navigation
 */
export function Pagination({ page, totalPages, onPageChange, summary, className }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav aria-label="Paginación" className={cn('flex flex-wrap items-center justify-between gap-3', className)}>
      <p className="text-xs text-subtle-foreground">{summary}</p>
      <div className="flex items-center gap-1">
        <Button
          variant="ghost"
          size="icon"
          aria-label="Página anterior"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          <ChevronLeftIcon />
        </Button>
        {visiblePages(page, totalPages).map((p, i) =>
          p === 'gap' ? (
            <span key={`gap-${i}`} className="px-1 text-xs text-subtle-foreground">…</span>
          ) : (
            <Button
              key={p}
              variant={p === page ? 'secondary' : 'ghost'}
              size="sm"
              aria-current={p === page ? 'page' : undefined}
              onClick={() => onPageChange(p)}
              className={cn('min-w-8 tabular-nums', p === page && 'text-foreground')}
            >
              {p}
            </Button>
          )
        )}
        <Button
          variant="ghost"
          size="icon"
          aria-label="Página siguiente"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          <ChevronRightIcon />
        </Button>
      </div>
    </nav>
  );
}
