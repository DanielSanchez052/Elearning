import { useId, type ReactNode } from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';
import { CloseButton } from './CloseButton';
import { useDialogBehavior } from './useDialogBehavior';

const panelVariants = cva(
  'flex max-h-[calc(100vh-2rem)] w-full flex-col rounded-2xl border border-input bg-card shadow-2xl',
  {
    variants: {
      size: {
        sm: 'max-w-md',
        md: 'max-w-xl',
        lg: 'max-w-2xl',
        xl: 'max-w-4xl',
      },
    },
    defaultVariants: {
      size: 'md',
    },
  }
);

export type ModalProps = VariantProps<typeof panelVariants> & {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: ReactNode;
  /** Acciones al pie (normalmente Cancelar + acción principal), alineadas a la derecha. */
  footer?: ReactNode;
  children: ReactNode;
  className?: string;
};

/**
 * Diálogo centrado sobre un overlay oscuro. Controlado: `open` + `onClose` (se cierra con Escape o clic fuera).
 * `title` y `description` en la cabecera, `footer` para las acciones (Cancelar + acción principal). `size`: sm | md | lg | xl.
 * @category Overlays
 */
export function Modal({ open, onClose, title, description, footer, children, size, className }: ModalProps) {
  const titleId = useId();
  useDialogBehavior(open, onClose);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-overlay p-4 backdrop-blur-sm"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={cn(panelVariants({ size }), className)}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-4 border-b border-border px-6 py-4">
          <div>
            <h2 id={titleId} className="text-lg font-semibold text-foreground">{title}</h2>
            {description && <p className="mt-1 text-sm text-muted-foreground">{description}</p>}
          </div>
          <CloseButton onClick={onClose} />
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-5">{children}</div>

        {footer && (
          <div className="flex items-center justify-end gap-2 border-t border-border px-6 py-4">{footer}</div>
        )}
      </div>
    </div>
  );
}
