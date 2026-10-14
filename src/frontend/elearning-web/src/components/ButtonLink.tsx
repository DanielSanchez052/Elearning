import { Link, type LinkProps } from 'react-router-dom';
import { type VariantProps } from 'class-variance-authority';
import { buttonVariants } from '@/components/ui/button-variants';
import { cn } from '@/lib/utils';

type ButtonLinkProps = LinkProps & VariantProps<typeof buttonVariants>;

/** Enlace de navegación con la apariencia de Button (misma jerarquía de `variant` y `size`). */
export function ButtonLink({ className, variant, size, fullWidth, ...props }: ButtonLinkProps) {
  return <Link className={cn(buttonVariants({ variant, size, fullWidth }), className)} {...props} />;
}
