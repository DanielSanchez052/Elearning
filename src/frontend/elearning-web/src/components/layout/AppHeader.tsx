import { useState } from 'react';
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { ROLES } from '@/types/user.types';
import { Avatar, Button } from '@/components/ui';
import { GraduationCapIcon, LogOutIcon, MenuIcon, ShieldIcon, XIcon } from '@/components/ui/icons';
import { NotificationBell } from '@/components/layout/NotificationBell';
import { cn } from '@/lib/utils';
import { adminHomeFor } from '@/lib/roles';

const navItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/courses', label: 'Cursos' },
];

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'rounded-lg px-3 py-1.5 text-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
    isActive ? 'bg-secondary font-medium text-foreground' : 'text-muted-foreground hover:text-foreground'
  );

const mobileLinkClass = 'rounded-lg px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground';

/**
 * Barra superior del área del estudiante (StudentTopbar del rediseño): logo, navegación,
 * campana de notificaciones, acceso al panel admin, usuario y salir; menú colapsado en móvil.
 */
export default function AppHeader() {
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const navigate = useNavigate();
  const location = useLocation();
  const [menu, setMenu] = useState<{ open: boolean; path: string }>({ open: false, path: location.pathname });

  // El menú móvil se cierra solo al cambiar de ruta.
  const menuOpen = menu.open && menu.path === location.pathname;
  const adminHome = adminHomeFor(user?.role);
  const roleLabel = user ? ROLES[user.role] ?? user.role : '';

  const handleLogout = () => {
    clearAuth();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-30 border-b border-border bg-sidebar">
      <div className="mx-auto flex h-16 max-w-7xl items-center gap-6 px-4 sm:px-6">
        <Link to="/dashboard" className="flex items-center gap-2.5 rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
          <span className="flex size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <GraduationCapIcon className="size-4" aria-hidden="true" />
          </span>
          <span className="text-base font-semibold text-foreground">ELearning</span>
        </Link>

        <nav aria-label="Principal" className="hidden items-center gap-1 md:flex">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} className={navLinkClass}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex-1" />

        <div className="flex items-center gap-2">
          {adminHome && (
            <Link
              to={adminHome}
              className="hidden items-center gap-2 rounded-lg px-3 py-1.5 text-xs font-medium text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground md:inline-flex"
            >
              <ShieldIcon className="size-4" aria-hidden="true" /> Panel admin
            </Link>
          )}

          <NotificationBell />

          {user && (
            <Link
              to="/profile"
              className="hidden items-center gap-3 rounded-lg border-l border-border py-1 pr-2 pl-3 transition-colors hover:bg-secondary md:flex"
              title="Mi perfil"
            >
              <Avatar name={user.fullName} />
              <span className="leading-tight">
                <span className="block max-w-40 truncate text-sm font-medium text-foreground">{user.fullName}</span>
                <span className="block text-xs text-muted-foreground">{roleLabel}</span>
              </span>
            </Link>
          )}

          <Button variant="ghost" size="sm" className="hidden md:inline-flex" onClick={handleLogout}>
            <LogOutIcon /> Salir
          </Button>

          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            aria-label={menuOpen ? 'Cerrar menú' : 'Abrir menú'}
            aria-expanded={menuOpen}
            aria-controls="student-mobile-menu"
            onClick={() => setMenu({ open: !menuOpen, path: location.pathname })}
          >
            {menuOpen ? <XIcon /> : <MenuIcon />}
          </Button>
        </div>
      </div>

      {menuOpen && (
        <div id="student-mobile-menu" className="border-t border-border px-4 py-4 sm:px-6 md:hidden">
          {user && (
            <div className="flex items-center gap-3 pb-4">
              <Avatar name={user.fullName} size="md" />
              <div className="min-w-0">
                <p className="truncate text-sm font-medium text-foreground">{user.fullName}</p>
                <p className="text-xs text-muted-foreground">{roleLabel}</p>
              </div>
            </div>
          )}
          <nav aria-label="Principal" className="flex flex-col gap-1 border-t border-border pt-3">
            {navItems.map((item) => (
              <NavLink key={item.to} to={item.to} className={({ isActive }) => cn(mobileLinkClass, isActive && 'bg-secondary font-medium text-foreground')}>
                {item.label}
              </NavLink>
            ))}
            <NavLink to="/profile" className={({ isActive }) => cn(mobileLinkClass, isActive && 'bg-secondary font-medium text-foreground')}>
              Mi perfil
            </NavLink>
            {adminHome && (
              <Link to={adminHome} className={mobileLinkClass}>
                Panel admin
              </Link>
            )}
            <button type="button" onClick={handleLogout} className={cn(mobileLinkClass, 'text-left')}>
              Salir
            </button>
          </nav>
        </div>
      )}
    </header>
  );
}
