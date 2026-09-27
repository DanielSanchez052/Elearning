import { useEffect } from 'react';
import type { RefObject } from 'react';
import {
  useNotifications,
  useMarkNotificationRead,
  useMarkAllNotificationsRead,
} from '../../hooks/useNotifications';
import type { NotificationDto } from '../../api/notifications';

interface NotificationPanelProps {
  onClose: () => void;
  /** Covers the bell button too, so clicking the bell isn't treated as "outside". */
  containerRef: RefObject<HTMLDivElement | null>;
}

function formatNotificationDate(value: string) {
  const date = new Date(value);
  const diffMs = Date.now() - date.getTime();
  const diffMinutes = Math.floor(diffMs / 60000);

  if (diffMinutes < 1) return 'Ahora mismo';
  if (diffMinutes < 60) return `Hace ${diffMinutes} min`;
  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `Hace ${diffHours} h`;
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 7) return `Hace ${diffDays} d`;
  return date.toLocaleDateString();
}

export const NotificationPanel = ({ onClose, containerRef }: NotificationPanelProps) => {
  const { data: notifications, isLoading } = useNotifications();
  const markAsRead = useMarkNotificationRead();
  const markAllAsRead = useMarkAllNotificationsRead();

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        onClose();
      }
    };
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [onClose, containerRef]);

  const hasUnread = (notifications ?? []).some((n) => !n.isRead);

  const handleNotificationClick = (notification: NotificationDto) => {
    if (!notification.isRead) {
      markAsRead.mutate(notification.id);
    }
  };

  return (
    <div className="absolute right-0 top-full mt-2 w-80 max-w-[90vw] rounded-lg border border-white/[0.08] bg-[#111118] shadow-xl z-50">
      <div className="flex items-center justify-between px-4 py-3 border-b border-white/[0.08]">
        <h3 className="text-sm font-semibold text-white">Notificaciones</h3>
        {hasUnread && (
          <button
            type="button"
            onClick={() => markAllAsRead.mutate()}
            disabled={markAllAsRead.isPending}
            className="text-xs text-indigo-400 hover:text-indigo-300 transition disabled:opacity-60"
          >
            Marcar todas como leídas
          </button>
        )}
      </div>

      <div className="max-h-96 overflow-y-auto">
        {isLoading ? (
          <p className="px-4 py-6 text-center text-sm text-zinc-400">Cargando notificaciones...</p>
        ) : (notifications ?? []).length === 0 ? (
          <p className="px-4 py-6 text-center text-sm text-zinc-400">No tienes notificaciones</p>
        ) : (
          <ul className="divide-y divide-white/[0.06]">
            {(notifications ?? []).map((notification) => (
              <li key={notification.id}>
                <button
                  type="button"
                  onClick={() => handleNotificationClick(notification)}
                  className={`w-full text-left px-4 py-3 transition hover:bg-white/[0.04] ${
                    notification.isRead ? '' : 'bg-indigo-500/[0.06]'
                  }`}
                >
                  <div className="flex items-start gap-2">
                    {!notification.isRead && (
                      <span className="mt-1.5 h-1.5 w-1.5 flex-shrink-0 rounded-full bg-indigo-400" />
                    )}
                    <div className={`min-w-0 flex-1 ${notification.isRead ? 'pl-3.5' : ''}`}>
                      <p className="text-sm font-medium text-white truncate">{notification.title}</p>
                      <p className="text-xs text-zinc-400 mt-0.5 line-clamp-2">{notification.message}</p>
                      <p className="text-[11px] text-zinc-500 mt-1">
                        {formatNotificationDate(notification.createdAt)}
                      </p>
                    </div>
                  </div>
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};
