import { useState, type ComponentProps } from 'react';
import { Input } from '@/components/ui';
import { EyeIcon, EyeOffIcon } from '@/components/ui/icons';

/**
 * Input de contraseña con botón para mostrar u ocultar el texto. Úsalo dentro de Field con la forma
 * de función: `{(control) => <PasswordInput {...control} {...register('password')} />}`.
 */
export function PasswordInput(props: Omit<ComponentProps<'input'>, 'type'>) {
  const [visible, setVisible] = useState(false);

  return (
    <div className="relative">
      <Input {...props} type={visible ? 'text' : 'password'} className="pr-11" />
      <button
        type="button"
        onClick={() => setVisible((v) => !v)}
        aria-label={visible ? 'Ocultar contraseña' : 'Mostrar contraseña'}
        aria-pressed={visible}
        className="absolute top-1/2 right-2 flex size-7 -translate-y-1/2 items-center justify-center rounded-lg text-subtle-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
      >
        {visible ? <EyeOffIcon className="size-4" /> : <EyeIcon className="size-4" />}
      </button>
    </div>
  );
}
