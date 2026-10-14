import { useId, type ComponentProps, type ReactNode } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const radioCardVariants = cva(
  'flex items-start gap-3 rounded-xl border px-4 py-3 transition-colors has-focus-visible:ring-2 has-focus-visible:ring-ring',
  {
    variants: {
      tone: {
        // Selección normal: borde y fondo de marca al marcarse
        default: 'border-border bg-secondary hover:bg-secondary-hover has-checked:border-brand/60 has-checked:bg-brand/10',
        // Revisión de resultados: respuesta correcta / incorrecta
        success: 'border-success/30 bg-success/10',
        danger: 'border-destructive/30 bg-destructive/10',
      },
    },
    defaultVariants: {
      tone: 'default',
    },
  }
);

export type RadioCardProps = Omit<ComponentProps<'input'>, 'type'> &
  VariantProps<typeof radioCardVariants> & {
    label: ReactNode;
    description?: ReactNode;
    /** Contenido a la derecha (p. ej. un Badge «Correcta»). */
    trailing?: ReactNode;
  };

/**
 * Opción de radio en forma de tarjeta (opciones de una evaluación, alcance global/por país, opción correcta).
 * Agrupa varias con el mismo `name` dentro de un `role="radiogroup"`. `tone`: default | success | danger
 * (success/danger para mostrar el resultado tras enviar).
 * @category Forms
 */
export function RadioCard({ label, description, trailing, tone, className, id, disabled, ...props }: RadioCardProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;

  return (
    <label
      htmlFor={inputId}
      className={cn(
        radioCardVariants({ tone }),
        disabled ? 'cursor-not-allowed opacity-60' : 'cursor-pointer',
        className
      )}
    >
      <input
        id={inputId}
        type="radio"
        disabled={disabled}
        className="mt-0.5 size-4 shrink-0 cursor-[inherit] appearance-none rounded-full border border-border-strong bg-secondary transition-colors checked:border-4 checked:border-primary checked:bg-primary-foreground focus-visible:outline-none"
        {...props}
      />
      <span className="flex min-w-0 flex-1 flex-col gap-0.5">
        <span className="text-sm text-foreground">{label}</span>
        {description && <span className="text-xs text-muted-foreground">{description}</span>}
      </span>
      {trailing}
    </label>
  );
}
