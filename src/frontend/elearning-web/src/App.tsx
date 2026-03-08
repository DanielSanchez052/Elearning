import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ProtectedRoute, PublicRoute, RoleRoute } from './components/RouteGuards';

import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import ForgotPasswordPage from './pages/auth/ForgotPasswordPage';
import ResetPasswordPage from './pages/auth/ResetPasswordPage';
import VerifyEmailPage from './pages/auth/VerifyEmailPage';
import DashboardPage from './pages/DashboardPage';
import CatalogPage from './pages/courses/CatalogPage';
import CourseDetailPage from './pages/courses/CourseDetailPage';
import QuizSessionPage from './pages/quiz/QuizSessionPage';

import AdminLayout from './components/layout/AdminLayout';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminUsersPage from './pages/admin/AdminUsersPage';
import AdminCoursesPage from './pages/admin/AdminCoursesPage';
import AdminCountriesPage from './pages/admin/AdminCountriesPage';
import AdminCourseFormPage from './pages/admin/AdminCourseFormPage';
import { AdminQuizzesPage } from './pages/admin/AdminQuizzesPage';
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
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/courses" element={<CatalogPage />} />
            <Route path="/courses/:id" element={<CourseDetailPage />} />
            <Route path="/courses/:id/lessons/:lessonId/quiz" element={<QuizSessionPage />} />
            <Route path="/courses/:id/exam" element={<QuizSessionPage />} />

            {/* ── Solo instructores, admins y super_admin ─────────────────── */}
            <Route element={<RoleRoute allowedRoles={['instructor', 'admin', 'superadmin']} />}>
              <Route element={<AdminLayout />}>
                <Route path="/admin/courses" element={<AdminCoursesPage />} />
                <Route path="/admin/courses/new" element={<AdminCourseFormPage />} />
                <Route path="/admin/courses/:id/edit" element={<AdminCourseFormPage />} />
                <Route path="/admin/courses/:courseId/quizzes" element={<AdminQuizzesPage />} />
                <Route path="/admin/courses/:courseId/lessons/:lessonId/quizzes" element={<AdminQuizzesPage />} />
              </Route>
            </Route>

            {/* ── Solo admins y super_admin ───────────────────────────────── */}
            <Route element={<RoleRoute allowedRoles={['admin', 'superadmin']} />}>
              <Route element={<AdminLayout />}>
                <Route path="/admin" element={<AdminDashboard />} />
                <Route path="/admin/users" element={<AdminUsersPage />} />
                <Route path="/admin/countries" element={<AdminCountriesPage />} />
              </Route>
            </Route>

          </Route>

          {/* ── Utilidades ─────────────────────────────────────────────────── */}
          <Route path="/unauthorized" element={<div>No tienes permisos</div>} />
          <Route path="/" element={<LoginPage />} />
          <Route path="*" element={<div>404</div>} />

        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
