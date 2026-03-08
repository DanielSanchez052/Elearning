// Exportar APIs por módulo
export { countriesApi } from './countries';
export { usersApi, type GetUsersParams } from './users';
export { coursesApi, type GetAdminCoursesParams } from './courses';
export { quizzesAdminApi } from './quizzes';

// Agregador para mantener compatibilidad
import { countriesApi } from './countries';
import { usersApi } from './users';
import { coursesApi } from './courses';
import { quizzesAdminApi } from './quizzes';

export const adminApi = {
  ...countriesApi,
  ...usersApi,
  ...coursesApi,
  ...quizzesAdminApi,
};
