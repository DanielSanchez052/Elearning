import axios from '@/lib/axios';
import type {
  CourseProgressDto,
  EnrollmentSummaryDto,
  MarkLessonCompleteResult,
} from '@/types/enrollment.types';

export const enrollmentsApi = {
  enrollInCourse: (courseId: string) =>
    axios.post<{ value: string }>('/enrollments', { courseId }),

  getMyEnrollments: () =>
    axios.get<EnrollmentSummaryDto[]>('/enrollments/me'),

  getCourseProgress: (courseId: string) =>
    axios.get<CourseProgressDto>(`/enrollments/me/courses/${courseId}`),

  markLessonComplete: (courseId: string, lessonId: string) =>
    axios.post<MarkLessonCompleteResult>(
      `/enrollments/me/courses/${courseId}/lessons/${lessonId}/complete`
    ),
};
