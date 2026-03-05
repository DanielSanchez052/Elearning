import axios from '../lib/axios';

export const quizzesApi = {
  getQuizByLesson: (lessonId: string) =>
    axios.get(`/quizzes?lessonId=${lessonId}`),

  createQuizQuestion: (data: any) =>
    axios.post('/quizzes/questions', data),

  submitQuizAnswer: (quizId: string, answers: any) =>
    axios.post(`/quizzes/${quizId}/submit`, { answers }),
};
