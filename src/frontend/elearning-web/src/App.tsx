import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ProtectedRoute, PublicRoute, RoleRoute } from './components/RouteGuards';

// ── Páginas (las crearemos en cada feature) ───────────────────────────────────
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import ForgotPasswordPage from './pages/auth/ForgotPasswordPage';
import ResetPasswordPage from './pages/auth/ResetPasswordPage';
import VerifyEmailPage from './pages/auth/VerifyEmailPage';
// import DashboardPage       from './pages/DashboardPage';
// import CatalogPage         from './pages/courses/CatalogPage';
// import CourseDetailPage    from './pages/courses/CourseDetailPage';
// import AdminCoursesPage    from './pages/admin/AdminCoursesPage';
// import AdminUsersPage      from './pages/admin/AdminUsersPage';
// import AdminCountriesPage  from './pages/admin/AdminCountriesPage';
// import UnauthorizedPage    from './pages/UnauthorizedPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // No reintentar en errores 4xx — son errores del cliente, no transitorios
      retry: (failureCount, error: any) => {
        const status = error?.response?.status;
        if (status >= 400 && status < 500) return false;
        return failureCount < 2;
      },
      staleTime: 1000 * 60 * 5, // 5 minutos de caché por defecto
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>

          {/* ── Rutas públicas — redirigen al dashboard si ya está logueado ── */}
          <Route element={<PublicRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
            <Route path="/verify-email" element={<VerifyEmailPage />} />
          </Route>

          {/* ── Rutas protegidas — requieren autenticación ─────────────────── */}
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<div>Dashboard</div>} />
            <Route path="/courses" element={<div>Catalog</div>} />
            <Route path="/courses/:id" element={<div>Course Detail</div>} />

            {/* ── Solo instructores, admins y super_admin ─────────────────── */}
            <Route element={<RoleRoute allowedRoles={['instructor', 'admin', 'superadmin']} />}>
              <Route path="/courses/manage" element={<div>Manage Courses</div>} />
              <Route path="/courses/new" element={<div>New Course</div>} />
              <Route path="/courses/:id/edit" element={<div>Edit Course</div>} />
            </Route>

            {/* ── Solo admins y super_admin ───────────────────────────────── */}
            <Route element={<RoleRoute allowedRoles={['admin', 'superadmin']} />}>
              <Route path="/admin/users" element={<div>Admin Users</div>} />
              <Route path="/admin/courses" element={<div>Admin Courses</div>} />
              <Route path="/admin/countries" element={<div>Admin Countries</div>} />
            </Route>
          </Route>

          {/* ── Utilidades ─────────────────────────────────────────────────── */}
          <Route path="/unauthorized" element={<div>No tienes permisos</div>} />
          <Route path="/" element={<div>Landing</div>} />
          <Route path="*" element={<div>404</div>} />

        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
