import { useId, type ComponentProps, type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { CheckIcon } from './icons';

export type CheckboxProps = Omit<ComponentProps<'input'>, 'type'> & {
  /** Texto junto a la casilla; sin él, pasa `aria-label`. */
  label?: ReactNode;
  /** Texto secundario bajo la etiqueta. */
  description?: ReactNode;
};

/**
 * Casilla de verificación nativa con el estilo del sistema (lección requerida, países asignados).
 * Funciona controlada (`checked` + `onChange`) o con `register` de react-hook-form.
 * @category Forms
 */
export function Checkbox({ label, description, className, id, disabled, ...props }: CheckboxProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;

  return (
    <label
      htmlFor={inputId}
      className={cn(
        'inline-flex items-start gap-3',
        disabled ? 'cursor-not-allowed opacity-50' : 'cursor-pointer',
        className
      )}
    >
      <span className="relative mt-0.5 flex size-4 shrink-0">
        <input
          id={inputId}
          type="checkbox"
          disabled={disabled}
          className="peer size-4 cursor-[inherit] appearance-none rounded border border-border-strong bg-secondary transition-colors checked:border-primary checked:bg-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          {...props}
        />
        <CheckIcon className="pointer-events-none absolute inset-0 m-auto size-3 text-primary-foreground opacity-0 peer-checked:opacity-100" />
      </span>
      {(label || description) && (
        <span className="flex flex-col gap-0.5">
          {label && <span className="text-sm text-foreground-soft">{label}</span>}
          {description && <span className="text-xs text-subtle-foreground">{description}</span>}
        </span>
      )}
    </label>
  );
}
