import { type ComponentProps } from 'react';
import { type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';
import { buttonVariants } from './button-variants';

export type ButtonProps = ComponentProps<'button'> & VariantProps<typeof buttonVariants>;

/**
 * Botón de acción. `variant` elige la jerarquía (primary para la acción principal de la vista,
 * secondary/outline/ghost para las demás, danger/destructive para eliminar); `size="icon"` para botones de solo ícono
 * (requieren `aria-label`). Por defecto es `type="button"`: en formularios usa `type="submit"`.
 * @category Actions
 */
export function Button({ className, variant, size, fullWidth, type = 'button', ...props }: ButtonProps) {
  return (
    <button
      type={type}
      className={cn(buttonVariants({ variant, size, fullWidth }), className)}
      {...props}
    />
  );
}
