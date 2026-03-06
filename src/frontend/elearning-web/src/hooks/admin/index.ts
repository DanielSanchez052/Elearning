// Exportar hooks por módulo
export {
  useAdminCountries,
  useCreateCountry,
  useToggleCountryStatus,
  countriesKeys,
} from './countries';

export {
  useAdminUsers,
  useChangeUserRole,
  useChangeUserCountry,
  usersKeys,
} from './users';

export {
  useAdminCourses,
  useToggleCourseStatus,
  useDeleteCourse,
  coursesKeys,
  useCreateCourse,
  useUpdateCourse,
  useAssignCountries,
  useCreateLesson,
  useUpdateLesson,
  useDeleteLesson,
} from './courses';

// Agregador de query keys para compatibilidad
import { countriesKeys } from './countries';
import { usersKeys } from './users';
import { coursesKeys } from './courses';

export const adminKeys = {
  ...countriesKeys,
  ...usersKeys,
  ...coursesKeys,
};
