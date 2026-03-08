import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import AdminAccessButton from '@/components/AdminAccessButton';
import { NotificationBell } from '@/components/layout/NotificationBell';

export default function AppHeader() {
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate('/login');
  };

  return (
    <header className="sticky top-0 z-40 border-b border-white/[0.06] bg-[#0a0a0f]/90 backdrop-blur-sm">
      <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-6 py-3">
        <div className="flex items-center gap-6 min-w-0">
          <Link to="/dashboard" className="flex items-center gap-2.5">
            <div className="w-7 h-7 rounded-lg bg-indigo-500/20 border border-indigo-500/30 flex items-center justify-center flex-shrink-0">
              <svg className="w-3.5 h-3.5 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
            </div>
            <span className="text-sm font-semibold text-white">ELearning</span>
          </Link>

          <nav className="flex items-center gap-1 overflow-x-auto">
            <NavLink
              to="/dashboard"
              className={({ isActive }) =>
                `rounded-lg px-3 py-1.5 text-sm transition ${isActive ? 'bg-indigo-500/15 text-indigo-300' : 'text-zinc-400 hover:text-white hover:bg-white/[0.04]'}`
              }
            >
              Dashboard
            </NavLink>
            <NavLink
              to="/courses"
              className={({ isActive }) =>
                `rounded-lg px-3 py-1.5 text-sm transition ${isActive ? 'bg-indigo-500/15 text-indigo-300' : 'text-zinc-400 hover:text-white hover:bg-white/[0.04]'}`
              }
            >
              Cursos
            </NavLink>
          </nav>
        </div>

        <div className="flex items-center gap-2">
          <NotificationBell />
          <AdminAccessButton />
          <div className="hidden md:block rounded-lg border border-white/[0.08] bg-white/[0.03] px-3 py-1.5">
            <p className="text-xs text-white leading-tight truncate max-w-[160px]">{user?.fullName}</p>
            <p className="text-[11px] text-zinc-500 leading-tight mt-0.5">{user?.role}</p>
          </div>
          <button
            onClick={handleLogout}
            className="rounded-lg px-3 py-1.5 text-sm text-zinc-400 hover:text-red-300 hover:bg-red-500/10 transition"
          >
            Salir
          </button>
        </div>
      </div>
    </header>
  );
}
