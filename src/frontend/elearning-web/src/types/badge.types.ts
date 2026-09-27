// Mirrors ELearning.Application.Features.Gamification.DTOs.UserBadgeDto
// (GET /api/badges/me, camelCase JSON).
export interface UserBadgeDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  obtainedAt: string;
  /** Set for course-scoped badges (CourseDone, Speedster); null for LoginFirst. */
  courseId: string | null;
  courseTitle: string | null;
}
