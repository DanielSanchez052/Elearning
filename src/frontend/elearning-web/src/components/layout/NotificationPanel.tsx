import { useEffect } from 'react';
import type { RefObject } from 'react';
import {
  useNotifications,
  useMarkNotificationRead,
  useMarkAllNotificationsRead,
} from '../../hooks/useNotifications';
import { Button, Skeleton } from '@/components/ui';
import { BellIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';

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
  return date.toLocaleDateString('es', { day: 'numeric', month: 'short', year: 'numeric' });
}

/** Dropdown de notificaciones de la barra superior: marcar como leída una o todas, con estado vacío. */
export const NotificationPanel = ({ onClose, containerRef }: NotificationPanelProps) => {
  const { data: notifications, isLoading, isError, refetch } = useNotifications();
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

  const items = notifications ?? [];
  const hasUnread = items.some((n) => !n.isRead);

  return (
    <div
      role="dialog"
      aria-label="Notificaciones"
      className="absolute top-full right-0 z-40 mt-2 w-80 max-w-[calc(100vw-2rem)] overflow-hidden rounded-xl border border-border-strong bg-card shadow-2xl"
    >
      <div className="flex items-center justify-between gap-2 border-b border-border px-4 py-3">
        <p className="text-sm font-semibold text-foreground">Notificaciones</p>
        {hasUnread && (
          <Button variant="ghost" size="sm" onClick={() => markAllAsRead.mutate()} disabled={markAllAsRead.isPending}>
            Marcar todas como leídas
          </Button>
        )}
      </div>

      {isLoading ? (
        <div className="flex flex-col gap-3 px-4 py-4" aria-busy="true">
          <Skeleton className="h-12 rounded-lg" />
          <Skeleton className="h-12 rounded-lg" />
          <Skeleton className="h-12 rounded-lg" />
        </div>
      ) : isError ? (
        <div className="flex flex-col items-center gap-3 px-6 py-8 text-center">
          <p className="text-sm text-muted-foreground">No pudimos cargar tus notificaciones.</p>
          <Button variant="secondary" size="sm" onClick={() => refetch()}>
            Reintentar
          </Button>
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center gap-2 px-6 py-10 text-center">
          <span className="flex size-10 items-center justify-center rounded-full bg-secondary text-muted-foreground">
            <BellIcon className="size-5" aria-hidden="true" />
          </span>
          <p className="text-sm font-medium text-foreground">Sin notificaciones</p>
          <p className="text-xs text-muted-foreground">Te avisaremos de nuevas lecciones, medallas y certificados.</p>
        </div>
      ) : (
        <ul className="max-h-96 divide-y divide-border overflow-y-auto">
          {items.map((notification) => (
            <li key={notification.id} className="flex gap-3 px-4 py-3">
              <span
                aria-hidden="true"
                className={cn(
                  'mt-1.5 block size-1.5 shrink-0 rounded-full',
                  notification.isRead ? 'bg-transparent' : 'bg-primary'
                )}
              />
              <div className="min-w-0 flex-1">
                <p className="text-sm font-medium text-foreground">{notification.title}</p>
                <p className="mt-0.5 text-sm text-muted-foreground">{notification.message}</p>
                <div className="mt-1.5 flex items-center justify-between gap-2">
                  <span className="text-xs text-subtle-foreground">{formatNotificationDate(notification.createdAt)}</span>
                  {!notification.isRead && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => markAsRead.mutate(notification.id)}
                      disabled={markAsRead.isPending && markAsRead.variables === notification.id}
                    >
                      Marcar como leída
                    </Button>
                  )}
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};
