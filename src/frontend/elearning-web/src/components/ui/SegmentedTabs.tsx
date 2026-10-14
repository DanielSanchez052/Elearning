import { cn } from '@/lib/utils';

export type SegmentedTabsOption<T extends string> = {
  value: T;
  label: string;
  /** Contador opcional junto a la etiqueta. */
  count?: number;
};

export type SegmentedTabsProps<T extends string> = {
  options: SegmentedTabsOption<T>[];
  value: T;
  onChange: (value: T) => void;
  'aria-label': string;
  className?: string;
};

/**
 * Control segmentado sobre bg-secondary para filtrar una lista (Mis cursos: Todos · Activos · Completados;
 * Cursos admin: Publicados · Borradores). Controlado con `value` + `onChange`.
 * @category Navigation
 */
export function SegmentedTabs<T extends string>({
  options,
  value,
  onChange,
  'aria-label': ariaLabel,
  className,
}: SegmentedTabsProps<T>) {
  return (
    <div
      role="tablist"
      aria-label={ariaLabel}
      className={cn('flex w-fit max-w-full gap-1 overflow-x-auto rounded-lg bg-secondary p-1', className)}
    >
      {options.map((o) => {
        const selected = o.value === value;
        return (
          <button
            key={o.value}
            type="button"
            role="tab"
            aria-selected={selected}
            onClick={() => onChange(o.value)}
            className={cn(
              'whitespace-nowrap rounded-md px-3 py-1.5 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
              selected ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'
            )}
          >
            {o.label}
            {o.count !== undefined && <span className="ml-1.5 tabular-nums text-subtle-foreground">{o.count}</span>}
          </button>
        );
      })}
    </div>
  );
}
