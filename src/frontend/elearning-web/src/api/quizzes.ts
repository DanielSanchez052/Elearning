import axios from '../lib/axios';
import type { SubmitQuizRequest, QuizQuestion, QuizResultDto, QuizAttemptDto } from '../types/quiz.types';

export const quizzesApi = {
  // Read endpoints (student)
  getLessonQuizzes: (lessonId: string) =>
    axios.get<QuizQuestion[]>(`/quizzes/lessons/${lessonId}`),

  getCourseExam: (courseId: string) =>
    axios.get<QuizQuestion[]>(`/quizzes/courses/${courseId}/exam`),

  // Submit endpoints
  submitLessonQuiz: (lessonId: string, data: SubmitQuizRequest) =>
    axios.post<QuizResultDto>(`/quizzes/lessons/${lessonId}/submit`, data),

  submitCourseExam: (courseId: string, data: SubmitQuizRequest) =>
    axios.post<QuizResultDto>(`/quizzes/courses/${courseId}/exam/submit`, data),

  // Results endpoints
  getLessonResults: (lessonId: string) =>
    axios.get<QuizAttemptDto[]>(`/quizzes/lessons/${lessonId}/results`),

  getCourseExamResults: (courseId: string) =>
    axios.get<QuizAttemptDto[]>(
      `/quizzes/courses/${courseId}/exam/results`
    ),
};
