import { X } from 'lucide-react';
import { Button, type ButtonProps } from './Button';

/**
 * Botón "×" de cabecera de Modal y Drawer (Button secondary de tamaño icon, aria-label "Cerrar").
 * @category Actions
 */
export function CloseButton({ 'aria-label': ariaLabel = 'Cerrar', ...props }: Omit<ButtonProps, 'variant' | 'size'>) {
  return (
    <Button variant="secondary" size="icon" aria-label={ariaLabel} {...props}>
      <X />
    </Button>
  );
}
