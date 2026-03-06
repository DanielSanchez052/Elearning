// Exportar APIs por módulo
export { countriesApi } from './countries';
export { usersApi, type GetUsersParams } from './users';
export { coursesApi, type GetAdminCoursesParams } from './courses';

// Agregador para mantener compatibilidad
import { countriesApi } from './countries';
import { usersApi } from './users';
import { coursesApi } from './courses';

export const adminApi = {
  ...countriesApi,
  ...usersApi,
  ...coursesApi,
};
