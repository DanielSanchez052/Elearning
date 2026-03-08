import axios from '@/lib/axios';
import type {
  CreateQuizQuestionRequest,
  UpdateQuizQuestionRequest,
  CreateQuizOptionRequest,
  UpdateQuizOptionRequest
} from '@/types/quiz.types';

// Questions
export const quizzesAdminApi = {
  // Questions Management
  createQuestion: (data: CreateQuizQuestionRequest) =>
    axios.post<{ value: string }>('/admin/quizzes/questions', data),

  updateQuestion: (questionId: string, data: UpdateQuizQuestionRequest) =>
    axios.put(`/admin/quizzes/questions/${questionId}`, data),

  deleteQuestion: (questionId: string) =>
    axios.delete(`/admin/quizzes/questions/${questionId}`),

  // Options Management
  createOption: (questionId: string, data: CreateQuizOptionRequest) =>
    axios.post<{ value: string }>(
      `/admin/quizzes/questions/${questionId}/options`,
      data
    ),

  updateOption: (
    questionId: string,
    optionId: string,
    data: UpdateQuizOptionRequest
  ) =>
    axios.put(
      `/admin/quizzes/questions/${questionId}/options/${optionId}`,
      data
    ),

  deleteOption: (questionId: string, optionId: string) =>
    axios.delete(
      `/admin/quizzes/questions/${questionId}/options/${optionId}`
    ),
};
