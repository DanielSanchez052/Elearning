import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import ForgotPasswordForm from '@/features/auth/ForgotPasswordForm';
import { AuthLayout, AuthMessage } from '@/components/layout/AuthLayout';
import { getApiErrorMessage, useForgotPassword, type ForgotPasswordFormData } from '@/hooks/useAuth';
import { Button } from '@/components/ui';
import { MailIcon } from '@/components/ui/icons';

export default function ForgotPasswordPage() {
  const forgot = useForgotPassword();
  const navigate = useNavigate();
  // Correo al que se envió el enlace: fija la vista de mensaje aunque luego se reenvíe.
  const [sentTo, setSentTo] = useState<string | null>(null);
  const [resent, setResent] = useState(false);

  const send = (data: ForgotPasswordFormData) =>
    forgot.mutate(data, { onSuccess: () => setSentTo(data.email) });

  const resend = () => {
    if (!sentTo) return;
    setResent(false);
    forgot.mutate({ email: sentTo }, { onSuccess: () => setResent(true) });
  };

  if (sentTo) {
    return (
      <AuthLayout>
        <AuthMessage
          icon={MailIcon}
          title="Te enviamos un enlace"
          text={
            <>
              Si {sentTo} está registrado, recibirás en unos minutos un enlace para crear una nueva contraseña.
              Revisa también tu carpeta de spam.
              {resent && <span className="mt-2 block text-success-foreground">Enviamos el enlace de nuevo.</span>}
              {forgot.isError && <span className="mt-2 block text-destructive-foreground">{getApiErrorMessage(forgot.error)}</span>}
            </>
          }
          action={
            <Button variant="secondary" fullWidth onClick={() => navigate('/login')}>
              Volver a iniciar sesión
            </Button>
          }
          secondary={
            <button
              type="button"
              onClick={resend}
              disabled={forgot.isPending}
              className="text-sm text-muted-foreground hover:text-foreground disabled:opacity-50"
            >
              {forgot.isPending ? 'Reenviando…' : 'Reenviar enlace'}
            </button>
          }
        />
      </AuthLayout>
    );
  }

  return (
    <AuthLayout
      title="Recupera tu contraseña"
      subtitle="Te enviaremos un enlace para crear una nueva."
      footer={
        <>
          ¿La recordaste?{' '}
          <Link to="/login" className="font-medium text-brand-foreground hover:text-foreground">
            Inicia sesión
          </Link>
        </>
      }
    >
      <ForgotPasswordForm
        onSubmit={send}
        isPending={forgot.isPending}
        error={forgot.isError ? getApiErrorMessage(forgot.error) : undefined}
      />
    </AuthLayout>
  );
}
