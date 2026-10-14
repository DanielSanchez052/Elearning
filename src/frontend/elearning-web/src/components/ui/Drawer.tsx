import { useId, useRef } from 'react';
import { cn } from '@/lib/utils';
import { CloseButton } from './CloseButton';
import { useDialogBehavior } from './useDialogBehavior';

interface DrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  width?: string; // Tailwind width class, default 'w-96'
}

/**
 * Panel lateral que entra por la derecha sobre un overlay. Controlado: `open` + `onClose` (Escape o clic fuera).
 * `width` es una clase de ancho de Tailwind (por defecto "w-96"). Para detalle o edición sin salir de una lista.
 * @category Overlays
 */
export function Drawer({ open, onClose, title, children, width = 'w-96' }: DrawerProps) {
  const titleId = useId();
  const panelRef = useRef<HTMLDivElement>(null);
  useDialogBehavior(open, onClose, panelRef);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex justify-end">
      {/* Overlay */}
      <div className="absolute inset-0 bg-overlay backdrop-blur-sm" onClick={onClose} />

      {/* Panel */}
      <div
        ref={panelRef}
        tabIndex={-1}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={cn('relative flex h-full max-w-full flex-col border-l border-border bg-sidebar shadow-2xl focus:outline-none', width)}
      >
        {/* Header */}
        <div className="flex items-center justify-between border-b border-border px-6 py-4">
          <h2 id={titleId} className="text-sm font-semibold text-foreground">{title}</h2>
          <CloseButton onClick={onClose} />
        </div>

        {/* Contenido */}
        <div className="flex-1 overflow-y-auto p-6">
          {children}
        </div>
      </div>
    </div>
  );
}

export default Drawer;
