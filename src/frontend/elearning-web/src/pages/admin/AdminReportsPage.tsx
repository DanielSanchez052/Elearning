import { useState } from 'react';
import { useOverviewReport } from '@/hooks/admin/reports';
import { useAdminCountries } from '@/hooks/admin/countries';
import { useAuthStore } from '@/store/authStore';
import {
  Card,
  ProgressBar,
  SegmentedTabs,
  Select,
  Skeleton,
  StatCard,
  StateBlock,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui';
import { BarChartIcon, BookOpenIcon, CheckCircleIcon, ClockIcon, GraduationCapIcon, UsersIcon } from '@/components/ui/icons';
import { formatDays, formatNumber, formatPercent } from '@/lib/format';
import { reportPeriodOptions } from '@/lib/reports';
import type { ReportKpis, ReportPeriod } from '@/types/report.types';

export default function AdminReportsPage() {
  const user = useAuthStore((s) => s.user);
  const isSuperAdmin = user?.role === 'superadmin';

  const [period, setPeriod] = useState<ReportPeriod>('30d');
  const [countryId, setCountryId] = useState<number | undefined>();
  const { data: countries } = useAdminCountries({ enabled: isSuperAdmin });
  const { data: report, isLoading, isError, refetch, isPlaceholderData } = useOverviewReport({ period, countryId });

  const scope = report?.countryName ?? (isSuperAdmin ? 'Toda la plataforma' : user?.country);

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Reportes</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          {scope} · Solo cuentan estudiantes.
        </p>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <SegmentedTabs aria-label="Periodo" options={reportPeriodOptions} value={period} onChange={setPeriod} />
        {isSuperAdmin && countries && (
          <Select
            aria-label="País"
            className="w-full sm:w-48"
            value={countryId ?? ''}
            onChange={(e) => setCountryId(e.target.value ? Number(e.target.value) : undefined)}
          >
            <option value="">Todos los países</option>
            {countries.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
        )}
      </div>

      {isError ? (
        <StateBlock tipo="error" title="No pudimos cargar el reporte" onAction={() => refetch()} />
      ) : isLoading || !report ? (
        <ReportSkeleton />
      ) : (
        <div className={isPlaceholderData ? 'flex flex-col gap-6 opacity-60' : 'flex flex-col gap-6'} aria-busy={isPlaceholderData || undefined}>
          <KpiGrid kpis={report.kpis} />

          <Card padding="none" className="overflow-hidden">
            <div className="border-b border-border px-6 py-4">
              <h2 className="text-base font-semibold text-foreground">Cursos más populares</h2>
              <p className="text-xs text-muted-foreground">Por inscripciones del periodo, con la parte de ellas ya completada.</p>
            </div>
            {report.popularCourses.length === 0 ? (
              <StateBlock bordered={false} compact icon={BookOpenIcon} title="Sin inscripciones en este periodo" />
            ) : (
              <Table>
                <TableHead>
                  <TableRow>
                    <TableHeader>Curso</TableHeader>
                    <TableHeader className="text-right">Inscripciones</TableHeader>
                    <TableHeader className="text-right">Completadas</TableHeader>
                    <TableHeader className="w-56">Tasa de completación</TableHeader>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {report.popularCourses.map((c) => (
                    <TableRow key={c.courseId}>
                      <TableCell className="py-3 font-medium text-foreground">{c.title}</TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatNumber(c.enrollments)}</TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatNumber(c.enrollmentsCompleted)}</TableCell>
                      <TableCell className="py-3">
                        <RateBar value={c.completionRate} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </Card>

          {report.countries && (
            <Card padding="none" className="overflow-hidden">
              <div className="border-b border-border px-6 py-4">
                <h2 className="text-base font-semibold text-foreground">Comparativa por país</h2>
                <p className="text-xs text-muted-foreground">Las mismas métricas para cada país activo.</p>
              </div>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableHeader>País</TableHeader>
                    <TableHeader className="text-right">Estudiantes</TableHeader>
                    <TableHeader className="text-right">Activos</TableHeader>
                    <TableHeader className="text-right">Inscripciones</TableHeader>
                    <TableHeader className="text-right">Completados</TableHeader>
                    <TableHeader className="w-48">Tasa de completación</TableHeader>
                    <TableHeader className="text-right">Días promedio</TableHeader>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {report.countries.map((c) => (
                    <TableRow key={c.countryId}>
                      <TableCell className="py-3 font-medium text-foreground">{c.countryName}</TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatNumber(c.kpis.students)}</TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatNumber(c.kpis.activeStudents)}</TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatNumber(c.kpis.enrollments)}</TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatNumber(c.kpis.completions)}</TableCell>
                      <TableCell className="py-3">
                        <RateBar value={c.kpis.completionRate} />
                      </TableCell>
                      <TableCell className="py-3 text-right tabular-nums">{formatDays(c.kpis.avgDaysToComplete)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Card>
          )}
        </div>
      )}
    </div>
  );
}

function KpiGrid({ kpis }: { kpis: ReportKpis }) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <StatCard label="Estudiantes" value={formatNumber(kpis.students)} hint="Registrados en total." icon={GraduationCapIcon} />
      <StatCard label="Estudiantes activos" value={formatNumber(kpis.activeStudents)} hint="Iniciaron sesión en el periodo." icon={UsersIcon} />
      <StatCard label="Inscripciones" value={formatNumber(kpis.enrollments)} hint="Hechas en el periodo." icon={BookOpenIcon} />
      <StatCard label="Cursos completados" value={formatNumber(kpis.completions)} hint="Terminados en el periodo." icon={CheckCircleIcon} />
      <StatCard
        label="Tasa de completación"
        value={formatPercent(kpis.completionRate)}
        hint="De las inscripciones del periodo, cuántas ya se completaron."
        icon={BarChartIcon}
      />
      <StatCard
        label="Tiempo para completar"
        value={formatDays(kpis.avgDaysToComplete)}
        hint="Promedio desde la inscripción, en los cursos completados del periodo."
        icon={ClockIcon}
      />
    </div>
  );
}

// La barra es decorativa: el valor exacto (con decimal) va como texto al lado, que es lo que se lee.
function RateBar({ value }: { value: number | null }) {
  if (value == null) return <span className="text-subtle-foreground">—</span>;
  return (
    <div className="flex items-center gap-3">
      <div className="flex-1" aria-hidden="true">
        <ProgressBar value={value} size="sm" />
      </div>
      <span className="w-14 text-right text-xs font-medium tabular-nums text-foreground-soft">{formatPercent(value)}</span>
    </div>
  );
}

function ReportSkeleton() {
  return (
    <div className="flex flex-col gap-6" aria-busy="true">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }, (_, i) => (
          <Skeleton key={i} className="h-28 rounded-2xl" />
        ))}
      </div>
      <Skeleton className="h-56 rounded-2xl" />
    </div>
  );
}
