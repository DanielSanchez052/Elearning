import { useAuthStore } from '@/store/authStore';
import { Link } from 'react-router-dom';

export default function AdminDashboard() {
  const user = useAuthStore((s) => s.user);

  const adminSections = [
    {
      title: 'Usuarios',
      description: 'Gestiona usuarios, roles y permisos',
      icon: (
        <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 8.048M12 4.354L9.172 7.172M12 4.354l2.828 2.818m0 0a4 4 0 110-8.048m0 8.048L9.172 7.172m2.828 2.818L15.828 11m0 0h4.356m-4.356 0l2.828 2.828M12.364 20c-2.239 0-4.306-.982-5.744-2.56M12.364 20l2.636 2.636M12.364 20c2.239 0 4.306-.982 5.744-2.56M4.62 17.44C3.08 15.94 2 13.879 2 11.5c0-3.314 2.686-6 6-6s6 2.686 6 6c0 2.379-1.08 4.44-2.62 5.94m0 0A9.953 9.953 0 0012 20.5c-5.522 0-10-4.478-10-10S6.478 0.5 12 0.5s10 4.478 10 10c0 .875-.1 1.728-.3 2.551" />
        </svg>
      ),
      href: '/admin/users',
      color: 'indigo',
    },
    {
      title: 'Cursos',
      description: 'Administra cursos, lecciones y contenido',
      icon: (
        <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
        </svg>
      ),
      href: '/admin/courses',
      color: 'emerald',
    },
    {
      title: 'Países',
      description: 'Configura países y regiones disponibles',
      icon: (
        <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20H7m6-4h6v2a3 3 0 01-3 3h-6a3 3 0 01-3-3v-2" />
        </svg>
      ),
      href: '/admin/countries',
      color: 'blue',
    },
  ];

  const getColorClasses = (color: string) => {
    const colorMap: { [key: string]: { bg: string; border: string; text: string } } = {
      indigo: {
        bg: 'indigo-500/20',
        border: 'indigo-500/30',
        text: 'text-indigo-400',
      },
      emerald: {
        bg: 'emerald-500/20',
        border: 'emerald-500/30',
        text: 'text-emerald-400',
      },
      blue: {
        bg: 'blue-500/20',
        border: 'blue-500/30',
        text: 'text-blue-400',
      },
    };
    return colorMap[color] || colorMap.indigo;
  };

  return (
    <div className="space-y-8 p-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-white">Panel de Administración</h1>
        <p className="text-zinc-400 mt-2">
          Bienvenido, {user?.fullName}
        </p>
      </div>

      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {adminSections.map((section) => {
          const colors = getColorClasses(section.color);
          return (
            <Link
              key={section.href}
              to={section.href}
              className="group bg-[#111118] border border-white/[0.06] rounded-2xl p-6 hover:border-white/[0.12] transition-all duration-300 shadow-lg hover:shadow-xl"
            >
              <div
                className={`w-12 h-12 rounded-xl ${colors.bg} border ${colors.border} mb-4 flex items-center justify-center group-hover:scale-110 transition-transform ${colors.text}`}
              >
                {section.icon}
              </div>
              <h3 className="text-lg font-semibold text-white mb-2 group-hover:text-indigo-400 transition-colors">
                {section.title}
              </h3>
              <p className="text-sm text-zinc-400">{section.description}</p>
            </Link>
          );
        })}
      </div>

      {/* Stats Section - Placeholder */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-[#111118] border border-white/[0.06] rounded-xl p-4">
          <p className="text-xs text-zinc-500 uppercase tracking-widest mb-2">
            Usuarios Totales
          </p>
          <p className="text-2xl font-bold text-white">—</p>
        </div>
        <div className="bg-[#111118] border border-white/[0.06] rounded-xl p-4">
          <p className="text-xs text-zinc-500 uppercase tracking-widest mb-2">
            Cursos Activos
          </p>
          <p className="text-2xl font-bold text-white">—</p>
        </div>
        <div className="bg-[#111118] border border-white/[0.06] rounded-xl p-4">
          <p className="text-xs text-zinc-500 uppercase tracking-widest mb-2">
            Inscripciones
          </p>
          <p className="text-2xl font-bold text-white">—</p>
        </div>
        <div className="bg-[#111118] border border-white/[0.06] rounded-xl p-4">
          <p className="text-xs text-zinc-500 uppercase tracking-widest mb-2">
            Regiones
          </p>
          <p className="text-2xl font-bold text-white">—</p>
        </div>
      </div>

      {/* Info Box */}
      <div className="bg-indigo-500/10 border border-indigo-500/20 rounded-xl p-6">
        <h3 className="text-white font-semibold mb-2">Próximamente</h3>
        <p className="text-indigo-200 text-sm">
          Más funcionalidades y estadísticas detalladas estarán disponibles en breve.
        </p>
      </div>
    </div>
  );
}
