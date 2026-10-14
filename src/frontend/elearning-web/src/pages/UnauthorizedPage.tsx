import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { AppRoot, StateBlock } from '@/components/ui';
import { LockIcon } from '@/components/ui/icons';

export default function UnauthorizedPage() {
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  const navigate = useNavigate();

  return (
    <AppRoot className="min-h-screen">
      <main className="flex min-h-screen items-center justify-center px-4 py-10">
        <StateBlock
          titleAs="h1"
          className="w-full max-w-lg"
          icon={LockIcon}
          title="No tienes permisos para ver esta sección"
          text="Si crees que es un error, pide a un administrador que revise tu rol."
          action={isLoggedIn ? 'Ir a mi dashboard' : 'Iniciar sesión'}
          onAction={() => navigate(isLoggedIn ? '/dashboard' : '/login')}
        />
      </main>
    </AppRoot>
  );
}
