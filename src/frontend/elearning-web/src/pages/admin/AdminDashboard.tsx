import { Link } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { Card, StatCard } from '@/components/ui';
import { useOverviewReport } from '@/hooks/admin/reports';
import { formatNumber, formatPercent } from '@/lib/format';
import { ArrowRightIcon, BarChartIcon, BookOpenIcon, CheckCircleIcon, GlobeIcon, UsersIcon } from '@/components/ui/icons';
import type { UserRole } from '@/types/user.types';

const links: { to: string; title: string; text: string; cta: string; Icon: typeof UsersIcon; roles: UserRole[] }[] = [
  { to: '/admin/users', title: 'Usuarios', text: 'Busca usuarios, cambia su rol o su país.', cta: 'Ir a usuarios', Icon: UsersIcon, roles: ['admin', 'superadmin'] },
  { to: '/admin/courses', title: 'Cursos', text: 'Crea cursos, organiza lecciones y publica.', cta: 'Ir a cursos', Icon: BookOpenIcon, roles: ['admin', 'superadmin'] },
  { to: '/admin/reports', title: 'Reportes', text: 'Métricas, cursos populares y comparativa por país.', cta: 'Ver reportes', Icon: BarChartIcon, roles: ['admin', 'superadmin'] },
  { to: '/admin/countries', title: 'Países', text: 'Activa países y define dónde se ven los cursos.', cta: 'Ir a países', Icon: GlobeIcon, roles: ['superadmin'] },
];

export default function AdminDashboard() {
  const role = useAuthStore((s) => s.user?.role);
  const visible = links.filter((l) => role && l.roles.includes(role));
  // Las métricas son reportes de la plataforma: solo admin y superadmin (el resto ni llega a esta página).
  const { data: report } = useOverviewReport({ period: '30d' }, { enabled: role === 'admin' || role === 'superadmin' });

  return (
    <div className="flex flex-col gap-8 px-4 py-8 sm:px-8">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Panel de administración</h1>
        <p className="mt-1 text-sm text-muted-foreground">Gestiona usuarios, cursos y países de la plataforma.</p>
      </div>

      <section className="flex flex-col gap-4" aria-labelledby="quick-links">
        <h2 id="quick-links" className="text-xs font-medium tracking-wider text-muted-foreground uppercase">
          Accesos rápidos
        </h2>
        <div className="grid gap-4 md:grid-cols-3">
          {visible.map(({ to, title, text, cta, Icon }) => (
            <Card key={to} variant="interactive" className="relative flex flex-col gap-4 focus-within:ring-2 focus-within:ring-ring">
              <span className="flex size-10 items-center justify-center rounded-xl bg-brand/10 text-brand-foreground">
                <Icon className="size-5" aria-hidden="true" />
              </span>
              <div>
                <h3 className="text-base font-semibold text-foreground">
                  <Link to={to} className="after:absolute after:inset-0 focus-visible:outline-none">
                    {title}
                  </Link>
                </h3>
                <p className="mt-1 text-sm text-muted-foreground">{text}</p>
              </div>
              <span className="mt-auto flex items-center gap-1 text-sm font-medium text-brand-foreground" aria-hidden="true">
                {cta} <ArrowRightIcon className="size-4" />
              </span>
            </Card>
          ))}
        </div>
      </section>

      <section className="flex flex-col gap-4" aria-labelledby="metrics">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h2 id="metrics" className="text-xs font-medium tracking-wider text-muted-foreground uppercase">
            Últimos 30 días
          </h2>
          <Link to="/admin/reports" className="flex items-center gap-1 text-sm font-medium text-brand-foreground hover:underline">
            Ver reportes <ArrowRightIcon className="size-4" aria-hidden="true" />
          </Link>
        </div>
        {/* Sin `value` mientras carga, StatCard se pinta como placeholder. */}
        <div className="grid gap-4 md:grid-cols-3">
          <StatCard
            label="Estudiantes activos"
            value={report && formatNumber(report.kpis.activeStudents)}
            hint="Iniciaron sesión en los últimos 30 días."
            icon={UsersIcon}
          />
          <StatCard
            label="Cursos completados"
            value={report && formatNumber(report.kpis.completions)}
            hint="Terminados en los últimos 30 días."
            icon={CheckCircleIcon}
          />
          <StatCard
            label="Tasa de completación"
            value={report && formatPercent(report.kpis.completionRate)}
            hint="De las inscripciones de los últimos 30 días."
            icon={BarChartIcon}
          />
        </div>
      </section>
    </div>
  );
}
