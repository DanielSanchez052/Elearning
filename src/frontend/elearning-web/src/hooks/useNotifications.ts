import { useQuery, useMutation } from '@tanstack/react-query';
import { notificationsApi } from '../api/notifications';

export const useNotifications = () => {
  return useQuery({
    queryKey: ['notifications'],
    queryFn: () => notificationsApi.getUserNotifications().then((r) => r.data),
    refetchInterval: 30000, // Refresh every 30 seconds
  });
};

export const useMarkNotificationRead = () => {
  return useMutation({
    mutationFn: (notificationId: string) =>
      notificationsApi.markNotificationRead(notificationId),
  });
};
