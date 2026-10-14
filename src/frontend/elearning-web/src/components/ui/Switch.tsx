import { useId, type ReactNode } from 'react';
import { cn } from '@/lib/utils';

export type SwitchProps = {
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
  /** Texto junto al interruptor; sin él, pasa `aria-label`. */
  label?: ReactNode;
  description?: ReactNode;
  disabled?: boolean;
  'aria-label'?: string;
  id?: string;
  className?: string;
};

/**
 * Interruptor encendido/apagado (país activo, pregunta requerida). Controlado con `checked` + `onCheckedChange`;
 * con react-hook-form, úsalo dentro de un Controller.
 * @category Forms
 */
export function Switch({
  checked,
  onCheckedChange,
  label,
  description,
  disabled,
  'aria-label': ariaLabel,
  id,
  className,
}: SwitchProps) {
  const generatedId = useId();
  const switchId = id ?? generatedId;

  const control = (
    <button
      id={switchId}
      type="button"
      role="switch"
      aria-checked={checked}
      aria-label={label ? undefined : ariaLabel}
      disabled={disabled}
      onClick={() => onCheckedChange(!checked)}
      className={cn(
        'relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50',
        checked ? 'bg-primary' : 'bg-secondary-hover'
      )}
    >
      <span
        className={cn(
          'block size-4 rounded-full bg-foreground shadow-sm transition-transform',
          checked ? 'translate-x-4.5' : 'translate-x-0.5'
        )}
      />
    </button>
  );

  if (!label && !description) return <span className={className}>{control}</span>;

  return (
    <div className={cn('flex items-start justify-between gap-4', className)}>
      <label htmlFor={switchId} className="flex flex-col gap-0.5">
        {label && <span className="text-sm text-foreground-soft">{label}</span>}
        {description && <span className="text-xs text-subtle-foreground">{description}</span>}
      </label>
      {control}
    </div>
  );
}
