import { cva, type VariantProps } from 'class-variance-authority';
import { cn, getInitials } from '@/lib/utils';

const avatarVariants = cva('flex shrink-0 items-center justify-center rounded-full font-semibold select-none', {
  variants: {
    size: {
      xs: 'size-6 text-xs',
      sm: 'size-8 text-xs',
      md: 'size-10 text-sm',
      lg: 'size-16 text-xl',
    },
    tone: {
      brand: 'bg-brand/20 text-brand-foreground',
      neutral: 'bg-secondary text-foreground-soft',
    },
  },
  defaultVariants: {
    size: 'sm',
    tone: 'brand',
  },
});

export type AvatarProps = VariantProps<typeof avatarVariants> & {
  /** Nombre completo: se muestran sus iniciales y se usa como etiqueta accesible. */
  name: string;
  className?: string;
};

/**
 * Avatar circular con las iniciales del usuario. `size`: xs | sm | md | lg.
 * `tone`: brand (usuario actual) | neutral (otras personas, p. ej. el instructor).
 * @category Data display
 */
export function Avatar({ name, size, tone, className }: AvatarProps) {
  return (
    <span role="img" aria-label={name} title={name} className={cn(avatarVariants({ size, tone }), className)}>
      {getInitials(name)}
    </span>
  );
}
