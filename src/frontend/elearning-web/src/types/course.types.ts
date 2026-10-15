import type { Country } from "./user.types";

export type LessonType = 'video' | 'pdf' | 'quiz';

export interface LessonDto {
  id: string;
  title: string;
  type: LessonType;
  contentUrl: string | null;
  orderIndex: number;
  isRequired: boolean;
}

export interface CourseSummaryDto {
  id: string;
  title: string;
  description: string | null;
  thumbnailUrl: string | null;
  isGlobal: boolean;
  isActive: boolean;
  instructorId: string | null;
  instructorName: string | null;
  lessonCount: number;
  createdAt: string;
}

export interface CourseDetailDto {
  id: string;
  title: string;
  description: string | null;
  thumbnailUrl: string | null;
  isGlobal: boolean;
  isActive: boolean;
  instructorName: string | null;
  instructorId: string | null;
  lessons: LessonDto[];
  countries: Country[];
  createdAt: string;
  updatedAt: string;
}
