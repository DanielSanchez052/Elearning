/**
 * @deprecated Importar directamente desde '@/hooks/admin'.
 * Los hooks han sido movidos a módulos específicos:
 * - Países: '@/hooks/admin/countries.ts'
 * - Usuarios: '@/hooks/admin/users.ts'
 * - Cursos: '@/hooks/admin/courses.ts'
 * 
 * Este archivo se mantiene para compatibilidad hacia atrás.
 */

// Re-exportar todo desde el directorio admin para mantener compatibilidad
export {
  useAdminCountries,
  useCreateCountry,
  useToggleCountryStatus,
  useAdminUsers,
  useChangeUserRole,
  useChangeUserCountry,
  useAdminCourses,
  useToggleCourseStatus,
  useDeleteCourse,
  adminKeys,
  useCreateCourse,
  useUpdateCourse,
  useAssignCountries,
  useCreateLesson,
  useUpdateLesson,
  useDeleteLesson,
  useReorderLessons
} from './admin';