import axios from '../lib/axios';

export const coursesApi = {
  getCatalog: (filters?: any) =>
    axios.get('/courses/catalog', { params: filters }),

  getCourseById: (courseId: string) =>
    axios.get(`/courses/${courseId}`),

  createCourse: (data: any) =>
    axios.post('/courses', data),

  updateCourse: (courseId: string, data: any) =>
    axios.put(`/courses/${courseId}`, data),

  publishCourse: (courseId: string) =>
    axios.post(`/courses/${courseId}/publish`),

  assignCourseToCountries: (courseId: string, countryIds: number[]) =>
    axios.post(`/courses/${courseId}/assign-countries`, { countryIds }),

  enrollCourse: (courseId: string) =>
    axios.post(`/enrollments`, { courseId }),

  getUserEnrollments: () =>
    axios.get('/enrollments'),

  getCourseProgress: (courseId: string) =>
    axios.get(`/enrollments/${courseId}/progress`),
};
