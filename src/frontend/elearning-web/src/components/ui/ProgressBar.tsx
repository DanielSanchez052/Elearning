import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const trackVariants = cva('w-full overflow-hidden rounded-full bg-secondary-hover', {
  variants: {
    size: {
      sm: 'h-1.5',
      md: 'h-2.5',
    },
  },
  defaultVariants: {
    size: 'md',
  },
});

const fillVariants = cva('h-full rounded-full transition-all duration-300 ease-out', {
  variants: {
    tone: {
      brand: 'bg-brand',
      success: 'bg-success',
      warning: 'bg-warning',
      danger: 'bg-destructive',
    },
  },
  defaultVariants: {
    tone: 'brand',
  },
});

export type ProgressBarProps = VariantProps<typeof trackVariants> &
  VariantProps<typeof fillVariants> & {
    /** Porcentaje 0–100; se recorta a ese rango. */
    value: number;
    label?: string;
    showValue?: boolean;
    className?: string;
  };

/**
 * Barra de progreso (role="progressbar"). `value` es un porcentaje 0–100; `label` y `showValue` muestran texto encima.
 * `tone`: brand (subidas, avance general) | success (progreso del curso) | warning | danger. `size`: sm | md.
 * @category Feedback
 */
export function ProgressBar({ value, label, showValue = false, size, tone, className }: ProgressBarProps) {
  const percentage = Math.round(Math.min(Math.max(value, 0), 100));

  return (
    <div className={cn('flex flex-col gap-2', className)}>
      {(label || showValue) && (
        <div className="flex items-center justify-between text-xs">
          {label && <span className="text-muted-foreground">{label}</span>}
          {showValue && <span className="font-medium tabular-nums text-foreground-soft">{percentage}%</span>}
        </div>
      )}
      <div
        className={trackVariants({ size })}
        role="progressbar"
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={percentage}
        aria-label={label}
      >
        <div className={fillVariants({ tone })} style={{ width: `${percentage}%` }} />
      </div>
    </div>
  );
}
