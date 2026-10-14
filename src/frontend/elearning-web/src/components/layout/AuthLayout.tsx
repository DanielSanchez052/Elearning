import type { ComponentType, ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { AppRoot, Card, Spinner } from '@/components/ui';
import { GraduationCapIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';

interface AuthLayoutProps {
  /** Título del formulario dentro de la tarjeta. Las pantallas de mensaje (AuthMessage) lo omiten. */
  title?: string;
  subtitle?: string;
  /** Aviso bajo el título (p. ej. «Cuenta creada»), normalmente un Alert. */
  notice?: ReactNode;
  /** Pie dentro de la tarjeta (enlace a registro / inicio de sesión). */
  footer?: ReactNode;
  children: ReactNode;
}

/** Shell de autenticación del rediseño: marca arriba y una tarjeta centrada con título, aviso, contenido y pie. */
export function AuthLayout({ title, subtitle, notice, footer, children }: AuthLayoutProps) {
  return (
    <AppRoot className="min-h-screen">
      <main className="flex min-h-screen flex-col items-center justify-center gap-8 px-4 py-12 sm:px-6">
        <Link
          to="/login"
          className="flex items-center gap-2.5 rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          <span className="flex size-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <GraduationCapIcon className="size-5" aria-hidden="true" />
          </span>
          <span className="text-lg font-semibold text-foreground">ELearning</span>
        </Link>

        <Card elevated padding="lg" className="w-full max-w-md">
          {title && (
            <div className="mb-6">
              <h1 className="text-2xl font-semibold text-foreground">{title}</h1>
              {subtitle && <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>}
            </div>
          )}
          {notice && <div className="mb-5">{notice}</div>}
          {children}
          {footer && <p className="mt-6 text-center text-sm text-muted-foreground">{footer}</p>}
        </Card>
      </main>
    </AppRoot>
  );
}

const chipTones = {
  brand: 'bg-brand/10 text-brand-foreground',
  success: 'bg-success/10 text-success-foreground',
  danger: 'bg-destructive/10 text-destructive-foreground',
};

interface AuthMessageProps {
  /** Ícono del chip; sin él se muestra un Spinner (estado «verificando»). */
  icon?: ComponentType<{ className?: string }>;
  tone?: keyof typeof chipTones;
  title: string;
  text: ReactNode;
  /** Acción principal a ancho completo (Button). */
  action?: ReactNode;
  /** Enlace secundario bajo la acción. */
  secondary?: ReactNode;
}

/** Pantalla de mensaje dentro de AuthLayout: enlace enviado, verificando, verificado o enlace inválido. */
export function AuthMessage({ icon: Icon, tone = 'brand', title, text, action, secondary }: AuthMessageProps) {
  return (
    <div className="flex flex-col items-center gap-4 py-4 text-center">
      {Icon ? (
        <span className={cn('flex size-14 items-center justify-center rounded-full', chipTones[tone])}>
          <Icon className="size-6" aria-hidden="true" />
        </span>
      ) : (
        <Spinner size="lg" label={title} />
      )}
      <div className="space-y-1">
        <h1 className="text-xl font-semibold text-foreground">{title}</h1>
        <p className="text-sm text-muted-foreground">{text}</p>
      </div>
      {action && <div className="mt-2 w-full">{action}</div>}
      {secondary}
    </div>
  );
}
