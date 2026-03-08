import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { enrollmentsApi } from '@/api/enrollments';
import { quizzesKeys } from '@/hooks/admin/quizzes';

export const enrollmentKeys = {
  all: ['enrollments'] as const,
  mine: () => ['enrollments', 'me'] as const,
  progress: (courseId: string) =>
    ['enrollments', 'me', 'courses', courseId] as const,
};

export function useMyEnrollments(enabled = true) {
  return useQuery({
    queryKey: enrollmentKeys.mine(),
    queryFn: () => enrollmentsApi.getMyEnrollments().then((r) => r.data),
    enabled,
    staleTime: 1000 * 60,
  });
}

export function useCourseProgress(courseId: string, enabled = true) {
  return useQuery({
    queryKey: enrollmentKeys.progress(courseId),
    queryFn: () => enrollmentsApi.getCourseProgress(courseId).then((r) => r.data),
    enabled: enabled && Boolean(courseId),
    staleTime: 1000 * 30,
  });
}

export function useEnrollInCourse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (courseId: string) => enrollmentsApi.enrollInCourse(courseId),
    onSuccess: (_, courseId) => {
      queryClient.invalidateQueries({ queryKey: enrollmentKeys.mine() });
      queryClient.invalidateQueries({ queryKey: enrollmentKeys.progress(courseId) });
      queryClient.invalidateQueries({ queryKey: quizzesKeys.courseExam(courseId) });
    },
  });
}

export function useMarkLessonComplete() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { courseId: string; lessonId: string }) =>
      enrollmentsApi.markLessonComplete(payload.courseId, payload.lessonId),
    onSuccess: (_, payload) => {
      queryClient.invalidateQueries({ queryKey: enrollmentKeys.mine() });
      queryClient.invalidateQueries({
        queryKey: enrollmentKeys.progress(payload.courseId),
      });
      queryClient.invalidateQueries({
        queryKey: quizzesKeys.courseExam(payload.courseId),
      });
      queryClient.invalidateQueries({
        queryKey: quizzesKeys.lesson(payload.lessonId),
      });
    },
  });
}
