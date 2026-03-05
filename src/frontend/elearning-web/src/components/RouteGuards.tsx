import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/features/auth/authStore';
import type { UserRole } from '../types/user.types';

// ── Ruta protegida — requiere autenticación ───────────────────────────────────
export function ProtectedRoute() {
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  return isLoggedIn ? <Outlet /> : <Navigate to="/login" replace />;
}

// ── Ruta por rol — redirige si el rol no tiene acceso ────────────────────────
interface RoleRouteProps {
  allowedRoles: UserRole[];
}

export function RoleRoute({ allowedRoles }: RoleRouteProps) {
  const user = useAuthStore((s) => s.user);

  if (!user) return <Navigate to="/login" replace />;

  if (!allowedRoles.includes(user.role)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
}

// ── Ruta pública — redirige al dashboard si ya está autenticado ───────────────
export function PublicRoute() {
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  return isLoggedIn ? <Navigate to="/dashboard" replace /> : <Outlet />;
}
