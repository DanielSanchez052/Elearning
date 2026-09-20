import axios from '../lib/axios';

export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export const notificationsApi = {
  getUserNotifications: () =>
    axios.get<NotificationDto[]>('/notifications'),

  createNotification: (data: any) =>
    axios.post('/notifications', data),

  markNotificationRead: (notificationId: string) =>
    axios.put(`/notifications/${notificationId}/mark-read`),
};
