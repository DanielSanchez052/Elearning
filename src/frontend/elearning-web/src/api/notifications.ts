import axios from '../lib/axios';

export interface NotificationDto {
  id: string;
  message: string;
  read: boolean;
}

export const notificationsApi = {
  getUserNotifications: () =>
    axios.get<NotificationDto[]>('/notifications'),

  createNotification: (data: any) =>
    axios.post('/notifications', data),

  markNotificationRead: (notificationId: string) =>
    axios.put(`/notifications/${notificationId}/mark-read`),
};
