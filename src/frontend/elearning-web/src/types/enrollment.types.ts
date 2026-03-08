export const EnrollmentStatus = {
  Active: 0,
  Completed: 1,
  Abandoned: 2,
} as const;

export type EnrollmentStatusCode =
  (typeof EnrollmentStatus)[keyof typeof EnrollmentStatus];

export type EnrollmentStatusValue =
  | EnrollmentStatusCode
  | 'Active'
  | 'Completed'
  | 'Abandoned';

export interface EnrollmentSummaryDto {
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  courseThumbnailUrl: string | null;
  status: EnrollmentStatusValue;
  totalLessons: number;
  requiredLessons: number;
  completedLessons: number;
  progressPercent: number;
  enrolledAt: string;
  completedAt: string | null;
  deadlineAt: string | null;
}

export interface LessonProgressDto {
  lessonId: string;
  title: string;
  type: 'video' | 'pdf' | 'quiz' | string;
  orderIndex: number;
  isRequired: boolean;
  isCompleted: boolean;
  completedAt: string | null;
  lastAccessedAt: string | null;
}

export interface CourseProgressDto {
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  courseThumbnailUrl: string | null;
  status: EnrollmentStatusValue;
  progressPercent: number;
  completedLessons: number;
  requiredLessons: number;
  enrolledAt: string;
  completedAt: string | null;
  lessons: LessonProgressDto[];
}

export interface MarkLessonCompleteResult {
  lessonWasAlreadyComplete: boolean;
  courseCompleted: boolean;
  completedLessons: number;
  totalRequiredLessons: number;
}
