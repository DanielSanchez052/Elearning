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
  useReorderLessons,
} from './courses';

export {
  useLessonQuizzes,
  useCourseExam,
  useSubmitLessonQuiz,
  useSubmitCourseExam,
  useLessonResults,
  useCourseExamResults,
  useCreateQuizQuestion,
  useUpdateQuizQuestion,
  useDeleteQuizQuestion,
  useCreateQuizOption,
  useUpdateQuizOption,
  useDeleteQuizOption,
  quizzesKeys,
} from './quizzes';

// Agregador de query keys para compatibilidad
import { countriesKeys } from './countries';
import { usersKeys } from './users';
import { coursesKeys } from './courses';
import { quizzesKeys } from './quizzes';

export const adminKeys = {
  ...countriesKeys,
  ...usersKeys,
  ...coursesKeys,
  ...quizzesKeys,
};
