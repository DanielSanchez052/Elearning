import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { quizzesApi } from '@/api/quizzes';

export const quizzesKeys = {
  all: ['quizzes'] as const,
  lesson: (lessonId: string) => ['quizzes', 'lesson', lessonId] as const,
  courseExam: (courseId: string) =>
    ['quizzes', 'courseExam', courseId] as const,
  results: {
    all: ['quizzes', 'results'] as const,
    lesson: (lessonId: string) =>
      ['quizzes', 'results', 'lesson', lessonId] as const,
    courseExam: (courseId: string) =>
      ['quizzes', 'results', 'courseExam', courseId] as const,
  },
};

export function useLessonQuizzes(lessonId: string, enabled = true) {
  return useQuery({
    queryKey: quizzesKeys.lesson(lessonId),
    queryFn: () => quizzesApi.getLessonQuizzes(lessonId).then((r) => r.data),
    enabled,
  });
}

export function useCourseExam(courseId: string, enabled = true) {
  return useQuery({
    queryKey: quizzesKeys.courseExam(courseId),
    queryFn: () => quizzesApi.getCourseExam(courseId).then((r) => r.data),
    enabled,
  });
}

export function useSubmitLessonQuiz() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: {
      lessonId: string;
      selectedOptionIds: string[];
    }) =>
      quizzesApi.submitLessonQuiz(payload.lessonId, {
        lessonId: payload.lessonId,
        courseId: null,
        selectedOptionIds: payload.selectedOptionIds,
      }),
    onSuccess: (_, payload) => {
      queryClient.invalidateQueries({
        queryKey: quizzesKeys.lesson(payload.lessonId),
      });
      queryClient.invalidateQueries({
        queryKey: quizzesKeys.results.lesson(payload.lessonId),
      });
    },
  });
}

export function useSubmitCourseExam() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: {
      courseId: string;
      selectedOptionIds: string[];
    }) =>
      quizzesApi.submitCourseExam(payload.courseId, {
        lessonId: null,
        courseId: payload.courseId,
        selectedOptionIds: payload.selectedOptionIds,
      }),
    onSuccess: (_, payload) => {
      queryClient.invalidateQueries({
        queryKey: quizzesKeys.courseExam(payload.courseId),
      });
      queryClient.invalidateQueries({
        queryKey: quizzesKeys.results.courseExam(payload.courseId),
      });
    },
  });
}

export function useLessonResults(lessonId: string, enabled = true) {
  return useQuery({
    queryKey: quizzesKeys.results.lesson(lessonId),
    queryFn: () => quizzesApi.getLessonResults(lessonId).then((r) => r.data),
    enabled,
  });
}

export function useCourseExamResults(courseId: string, enabled = true) {
  return useQuery({
    queryKey: quizzesKeys.results.courseExam(courseId),
    queryFn: () => quizzesApi.getCourseExamResults(courseId).then((r) => r.data),
    enabled,
  });
}
