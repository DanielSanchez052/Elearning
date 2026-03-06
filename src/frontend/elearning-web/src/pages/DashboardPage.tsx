import { useAuthStore } from '@/store/authStore';
import AdminAccessButton from '@/components/AdminAccessButton';

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);

  return (
    <div className="min-h-screen bg-gradient-to-br from-[#0a0a0f] to-[#1a1a2e] p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-12 flex items-start justify-between">
          <div>
            <h1 className="text-4xl font-bold text-white mb-2">
              Bienvenido, {user?.fullName}
            </h1>
            <p className="text-zinc-400">
              Tu panel de control y espacio personalizado
            </p>
          </div>
          <AdminAccessButton />
        </div>

        {/* Main Content - Placeholder */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          {/* Card 1 */}
          <div className="bg-[#111118] border border-white/[0.06] rounded-2xl p-6 shadow-lg">
            <div className="w-12 h-12 rounded-xl bg-indigo-500/20 border border-indigo-500/30 mb-4 flex items-center justify-center">
              <svg className="w-6 h-6 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
            </div>
            <h3 className="text-white font-semibold mb-1">Cursos Inscritos</h3>
            <p className="text-zinc-400 text-sm">Ver tus cursos activos</p>
          </div>

          {/* Card 2 */}
          <div className="bg-[#111118] border border-white/[0.06] rounded-2xl p-6 shadow-lg">
            <div className="w-12 h-12 rounded-xl bg-emerald-500/20 border border-emerald-500/30 mb-4 flex items-center justify-center">
              <svg className="w-6 h-6 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h3 className="text-white font-semibold mb-1">Progreso</h3>
            <p className="text-zinc-400 text-sm">Revisa tu avance</p>
          </div>

          {/* Card 3 */}
          <div className="bg-[#111118] border border-white/[0.06] rounded-2xl p-6 shadow-lg">
            <div className="w-12 h-12 rounded-xl bg-amber-500/20 border border-amber-500/30 mb-4 flex items-center justify-center">
              <svg className="w-6 h-6 text-amber-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h3 className="text-white font-semibold mb-1">Logros</h3>
            <p className="text-zinc-400 text-sm">Tus certificados y medallas</p>
          </div>
        </div>

        {/* Empty State Message */}
        <div className="bg-[#111118] border border-white/[0.06] rounded-2xl p-12 text-center shadow-lg">
          <h2 className="text-2xl font-semibold text-white mb-3">
            Tu dashboard está listo
          </h2>
          <p className="text-zinc-400 mb-8 max-w-md mx-auto">
            Contenido personalizado y estadísticas aparecerán aquí cuando comiences con tus cursos.
          </p>
          <a
            href="/courses"
            className="inline-block px-6 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-medium transition-colors"
          >
            Explorar cursos
          </a>
        </div>
      </div>
    </div>
  );
}
