import { Link } from 'react-router-dom';
import RegisterForm from '@/features/auth/RegisterForm';
import { AuthLayout } from '@/components/layout/AuthLayout';

export default function RegisterPage() {
  return (
    <AuthLayout
      title="Crea tu cuenta"
      subtitle="Accede a cursos de tu región"
      footer={
        <>
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="font-medium text-brand-foreground hover:text-foreground">
            Ingresa aquí
          </Link>
        </>
      }
    >
      <RegisterForm />
    </AuthLayout>
  );
}
