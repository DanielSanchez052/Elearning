import { type ReactNode } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';
import { AlertTriangleIcon, CheckCircleIcon, InfoIcon, XCircleIcon } from './icons';

const alertVariants = cva('flex items-start gap-3 rounded-xl border px-4 py-3 text-sm', {
  variants: {
    tone: {
      info: 'border-info/30 bg-info/10 text-info-foreground',
      success: 'border-success/30 bg-success/10 text-success-foreground',
      warning: 'border-warning/30 bg-warning/10 text-warning-foreground',
      danger: 'border-destructive/30 bg-destructive/10 text-destructive-foreground',
    },
  },
  defaultVariants: {
    tone: 'info',
  },
});

const icons = { info: InfoIcon, success: CheckCircleIcon, warning: AlertTriangleIcon, danger: XCircleIcon };

export type AlertProps = VariantProps<typeof alertVariants> & {
  title?: ReactNode;
  children?: ReactNode;
  className?: string;
};

/**
 * Mensaje en línea con ícono (error de autenticación, «Cuenta creada, verifica tu email», avisos en formularios).
 * `tone`: info | success | warning | danger. danger se anuncia con role="alert".
 * @category Feedback
 */
export function Alert({ tone, title, children, className }: AlertProps) {
  const Icon = icons[tone ?? 'info'];

  return (
    <div role={tone === 'danger' ? 'alert' : 'status'} className={cn(alertVariants({ tone }), className)}>
      <Icon className="mt-0.5 size-4 shrink-0" />
      <div className="flex min-w-0 flex-col gap-0.5">
        {title && <p className="font-medium">{title}</p>}
        {children && <div className={cn(title && 'text-foreground-soft')}>{children}</div>}
      </div>
    </div>
  );
}
