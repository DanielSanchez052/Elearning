import ForgotPasswordForm from '@/features/auth/ForgotPasswordForm';
import { AuthLayout } from '@/components/layout/AuthLayout';

export default function ForgotPasswordPage() {
  return (
    <AuthLayout
      title="Recupera tu contraseña"
      subtitle="Te enviaremos un enlace para restablecerla"
    >
      <ForgotPasswordForm />
    </AuthLayout>
  );
}
