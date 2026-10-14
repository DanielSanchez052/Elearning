import type { UserRole } from '@/types/user.types';

/** Destino del panel de administración según el rol: el instructor solo gestiona cursos; el estudiante no tiene panel. */
export function adminHomeFor(role: UserRole | undefined): string | null {
  if (role === 'admin' || role === 'superadmin') return '/admin';
  if (role === 'instructor') return '/admin/courses';
  return null;
}
