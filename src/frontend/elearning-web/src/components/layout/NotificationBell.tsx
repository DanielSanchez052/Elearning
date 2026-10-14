import { useRef, useState } from 'react';
import { useNotifications } from '../../hooks/useNotifications';
import { NotificationPanel } from './NotificationPanel';
import { Button } from '@/components/ui';
import { BellIcon } from '@/components/ui/icons';

export const NotificationBell = () => {
  const { data: notifications } = useNotifications();
  const unreadCount = notifications?.filter((n) => !n.isRead).length || 0;
  const [isOpen, setIsOpen] = useState(false);
  // Covers the button AND the panel, so the panel's outside-click check doesn't
  // treat clicking the bell itself as "outside" (which would close then
  // immediately reopen it via the button's own toggle).
  const containerRef = useRef<HTMLDivElement>(null);

  return (
    <div className="relative" ref={containerRef}>
      <Button
        variant="ghost"
        size="icon"
        aria-label={unreadCount > 0 ? `Notificaciones, ${unreadCount} sin leer` : 'Notificaciones'}
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        onClick={() => setIsOpen((prev) => !prev)}
      >
        <BellIcon />
      </Button>
      {unreadCount > 0 && (
        <span
          aria-hidden="true"
          className="pointer-events-none absolute -top-1 -right-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-primary-foreground"
        >
          {unreadCount > 9 ? '9+' : unreadCount}
        </span>
      )}
      {isOpen && <NotificationPanel onClose={() => setIsOpen(false)} containerRef={containerRef} />}
    </div>
  );
};
