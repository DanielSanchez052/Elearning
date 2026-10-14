import { type ComponentProps } from 'react';
import { cn } from '@/lib/utils';

/**
 * Superficie raíz de la app (tema oscuro): fondo de página bg-background y texto text-foreground.
 * Envuelve cada pantalla; sin ella los componentes quedan sobre el fondo del documento.
 * Para una página completa: <AppRoot className="min-h-screen">.
 * @category Layout
 */
export function AppRoot({ className, ...props }: ComponentProps<'div'>) {
  return <div className={cn('bg-background text-foreground antialiased', className)} {...props} />;
}
