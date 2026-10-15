import type { CurrentUser } from '@/types/user.types';

// Espejo de las reglas del backend (ELearning.Application/Common/Authorization/CourseAccess.cs).
// Solo sirven para no ofrecer acciones que el backend va a rechazar: la autorización real es la del backend.
//
// Gestionar un curso (editarlo, publicarlo, eliminarlo y administrar sus lecciones y quizzes):
//   superadmin → cualquier curso, incluidos los globales.
//   admin      → los cursos no globales que tienen su país entre sus países.
//   instructor → los cursos de los que es el instructor asignado (instructorId), sean globales o no.
//                Haber creado el curso no da ningún permiso.
// Además:
//   - Solo admin y superadmin asignan o quitan el instructor de un curso que gestionan.
//   - Fuera del superadmin, en los países de un curso solo se puede añadir o quitar el propio.
//   - Solo el superadmin crea cursos globales o cambia si un curso es global.
//   - Quien no gestiona un curso solo lo ve, o se inscribe, si está publicado y disponible en su país.

type User = Pick<CurrentUser, 'id' | 'role' | 'countryId'> | null | undefined;

interface CourseScope {
  isGlobal: boolean;
  instructorId: string | null;
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
      return course.instructorId === user.id;
    default:
      return false;
  }
}

/**
 * Disponible para inscribirse (CourseAccess.IsAvailableIn): publicado y, además, global o con el país
 * del usuario entre sus países. El staff puede abrir otros cursos en vista previa, pero no inscribirse.
 */
export function isCourseAvailableIn(user: User, course: Pick<CourseScope, 'isGlobal' | 'countries'> & { isActive: boolean }): boolean {
  return course.isActive && (course.isGlobal || course.countries.some((c) => c.id === user?.countryId));
}

/** Por qué el usuario no puede gestionar el curso, para mostrarlo en la página. */
export function courseAccessDeniedReason(user: User, course: Pick<CourseScope, 'isGlobal'>): string {
  if (user?.role === 'instructor') return 'Solo puedes gestionar los cursos que tienes asignados.';
  if (course.isGlobal) return 'Los cursos globales solo los gestiona el superadmin.';
  return 'Solo puedes gestionar los cursos de tu país.';
}

/**
 * Para filas del listado de administración (`CourseSummaryDto`), que no traen países: el backend
 * ya limita el listado a los cursos del país del admin (más los globales) y a los que tiene
 * asignados el instructor, así que al admin solo le quedan por descartar los globales.
 */
export function canManageListedCourse(user: User, course: Pick<CourseScope, 'isGlobal'>): boolean {
  if (isSuperAdmin(user) || user?.role === 'instructor') return true;
  return user?.role === 'admin' && !course.isGlobal;
}

export const canAssignCourseInstructor = (user: User) => user?.role === 'admin' || isSuperAdmin(user);

export const canToggleCourseCountry = (user: User, countryId: number) =>
  isSuperAdmin(user) || user?.countryId === countryId;
