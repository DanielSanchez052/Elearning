import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { coursesApi, type CreateCourseRequest, type GetAdminCoursesParams, type UpdateCourseRequest } from '@/api/admin/courses';
import { courseKeys } from '@/hooks/useCourses';

// ── Query keys ────────────────────────────────────────────────────────────────

export const coursesKeys = {
  all: ['admin', 'courses'] as const,
  byParams: (params: GetAdminCoursesParams) => ['admin', 'courses', params] as const,
};

// ── Queries ───────────────────────────────────────────────────────────────────

export function useAdminCourses(params: GetAdminCoursesParams = {}) {
  return useQuery({
    queryKey: coursesKeys.byParams(params),
    queryFn: () => coursesApi.getAdminCourses(params).then((r) => r.data),
    placeholderData: keepPreviousData,
    staleTime: 1000 * 60 * 2,
  });
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useToggleCourseStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => coursesApi.toggleCourseStatus(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: coursesKeys.all }),
  });
}

export function useDeleteCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => coursesApi.deleteCourse(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: coursesKeys.all }),
  });
}

export function useCreateCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCourseRequest) =>
      coursesApi.createCourse(data).then((r) => r.data.value),
    onSuccess: () => qc.invalidateQueries({ queryKey: coursesKeys.all }),
  });
}

export function useUpdateCourse(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateCourseRequest) =>
      coursesApi.updateCourse(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: coursesKeys.all });
      qc.invalidateQueries({ queryKey: courseKeys.detail(id) });
    },
  });
}

export function useAssignCountries(courseId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (countryIds: number[]) =>
      coursesApi.assignCountries(courseId, { countryIds }),
    onSuccess: () => qc.invalidateQueries({ queryKey: courseKeys.detail(courseId) }),
  });
}

export function useCreateLesson(courseId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Parameters<typeof coursesApi.createLesson>[1]) =>
      coursesApi.createLesson(courseId, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: courseKeys.detail(courseId) }),
  });
}

export function useUpdateLesson(courseId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ lessonId, ...data }: { lessonId: string } & Parameters<typeof coursesApi.updateLesson>[2]) =>
      coursesApi.updateLesson(courseId, lessonId, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: courseKeys.detail(courseId) }),
  });
}

export function useDeleteLesson(courseId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (lessonId: string) =>
      coursesApi.deleteLesson(courseId, lessonId),
    onSuccess: () => qc.invalidateQueries({ queryKey: courseKeys.detail(courseId) }),
  });
}
