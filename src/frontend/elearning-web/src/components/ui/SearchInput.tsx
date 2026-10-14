import { type ComponentProps } from 'react';
import { cn } from '@/lib/utils';
import { Input } from './Input';
import { SearchIcon } from './icons';

/**
 * Campo de búsqueda con lupa (Catálogo, Usuarios, Cursos admin). Requiere `aria-label` o un Label externo.
 * `className` se aplica al contenedor: úsalo para el ancho.
 * @category Forms
 */
export function SearchInput({ className, ...props }: Omit<ComponentProps<'input'>, 'type'>) {
  return (
    <div className={cn('relative', className)}>
      <SearchIcon className="pointer-events-none absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-subtle-foreground" />
      <Input type="search" className="pl-10" {...props} />
    </div>
  );
}
