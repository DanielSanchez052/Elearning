import { type ComponentType, type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Button } from './Button';
import { AlertTriangleIcon, BookOpenIcon } from './icons';

export type StateBlockProps = {
  /** vacio = todavía no hay datos; error = la carga falló (muestra «Reintentar» por defecto). */
  tipo?: 'vacio' | 'error';
  /** Ícono del bundle (p. ej. GraduationCapIcon). Por defecto BookOpenIcon (vacío) o AlertTriangleIcon (error). */
  icon?: ComponentType<{ className?: string }>;
  title?: string;
  text?: ReactNode;
  /** Texto del botón. En error es «Reintentar» por defecto; en vacío solo aparece si se indica. */
  action?: string;
  onAction?: () => void;
  actionVariant?: 'primary' | 'secondary' | 'outline';
  /** Menos aire vertical, para usarlo dentro de un panel. */
  compact?: boolean;
  /** Borde punteado alrededor (por defecto true). */
  bordered?: boolean;
  className?: string;
};

/**
 * Estado vacío o de error de una vista: ícono en chip + título + texto + una sola acción.
 * `tipo="error"` usa el tono destructivo y la acción «Reintentar» (pásale el refetch en `onAction`).
 * @category Feedback
 */
export function StateBlock({
  tipo = 'vacio',
  icon,
  title,
  text,
  action,
  onAction,
  actionVariant,
  compact = false,
  bordered = true,
  className,
}: StateBlockProps) {
  const isError = tipo === 'error';
  const Icon = icon ?? (isError ? AlertTriangleIcon : BookOpenIcon);
  const label = action ?? (isError ? 'Reintentar' : undefined);
  const body = text ?? (isError ? 'Revisa tu conexión e inténtalo de nuevo.' : undefined);

  return (
    <div
      role={isError ? 'alert' : undefined}
      className={cn(
        'flex flex-col items-center gap-4 text-center',
        bordered && 'rounded-2xl border border-dashed border-border-strong',
        compact ? 'px-6 py-8' : 'px-6 py-16',
        className
      )}
    >
      <span
        className={cn(
          'flex size-12 items-center justify-center rounded-full',
          isError ? 'bg-destructive/10 text-destructive-foreground' : 'bg-brand/10 text-brand-foreground'
        )}
      >
        <Icon className="size-6" />
      </span>
      <div className="max-w-md space-y-1">
        <p className="text-base font-semibold text-foreground">
          {title ?? (isError ? 'No pudimos cargar esta sección' : 'Aún no hay nada aquí')}
        </p>
        {body && <p className="text-sm text-muted-foreground">{body}</p>}
      </div>
      {label && (
        <Button variant={actionVariant ?? (isError ? 'secondary' : 'primary')} onClick={onAction}>
          {label}
        </Button>
      )}
    </div>
  );
}
