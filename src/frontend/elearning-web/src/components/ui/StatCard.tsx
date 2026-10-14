import { type ComponentType, type ReactNode } from 'react';
import { cn } from '@/lib/utils';

export type StatCardProps = {
  label: string;
  /** Valor principal (p. ej. «90%», «9 / 10»). Si se omite, la tarjeta se muestra como placeholder «Próximamente». */
  value?: ReactNode;
  /** Texto secundario bajo el valor. */
  hint?: ReactNode;
  icon?: ComponentType<{ className?: string }>;
  /** Color del valor: default (foreground) | success | danger. */
  tone?: 'default' | 'success' | 'danger';
  className?: string;
};

const valueTones = {
  default: 'text-foreground',
  success: 'text-success-foreground',
  danger: 'text-destructive-foreground',
};

/**
 * Tarjeta de estadística: etiqueta + valor grande + ayuda. Sin `value` se pinta como placeholder punteado
 * (métricas del dashboard admin que aún no tienen endpoint).
 * @category Data display
 */
export function StatCard({ label, value, hint, icon: Icon, tone = 'default', className }: StatCardProps) {
  const placeholder = value === undefined;

  return (
    <div
      className={cn(
        'flex flex-col gap-4 rounded-2xl p-6',
        placeholder ? 'border border-dashed border-border-strong' : 'border border-border bg-card',
        className
      )}
    >
      <div className="flex items-center justify-between gap-2">
        <p className="text-sm font-medium text-foreground-soft">{label}</p>
        {Icon && <Icon className="size-4 text-subtle-foreground" />}
      </div>
      {placeholder ? (
        <div className="space-y-2" aria-hidden="true">
          <div className="h-8 w-16 rounded-md bg-secondary" />
          <div className="h-3 w-3/4 rounded-md bg-secondary" />
        </div>
      ) : (
        <p className={cn('text-3xl font-semibold tabular-nums', valueTones[tone])}>{value}</p>
      )}
      {hint && <p className="text-xs text-subtle-foreground">{hint}</p>}
    </div>
  );
}
