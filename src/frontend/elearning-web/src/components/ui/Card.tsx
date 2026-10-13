import { type ComponentProps } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const cardVariants = cva('rounded-2xl border text-card-foreground', {
  variants: {
    variant: {
      // Panel principal: formularios, detalle, resúmenes
      default: 'border-border bg-card',
      // Elemento de lista dentro de otro panel
      subtle: 'border-border bg-secondary',
      // Fila clicable o tarjeta de catálogo
      interactive: 'border-border bg-card transition-colors hover:border-brand/30',
    },
    padding: {
      none: '',
      sm: 'p-4',
      md: 'p-6',
      lg: 'p-8',
    },
    elevated: {
      true: 'shadow-2xl',
    },
  },
  defaultVariants: {
    variant: 'default',
    padding: 'md',
  },
});

export type CardProps = ComponentProps<'div'> & VariantProps<typeof cardVariants>;

/**
 * Contenedor de superficie (bg-card, borde, rounded-2xl). Compón el encabezado con CardHeader > CardTitle + CardDescription.
 * `variant`: default (panel principal), subtle (elemento dentro de otro panel), interactive (fila o tarjeta clicable).
 * `padding`: none | sm | md | lg; `elevated` agrega sombra (modales, formularios centrados).
 * @category Layout
 */
export function Card({ className, variant, padding, elevated, ...props }: CardProps) {
  return <div className={cn(cardVariants({ variant, padding, elevated }), className)} {...props} />;
}

/** Encabezado de Card: apila CardTitle y CardDescription con margen inferior. */
export function CardHeader({ className, ...props }: ComponentProps<'div'>) {
  return <div className={cn('mb-5 flex flex-col gap-1', className)} {...props} />;
}

/** Título de Card (h3, texto base semibold). */
export function CardTitle({ className, ...props }: ComponentProps<'h3'>) {
  return <h3 className={cn('text-base font-semibold text-foreground', className)} {...props} />;
}

/** Texto secundario bajo CardTitle. */
export function CardDescription({ className, ...props }: ComponentProps<'p'>) {
  return <p className={cn('text-sm text-muted-foreground', className)} {...props} />;
}
