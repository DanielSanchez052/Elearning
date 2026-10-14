import { useId, type ComponentProps, type ReactElement, type ReactNode, cloneElement } from 'react';
import { cn } from '@/lib/utils';

// Base compartida por Input, Select y Textarea: el patrón de campo que hoy se repite en las páginas.
const fieldBase =
  'w-full rounded-xl border border-input bg-secondary px-4 py-2.5 text-sm text-foreground placeholder:text-placeholder transition-all focus:border-ring focus:bg-secondary-hover focus:outline-none disabled:cursor-not-allowed disabled:opacity-50 aria-invalid:border-destructive/60';

/**
 * Campo de texto con el estilo del sistema. Normalmente va dentro de Field, que aporta label, ayuda y error.
 * @category Forms
 */
export function Input({ className, ...props }: ComponentProps<'input'>) {
  return <input className={cn(fieldBase, className)} {...props} />;
}

/**
 * Área de texto multilínea (sin redimensionar). Normalmente va dentro de Field.
 * @category Forms
 */
export function Textarea({ className, ...props }: ComponentProps<'textarea'>) {
  return <textarea className={cn(fieldBase, 'resize-none', className)} {...props} />;
}

/**
 * Lista desplegable nativa con el estilo del sistema; las opciones son <option> hijos. Normalmente va dentro de Field.
 * @category Forms
 */
export function Select({ className, ...props }: ComponentProps<'select'>) {
  return <select className={cn(fieldBase, 'cursor-pointer', className)} {...props} />;
}

/**
 * Etiqueta de formulario. Field ya la incluye; úsala sola solo para controles fuera de Field.
 * @category Forms
 */
export function Label({ className, ...props }: ComponentProps<'label'>) {
  return <label className={cn('mb-1.5 block text-sm font-medium text-foreground-soft', className)} {...props} />;
}

/** Atributos que Field asigna al control para enlazarlo con su Label y su mensaje. */
export type FieldControlProps = {
  id: string;
  'aria-invalid': true | undefined;
  'aria-describedby': string | undefined;
};

type FieldProps = {
  label: string;
  /**
   * Un único Input, Select o Textarea (Field le asigna id y atributos de accesibilidad), o una función que
   * recibe esos atributos para controles que no son un elemento directo (SearchInput, componentes envueltos).
   */
  children: ReactElement<ComponentProps<'input'>> | ((control: FieldControlProps) => ReactNode);
  hint?: ReactNode;
  error?: string;
  /** id del control; necesario solo con la forma de función si quieres fijarlo. */
  id?: string;
  className?: string;
};

/**
 * Campo de formulario completo: Label + un único Input/Select/Textarea hijo + texto de ayuda (`hint`) o de error (`error`).
 * Asigna id, aria-invalid y aria-describedby al control automáticamente; con `children` como función,
 * los recibe como argumento para aplicarlos tú.
 * @category Forms
 */
export function Field({ label, children, hint, error, id: idProp, className }: FieldProps) {
  const generatedId = useId();
  const id = idProp ?? (typeof children === 'function' ? undefined : children.props.id) ?? generatedId;
  const messageId = `${id}-message`;
  const message = error ?? hint;
  const control: FieldControlProps = {
    id,
    'aria-invalid': error ? true : undefined,
    'aria-describedby': message ? messageId : undefined,
  };

  return (
    <div className={className}>
      <Label htmlFor={id}>{label}</Label>
      {typeof children === 'function' ? children(control) : cloneElement(children, control)}
      {message && (
        <p
          id={messageId}
          className={cn('mt-1.5 text-xs', error ? 'text-destructive-foreground' : 'text-subtle-foreground')}
        >
          {message}
        </p>
      )}
    </div>
  );
}
