import type { CurrentUser } from '@/types/user.types';

// Espejo de las reglas del backend (ELearning.Application/Common/Authorization/CourseAccess.cs).
// Solo sirven para no ofrecer acciones que el backend va a rechazar: la autorización real es la del backend.
//
//   superadmin → cualquier curso; único que crea o marca cursos globales.
//   admin      → cursos no globales asignados a su país.
//   instructor → cursos no globales que él creó.
//   Fuera del superadmin, solo se puede añadir o quitar el país propio de un curso.

type User = Pick<CurrentUser, 'id' | 'role' | 'countryId'> | null | undefined;

interface CourseScope {
  isGlobal: boolean;
  instructorId: string;
  countries: { id: number }[];
}

export const isSuperAdmin = (user: User) => user?.role === 'superadmin';

export function canManageCourse(user: User, course: CourseScope): boolean {
  switch (user?.role) {
    case 'superadmin':
      return true;
    case 'admin':
      return !course.isGlobal && course.countries.some((c) => c.id === user.countryId);
    case 'instructor':
      return !course.isGlobal && course.instructorId === user.id;
    default:
      return false;
  }
}

/**
 * Disponible para inscribirse (CourseAccess.IsAvailableIn): publicado y global o asignado al país
 * del usuario. El staff puede abrir otros cursos en vista previa, pero no inscribirse en ellos.
 */
export function isCourseAvailableIn(user: User, course: Pick<CourseScope, 'isGlobal' | 'countries'> & { isActive: boolean }): boolean {
  return course.isActive && (course.isGlobal || course.countries.some((c) => c.id === user?.countryId));
}

/** Por qué el usuario no puede gestionar el curso, para mostrarlo en la página. */
export function courseAccessDeniedReason(user: User, course: Pick<CourseScope, 'isGlobal'>): string {
  if (course.isGlobal) return 'Los cursos globales solo los gestiona el superadmin.';
  return user?.role === 'admin'
    ? 'Solo puedes gestionar los cursos asignados a tu país.'
    : 'Solo puedes gestionar los cursos que creaste.';
}

/**
 * Para filas del listado de administración (`CourseSummaryDto`), que no traen país ni creador:
 * el backend ya limita el listado a los cursos del país del admin (más los globales) y a los
 * cursos propios del instructor, así que basta con descartar los globales.
 */
export function canManageListedCourse(user: User, course: Pick<CourseScope, 'isGlobal'>): boolean {
  if (isSuperAdmin(user)) return true;
  return (user?.role === 'admin' || user?.role === 'instructor') && !course.isGlobal;
}

export const canToggleCourseCountry = (user: User, countryId: number) =>
  isSuperAdmin(user) || user?.countryId === countryId;
