import axios from '../lib/axios';

export const notificationsApi = {
  getUserNotifications: () =>
    axios.get('/notifications'),

  createNotification: (data: any) =>
    axios.post('/notifications', data),

  markNotificationRead: (notificationId: string) =>
    axios.put(`/notifications/${notificationId}/mark-read`),
};
