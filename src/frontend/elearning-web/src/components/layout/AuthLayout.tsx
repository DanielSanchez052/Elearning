import type { ReactNode } from 'react';
import { AppRoot, Card } from '@/components/ui';
import { GraduationCapIcon } from '@/components/ui/icons';

interface AuthLayoutProps {
  title: string;
  subtitle?: string;
  /** Avisos sobre la tarjeta (p. ej. «Cuenta creada»), normalmente un Alert. */
  notice?: ReactNode;
  /** Texto bajo la tarjeta (enlace a registro / inicio de sesión). */
  footer?: ReactNode;
  children: ReactNode;
}

/** Shell de las pantallas de autenticación: marca, título, tarjeta centrada y pie opcional. */
export function AuthLayout({ title, subtitle, notice, footer, children }: AuthLayoutProps) {
  return (
    <AppRoot className="flex min-h-screen items-center justify-center px-4 py-10">
      <div className="w-full max-w-md">
        <div className="mb-8 flex flex-col items-center text-center">
          <span className="mb-4 flex size-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
            <GraduationCapIcon className="size-6" aria-hidden="true" />
          </span>
          <h1 className="text-2xl font-semibold text-foreground">{title}</h1>
          {subtitle && <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>}
        </div>

        {notice && <div className="mb-4">{notice}</div>}

        <Card padding="lg" elevated>
          {children}
        </Card>

        {footer && <p className="mt-6 text-center text-sm text-muted-foreground">{footer}</p>}
      </div>
    </AppRoot>
  );
}
