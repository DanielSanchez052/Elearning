import { useNavigate, useSearchParams } from 'react-router-dom';
import { AuthLayout, AuthMessage } from '@/components/layout/AuthLayout';
import { useVerifyEmail } from '@/hooks/useAuth';
import { Button } from '@/components/ui';
import { CheckCircleIcon, XCircleIcon } from '@/components/ui/icons';

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const { isSuccess, isError } = useVerifyEmail(token);
  const navigate = useNavigate();

  const toLogin = (
    <Button fullWidth onClick={() => navigate('/login')}>
      Iniciar sesión
    </Button>
  );

  return (
    <AuthLayout>
      {isSuccess ? (
        <AuthMessage
          icon={CheckCircleIcon}
          tone="success"
          title="Correo verificado"
          text="Tu cuenta está lista. Ya puedes iniciar sesión."
          action={toLogin}
        />
      ) : isError || !token ? (
        // No hay endpoint para reenviar la verificación: la única salida es volver al inicio de sesión.
        <AuthMessage
          icon={XCircleIcon}
          tone="danger"
          title="Enlace inválido"
          text="El enlace de verificación no es válido o ya fue utilizado."
          action={toLogin}
        />
      ) : (
        <AuthMessage title="Verificando tu correo…" text="Esto tomará solo unos segundos." />
      )}
    </AuthLayout>
  );
}
