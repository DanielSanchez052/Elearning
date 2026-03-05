import axios from '@/lib/axios';
import type { PagedResult } from '@/types';
import type { CourseSummaryDto, CourseDetailDto } from '../types/course.types';

export interface GetCatalogParams {
  search?: string;
  page?: number;
  pageSize?: number;
}

export const coursesApi = {
  getCatalog: (params: GetCatalogParams = {}) =>
    axios.get<PagedResult<CourseSummaryDto>>('/courses', { params }),

  getDetail: (id: string) =>
    axios.get<CourseDetailDto>(`/courses/${id}`),
};