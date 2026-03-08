import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { quizzesApi } from '@/api/quizzes';
import { quizzesAdminApi } from '@/api/admin/quizzes';
import type {
  CreateQuizQuestionRequest,
  UpdateQuizQuestionRequest,
  CreateQuizOptionRequest,
  UpdateQuizOptionRequest,
} from '@/types/quiz.types';

// Query Keys
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

// ────── Student Hooks (Read / Submit) ──────────────────────────────────

/** Get all quiz questions for a lesson */
export function useLessonQuizzes(lessonId: string, enabled = true) {
  return useQuery({
    queryKey: quizzesKeys.lesson(lessonId),
    queryFn: () => quizzesApi.getLessonQuizzes(lessonId).then((r) => r.data),
    enabled,
  });
}

/** Get course final exam questions */
export function useCourseExam(courseId: string, enabled = true) {
  return useQuery({
    queryKey: quizzesKeys.courseExam(courseId),
    queryFn: () => quizzesApi.getCourseExam(courseId).then((r) => r.data),
    enabled,
  });
}

/** Submit lesson quiz answers */
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

/** Submit course exam answers */
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

/** Get lesson quiz results (student's attempts) */
export function useLessonResults(lessonId: string) {
  return useQuery({
    queryKey: quizzesKeys.results.lesson(lessonId),
    queryFn: () =>
      quizzesApi.getLessonResults(lessonId).then((r) => r.data),
  });
}

/** Get course exam results (student's attempts) */
export function useCourseExamResults(courseId: string) {
  return useQuery({
    queryKey: quizzesKeys.results.courseExam(courseId),
    queryFn: () =>
      quizzesApi.getCourseExamResults(courseId).then((r) => r.data),
  });
}

// ────── Admin Hooks (CRUD) ──────────────────────────────────────────────

/** Create a quiz question (PerLesson or CourseExam) */
export function useCreateQuizQuestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateQuizQuestionRequest) =>
      quizzesAdminApi.createQuestion(data),
    onSuccess: () => {
      // Invalidate all quiz-related queries
      queryClient.invalidateQueries({ queryKey: quizzesKeys.all });
    },
  });
}

/** Update a quiz question */
export function useUpdateQuizQuestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: {
      questionId: string;
      data: UpdateQuizQuestionRequest;
    }) =>
      quizzesAdminApi.updateQuestion(payload.questionId, payload.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: quizzesKeys.all });
    },
  });
}

/** Delete a quiz question */
export function useDeleteQuizQuestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (questionId: string) =>
      quizzesAdminApi.deleteQuestion(questionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: quizzesKeys.all });
    },
  });
}

/** Create a quiz option */
export function useCreateQuizOption() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: {
      questionId: string;
      data: CreateQuizOptionRequest;
    }) =>
      quizzesAdminApi.createOption(payload.questionId, payload.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: quizzesKeys.all });
    },
  });
}

/** Update a quiz option */
export function useUpdateQuizOption() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: {
      questionId: string;
      optionId: string;
      data: UpdateQuizOptionRequest;
    }) =>
      quizzesAdminApi.updateOption(
        payload.questionId,
        payload.optionId,
        payload.data
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: quizzesKeys.all });
    },
  });
}

/** Delete a quiz option */
export function useDeleteQuizOption() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: {
      questionId: string;
      optionId: string;
    }) =>
      quizzesAdminApi.deleteOption(payload.questionId, payload.optionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: quizzesKeys.all });
    },
  });
}
