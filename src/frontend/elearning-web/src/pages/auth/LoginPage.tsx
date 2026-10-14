import { Link, useSearchParams } from 'react-router-dom';
import LoginForm from '@/features/auth/LoginForm';
import { AuthLayout } from '@/components/layout/AuthLayout';
import { Alert } from '@/components/ui';

export default function LoginPage() {
  const [searchParams] = useSearchParams();
  const justRegistered = searchParams.get('registered') === 'true';
  const justReset = searchParams.get('reset') === 'true';

  return (
    <AuthLayout
      title="Bienvenido de nuevo"
      subtitle="Ingresa a tu cuenta para continuar"
      notice={
        justRegistered ? (
          <Alert tone="success">Cuenta creada. Revisa tu email para verificarla antes de ingresar.</Alert>
        ) : justReset ? (
          <Alert tone="success">Contraseña actualizada. Ya puedes ingresar.</Alert>
        ) : undefined
      }
      footer={
        <>
          ¿No tienes cuenta?{' '}
          <Link to="/register" className="font-medium text-brand-foreground hover:text-foreground">
            Regístrate
          </Link>
        </>
      }
    >
      <LoginForm />
    </AuthLayout>
  );
}
