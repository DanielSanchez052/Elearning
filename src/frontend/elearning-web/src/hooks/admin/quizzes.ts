import { useMutation, useQueryClient } from '@tanstack/react-query';
import { quizzesAdminApi } from '@/api/admin/quizzes';
import { quizzesKeys } from '@/hooks/quizzes';
import type {
  CreateQuizQuestionRequest,
  UpdateQuizQuestionRequest,
  CreateQuizOptionRequest,
  UpdateQuizOptionRequest,
} from '@/types/quiz.types';

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
