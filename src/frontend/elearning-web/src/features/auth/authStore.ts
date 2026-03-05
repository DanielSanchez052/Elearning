import { create } from 'zustand';
import type { CurrentUser, UserRole } from '@/types/user.types';

interface AuthState {
  user: CurrentUser | null;
  token: string | null;
  isLoggedIn: boolean;

  // Actions
  setAuth: (user: CurrentUser, token: string) => void;
  clearAuth: () => void;

  // Role helpers
  isRole: (role: UserRole) => boolean;
  isSuperAdmin: () => boolean;
  isAdmin: () => boolean;      // Admins also have super admin privileges
  isInstructor: () => boolean;
  isStudent: () => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: restoreUser(),
  token: localStorage.getItem('access_token'),
  isLoggedIn: !!localStorage.getItem('access_token'),

  setAuth: (user, token) => {
    localStorage.setItem('access_token', token);
    localStorage.setItem('user', JSON.stringify(user));
    set({ user, token, isLoggedIn: true });
  },

  clearAuth: () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('user');
    set({ user: null, token: null, isLoggedIn: false });
  },

  // Role helpers
  isRole: (role) => get().user?.role === role,
  isSuperAdmin: () => get().user?.role === 'superadmin',
  isAdmin: () => get().user?.role === 'admin' || get().user?.role === 'superadmin',
  isInstructor: () => get().user?.role === 'instructor',
  isStudent: () => get().user?.role === 'student',
}));

// ── Helpers ───────────────────────────────────────────────────────────────────

function restoreUser(): CurrentUser | null {
  try {
    const raw = localStorage.getItem('user');
    return raw ? (JSON.parse(raw) as CurrentUser) : null;
  } catch {
    return null;
  }
}