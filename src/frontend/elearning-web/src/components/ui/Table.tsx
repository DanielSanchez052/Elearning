import { type ComponentProps } from 'react';
import { cn } from '@/lib/utils';

/**
 * Tabla de datos con scroll horizontal en pantallas estrechas. Va dentro de `Card padding="none"`.
 * Compón con TableHead > TableRow > TableHeader y TableBody > TableRow > TableCell.
 * @category Data display
 */
export function Table({ className, ...props }: ComponentProps<'table'>) {
  return (
    // relative: contiene los elementos absolutos (p. ej. textos sr-only de cabecera) para que no ensanchen la página.
    <div className="relative w-full overflow-x-auto">
      <table className={cn('w-full text-left text-sm', className)} {...props} />
    </div>
  );
}

/** Cabecera de Table: fondo bg-secondary y texto en mayúsculas. */
export function TableHead({ className, ...props }: ComponentProps<'thead'>) {
  return (
    <thead
      className={cn('border-b border-border bg-secondary text-xs uppercase tracking-wider text-muted-foreground', className)}
      {...props}
    />
  );
}

/** Cuerpo de Table: filas separadas con divide-y. */
export function TableBody({ className, ...props }: ComponentProps<'tbody'>) {
  return <tbody className={cn('divide-y divide-border', className)} {...props} />;
}

/** Fila de Table. */
export function TableRow({ className, ...props }: ComponentProps<'tr'>) {
  return <tr className={cn('transition-colors', className)} {...props} />;
}

/** Celda de cabecera (th). */
export function TableHeader({ className, ...props }: ComponentProps<'th'>) {
  return <th scope="col" className={cn('whitespace-nowrap px-6 py-3 font-medium', className)} {...props} />;
}

/** Celda de datos (td). */
export function TableCell({ className, ...props }: ComponentProps<'td'>) {
  return <td className={cn('px-6 py-4 text-foreground-soft', className)} {...props} />;
}
