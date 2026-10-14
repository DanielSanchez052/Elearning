import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import ResetPasswordForm from '@/features/auth/ResetPasswordForm';
import { AuthLayout, AuthMessage } from '@/components/layout/AuthLayout';
import { Button } from '@/components/ui';
import { XCircleIcon } from '@/components/ui/icons';

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const navigate = useNavigate();

  if (!token) {
    return (
      <AuthLayout>
        <AuthMessage
          icon={XCircleIcon}
          tone="danger"
          title="Enlace inválido"
          text="El enlace para restablecer tu contraseña no es válido o ya venció. Solicita uno nuevo."
          action={
            <Button fullWidth onClick={() => navigate('/forgot-password')}>
              Solicitar nuevo enlace
            </Button>
          }
          secondary={
            <Link to="/login" className="text-sm text-muted-foreground hover:text-foreground">
              Volver a iniciar sesión
            </Link>
          }
        />
      </AuthLayout>
    );
  }

  return (
    <AuthLayout
      title="Nueva contraseña"
      subtitle="Elige una contraseña que no hayas usado antes."
      footer={
        <>
          ¿Necesitas ayuda?{' '}
          <Link to="/login" className="font-medium text-brand-foreground hover:text-foreground">
            Volver a iniciar sesión
          </Link>
        </>
      }
    >
      <ResetPasswordForm token={token} />
    </AuthLayout>
  );
}
