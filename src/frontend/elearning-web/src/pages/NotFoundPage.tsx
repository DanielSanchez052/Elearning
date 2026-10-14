import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { AppRoot, StateBlock } from '@/components/ui';
import { SearchIcon } from '@/components/ui/icons';

export default function NotFoundPage() {
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  const navigate = useNavigate();

  return (
    <AppRoot className="flex min-h-screen items-center justify-center px-4 py-10">
      <StateBlock
        className="w-full max-w-lg"
        icon={SearchIcon}
        title="No encontramos esta página"
        text="Puede que el enlace esté mal escrito o que la página ya no exista."
        action={isLoggedIn ? 'Ir a mi dashboard' : 'Iniciar sesión'}
        onAction={() => navigate(isLoggedIn ? '/dashboard' : '/login')}
      />
    </AppRoot>
  );
}
