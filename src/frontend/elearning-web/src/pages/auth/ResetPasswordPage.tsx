import ResetPasswordForm from '@/features/auth/ResetPasswordForm';
import { AuthLayout } from '@/components/layout/AuthLayout';

export default function ResetPasswordPage() {
  return (
    <AuthLayout
      title="Nueva contraseña"
      subtitle="Elige una contraseña segura para tu cuenta"
    >
      <ResetPasswordForm />
    </AuthLayout>
  );
}
