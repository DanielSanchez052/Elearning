import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { coursesApi, type GetCatalogParams } from '@/api/courses';

export const courseKeys = {
  all: ['courses'] as const,
  catalog: (params: GetCatalogParams) => ['courses', 'catalog', params] as const,
  detail: (id: string) => ['courses', 'detail', id] as const,
};

export function useCourseCatalog(params: GetCatalogParams = {}) {
  return useQuery({
    queryKey: courseKeys.catalog(params),
    queryFn: () => coursesApi.getCatalog(params).then((r) => r.data),
    placeholderData: keepPreviousData, // evita flash al cambiar de página
    staleTime: 1000 * 60 * 2,
  });
}

export function useCourseDetail(id: string) {
  return useQuery({
    queryKey: courseKeys.detail(id),
    queryFn: () => coursesApi.getDetail(id).then((r) => r.data),
    enabled: !!id,
    staleTime: 1000 * 60 * 5,
  });
}