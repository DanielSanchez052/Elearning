export interface Notification {
  id: string;
  userId: string;
  type: 'ENROLLMENT' | 'LESSON_COMPLETED' | 'BADGE_EARNED' | 'COURSE_UPDATE' | 'ADMIN_MESSAGE';
  title: string;
  message: string;
  read: boolean;
  relatedId?: string;
  createdAt: string;
}

export interface NotificationResponse {
  id: string;
  userId: string;
  type: string;
  title: string;
  message: string;
  read: boolean;
  relatedId?: string;
  createdAt: string;
}
