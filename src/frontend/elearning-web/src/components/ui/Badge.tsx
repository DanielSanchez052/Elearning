import { type ComponentProps } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-medium whitespace-nowrap [&_svg]:size-3',
  {
    variants: {
      tone: {
        neutral: 'border-border-strong bg-secondary text-muted-foreground',
        brand: 'border-brand/30 bg-brand/20 text-brand-foreground',
        success: 'border-success/30 bg-success/10 text-success-foreground',
        warning: 'border-warning/30 bg-warning/10 text-warning-foreground',
        info: 'border-info/30 bg-info/10 text-info-foreground',
        danger: 'border-destructive/30 bg-destructive/10 text-destructive-foreground',
      },
    },
    defaultVariants: {
      tone: 'neutral',
    },
  }
);

export type BadgeProps = ComponentProps<'span'> & VariantProps<typeof badgeVariants>;

/**
 * Etiqueta de estado en forma de píldora: "Inscrito", "Activo", "Global", rol del usuario.
 * `tone`: neutral | brand | success | warning | info | danger.
 * @category Feedback
 */
export function Badge({ className, tone, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ tone }), className)} {...props} />;
}
