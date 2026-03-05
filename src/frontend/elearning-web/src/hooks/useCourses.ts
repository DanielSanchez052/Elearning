import { useQuery, useMutation } from '@tanstack/react-query';
import { coursesApi } from '../api/courses';

export const useCourses = () => {
  return useQuery({
    queryKey: ['courses'],
    queryFn: () => coursesApi.getCatalog(),
  });
};

export const useCourseById = (courseId: string) => {
  return useQuery({
    queryKey: ['course', courseId],
    queryFn: () => coursesApi.getCourseById(courseId),
    enabled: !!courseId,
  });
};

export const useEnrollCourse = () => {
  return useMutation({
    mutationFn: (courseId: string) => coursesApi.enrollCourse(courseId),
  });
};

export const useUserEnrollments = () => {
  return useQuery({
    queryKey: ['enrollments'],
    queryFn: () => coursesApi.getUserEnrollments(),
  });
};

export const useCourseProgress = (courseId: string) => {
  return useQuery({
    queryKey: ['courseProgress', courseId],
    queryFn: () => coursesApi.getCourseProgress(courseId),
    enabled: !!courseId,
  });
};
