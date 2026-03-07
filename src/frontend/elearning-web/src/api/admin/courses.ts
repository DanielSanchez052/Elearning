import axios from '@/lib/axios';
import type { PagedResult, CourseSummaryDto } from '@/types';
import type { UploadResultDto } from '../../types';

// ── Course mutations ──────────────────────────────────────────────────────────

export interface CreateCourseRequest {
  title: string;
  description?: string;
  thumbnailUrl?: string;
  isGlobal: boolean;
}

export interface UpdateCourseRequest {
  title: string;
  description?: string;
  thumbnailUrl?: string;
  isGlobal: boolean;
}

export interface CreateLessonRequest {
  title: string;
  type: string;
  contentUrl?: string;
  isRequired: boolean;
}

export interface UpdateLessonRequest {
  title: string;
  contentUrl?: string;
  isRequired: boolean;
}

export interface AssignCountriesRequest {
  countryIds: number[];
}

export interface LessonOrderItem {
  lessonId: string;
  newOrder: number;
}

export interface ReorderLessonsRequest {
  orders: LessonOrderItem[];
}

// ── Params ────────────────────────────────────────────────────────────────────

export interface GetAdminCoursesParams {
  instructorId?: string;
  countryId?: number;
  isActive?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

// ── API ───────────────────────────────────────────────────────────────────────

export const coursesApi = {
  getAdminCourses: (params: GetAdminCoursesParams = {}) =>
    axios.get<PagedResult<CourseSummaryDto>>('/courses/admin', { params }),

  toggleCourseStatus: (id: string) =>
    axios.patch(`/courses/${id}/toggle-status`),

  deleteCourse: (id: string) =>
    axios.delete(`/courses/${id}`),
  // Courses
  createCourse: (data: CreateCourseRequest) =>
    axios.post<{ value: string }>('/courses', data),

  updateCourse: (id: string, data: UpdateCourseRequest) =>
    axios.put(`/courses/${id}`, data),

  assignCountries: (id: string, data: AssignCountriesRequest) =>
    axios.put(`/courses/${id}/countries`, data),

  // Lessons
  createLesson: (courseId: string, data: CreateLessonRequest) =>
    axios.post<{ value: string }>(`/courses/${courseId}/lessons`, data),

  updateLesson: (courseId: string, lessonId: string, data: UpdateLessonRequest) =>
    axios.put(`/courses/${courseId}/lessons/${lessonId}`, data),

  deleteLesson: (courseId: string, lessonId: string) =>
    axios.delete(`/courses/${courseId}/lessons/${lessonId}`),

  reorderLessons: (courseId: string, data: ReorderLessonsRequest) =>
    axios.patch(`/courses/${courseId}/lessons/reorder`, data),

  // Media uploads
  uploadThumbnail: (file: File, onProgress?: (pct: number) => void) => {
    const form = new FormData();
    form.append('file', file);
    return axios.post<UploadResultDto>('/media/thumbnails', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        if (onProgress && e.total) onProgress(Math.round((e.loaded * 100) / e.total));
      },
    });
  },

  uploadVideo: (file: File, onProgress?: (pct: number) => void) => {
    const form = new FormData();
    form.append('file', file);
    return axios.post<UploadResultDto>('/media/videos', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 0,
      onUploadProgress: (e) => {
        if (onProgress && e.total) onProgress(Math.round((e.loaded * 100) / e.total));
      },
    });
  },

  uploadPdf: (file: File, onProgress?: (pct: number) => void) => {
    const form = new FormData();
    form.append('file', file);
    return axios.post<UploadResultDto>('/media/pdfs', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        if (onProgress && e.total) onProgress(Math.round((e.loaded * 100) / e.total));
      },
    });
  },
};
