import { cn } from '@/lib/utils';
import { LoaderIcon } from './icons';

export type SpinnerProps = {
  /** sm = 16px (dentro de botones), md = 24px, lg = 32px. */
  size?: 'sm' | 'md' | 'lg';
  /** Texto accesible; por defecto «Cargando». */
  label?: string;
  className?: string;
};

const sizes = { sm: 'size-4', md: 'size-6', lg: 'size-8' };

/**
 * Indicador giratorio para acciones puntuales (verificar email, reordenar lecciones). Para cargar una vista usa Skeleton.
 * @category Feedback
 */
export function Spinner({ size = 'md', label = 'Cargando', className }: SpinnerProps) {
  return (
    <span role="status" aria-label={label} className={cn('inline-flex text-brand-foreground', className)}>
      <LoaderIcon className={cn('animate-spin', sizes[size])} />
    </span>
  );
}
