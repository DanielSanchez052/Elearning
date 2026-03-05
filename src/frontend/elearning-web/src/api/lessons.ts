import axios from '../lib/axios';

export const lessonsApi = {
  getLessonsByCourse: (courseId: string) =>
    axios.get(`/lessons?courseId=${courseId}`),

  getLessonById: (lessonId: string) =>
    axios.get(`/lessons/${lessonId}`),

  createLesson: (data: any) =>
    axios.post('/lessons', data),

  uploadLessonMedia: (lessonId: string, formData: FormData) =>
    axios.post(`/lessons/${lessonId}/upload-media`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),

  completeLesson: (courseId: string, lessonId: string) =>
    axios.post(`/lessons/${lessonId}/complete`, { courseId }),
};
