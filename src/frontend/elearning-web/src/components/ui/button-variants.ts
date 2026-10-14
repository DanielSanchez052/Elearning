import { cva } from 'class-variance-authority';

/** Estilos de Button, compartidos con enlaces que se ven como botón (ButtonLink en la app). */
export const buttonVariants = cva(
  'inline-flex items-center justify-center gap-2 whitespace-nowrap font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 [&_svg]:size-4 [&_svg]:shrink-0',
  {
    variants: {
      variant: {
        // Acción principal: guardar, crear, inscribirse
        primary: 'bg-primary text-primary-foreground hover:bg-primary-hover',
        // Acción secundaria sobre superficie translúcida: cancelar, paginación
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary-hover hover:text-foreground',
        // Contorno: acciones terciarias junto a una primaria
        outline: 'border border-border-strong text-foreground-soft hover:bg-secondary-hover hover:text-foreground',
        // Sin fondo: navegación, acciones en tablas
        ghost: 'text-muted-foreground hover:bg-secondary hover:text-foreground',
        // Acento suave de marca: filtros activos, acciones destacadas no principales
        soft: 'border border-brand/30 bg-brand/20 text-brand-foreground hover:bg-brand/30',
        // Confirmación positiva: completar, aprobar
        success: 'bg-success text-white hover:bg-success/90',
        // Acción destructiva discreta: eliminar en tablas, salir
        danger: 'text-destructive-foreground hover:bg-destructive/10',
        // Acción destructiva principal: confirmar eliminación
        destructive: 'bg-destructive text-white hover:bg-destructive/90',
      },
      size: {
        sm: 'rounded-lg px-3 py-1.5 text-xs',
        md: 'rounded-xl px-4 py-2 text-sm',
        lg: 'rounded-xl px-6 py-2.5 text-sm',
        icon: 'size-7 rounded-lg',
      },
      fullWidth: {
        true: 'w-full',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
    },
  }
);
