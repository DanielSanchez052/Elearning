import { useRef, useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { ROLES, type UserRole } from '@/types/user.types';
import { AppRoot, Avatar, Button } from '@/components/ui';
import {
  ArrowLeftIcon,
  BookOpenIcon,
  GlobeIcon,
  GraduationCapIcon,
  LayoutDashboardIcon,
  LogOutIcon,
  MenuIcon,
  UsersIcon,
  XIcon,
} from '@/components/ui/icons';
import { useDialogBehavior } from '@/components/ui/useDialogBehavior';
import { cn } from '@/lib/utils';

const navItems: { to: string; label: string; roles: UserRole[]; Icon: typeof BookOpenIcon }[] = [
  { to: '/admin', label: 'Dashboard', roles: ['admin', 'superadmin'], Icon: LayoutDashboardIcon },
  { to: '/admin/users', label: 'Usuarios', roles: ['admin', 'superadmin'], Icon: UsersIcon },
  { to: '/admin/courses', label: 'Cursos', roles: ['instructor', 'admin', 'superadmin'], Icon: BookOpenIcon },
  { to: '/admin/countries', label: 'Países', roles: ['superadmin'], Icon: GlobeIcon },
];

const itemClass = 'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring';

/**
 * Shell de administración (AdminSidebar del rediseño): sidebar fija de w-64 con navegación por rol,
 * vuelta al área del estudiante y usuario. En móvil la sidebar se abre como panel desde una barra superior.
 */
export default function AdminLayout() {
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const navigate = useNavigate();
  const location = useLocation();
  const [drawer, setDrawer] = useState<{ open: boolean; path: string }>({ open: false, path: location.pathname });

  // El panel móvil se cierra solo al cambiar de ruta.
  const drawerOpen = drawer.open && drawer.path === location.pathname;
  const closeDrawer = () => setDrawer({ open: false, path: location.pathname });
  const drawerRef = useRef<HTMLDivElement>(null);
  useDialogBehavior(drawerOpen, closeDrawer, drawerRef);

  const visibleItems = navItems.filter((item) => (user?.role ? item.roles.includes(user.role) : false));
  const roleLabel = user ? ROLES[user.role] ?? user.role : '';

  const handleLogout = () => {
    clearAuth();
    navigate('/login');
  };

  const sidebar = (
    <>
      <div className="flex h-16 shrink-0 items-center gap-2.5 border-b border-border px-5">
        <span className="flex size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <GraduationCapIcon className="size-4" aria-hidden="true" />
        </span>
        <div className="min-w-0 flex-1 leading-tight">
          <p className="text-sm font-semibold text-foreground">ELearning</p>
          <p className="text-xs text-muted-foreground">Administración</p>
        </div>
        <Button variant="ghost" size="icon" className="md:hidden" aria-label="Cerrar menú" onClick={closeDrawer}>
          <XIcon />
        </Button>
      </div>

      <nav aria-label="Administración" className="flex flex-1 flex-col gap-1 overflow-y-auto p-3">
        {visibleItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/admin'}
            className={({ isActive }) =>
              cn(
                itemClass,
                isActive ? 'bg-secondary font-medium text-foreground' : 'text-muted-foreground hover:bg-secondary hover:text-foreground'
              )
            }
          >
            <item.Icon className="size-4" aria-hidden="true" />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="flex flex-col gap-1 border-t border-border p-3">
        <Link to="/dashboard" className={cn(itemClass, 'text-muted-foreground hover:bg-secondary hover:text-foreground')}>
          <ArrowLeftIcon className="size-4" aria-hidden="true" />
          Área del estudiante
        </Link>
        {user && (
          <div className="mt-2 flex items-center gap-3 rounded-lg px-3 py-2">
            <Avatar name={user.fullName} />
            <div className="min-w-0 flex-1 leading-tight">
              <p className="truncate text-sm font-medium text-foreground">{user.fullName}</p>
              <p className="truncate text-xs text-muted-foreground">{roleLabel}</p>
            </div>
            <Button variant="ghost" size="icon" aria-label="Salir" title="Salir" onClick={handleLogout}>
              <LogOutIcon />
            </Button>
          </div>
        )}
      </div>
    </>
  );

  return (
    <AppRoot className="flex min-h-screen flex-col md:flex-row">
      {/* Barra superior solo en móvil */}
      <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b border-border bg-sidebar px-4 md:hidden">
        <Button
          variant="ghost"
          size="icon"
          aria-label="Abrir menú"
          aria-expanded={drawerOpen}
          aria-controls="admin-sidebar"
          onClick={() => setDrawer({ open: true, path: location.pathname })}
        >
          <MenuIcon />
        </Button>
        <p className="text-sm font-semibold text-foreground">Administración</p>
      </header>

      {/* Sidebar fija en escritorio */}
      <aside className="sticky top-0 hidden h-screen w-64 shrink-0 flex-col border-r border-border bg-sidebar md:flex">
        {sidebar}
      </aside>

      {/* Sidebar como panel en móvil */}
      {drawerOpen && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div className="absolute inset-0 bg-overlay backdrop-blur-sm" onClick={closeDrawer} aria-hidden="true" />
          <div
            ref={drawerRef}
            tabIndex={-1}
            id="admin-sidebar"
            role="dialog"
            aria-modal="true"
            aria-label="Menú de administración"
            className="relative flex h-full w-64 max-w-[85vw] flex-col border-r border-border bg-sidebar shadow-2xl focus:outline-none"
          >
            {sidebar}
          </div>
        </div>
      )}

      <main className="min-w-0 flex-1">
        <Outlet />
      </main>
    </AppRoot>
  );
}
