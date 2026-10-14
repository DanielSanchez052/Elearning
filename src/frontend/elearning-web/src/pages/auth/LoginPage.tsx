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
      title="Inicia sesión"
      subtitle="Continúa con tus cursos donde los dejaste."
      notice={
        justRegistered ? (
          <Alert tone="success" title="Cuenta creada">
            Revisa tu correo para verificarla antes de ingresar.
          </Alert>
        ) : justReset ? (
          <Alert tone="success">Contraseña actualizada. Ya puedes iniciar sesión.</Alert>
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
