import { type ComponentProps } from 'react';
import { cn } from '@/lib/utils';

/**
 * Bloque de carga con pulso (bg-secondary). Dale la forma del contenido que va a llegar con clases de tamaño
 * y radio (`h-4 w-48`, `h-48 rounded-2xl`), y marca el contenedor de la vista con `aria-busy`.
 * @category Feedback
 */
export function Skeleton({ className, ...props }: ComponentProps<'div'>) {
  return <div aria-hidden="true" className={cn('animate-pulse rounded-md bg-secondary', className)} {...props} />;
}
