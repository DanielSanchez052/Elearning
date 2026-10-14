import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { useMyEnrollments } from '@/hooks/useEnrollments';
import { useMyBadges } from '@/hooks/useBadges';
import { useDownloadCertificate, useMyCertificates } from '@/hooks/useCertificates';
import { useNotifications } from '@/hooks/useNotifications';
import { Badge, Button, Card, ProgressBar, SegmentedTabs, Skeleton, StateBlock } from '@/components/ui';
import { AwardIcon, ClockIcon, DownloadIcon, GraduationCapIcon, PlayCircleIcon } from '@/components/ui/icons';
import { ButtonLink } from '@/components/ButtonLink';
import { CourseThumbnail } from '@/components/courses/CourseThumbnail';
import { badgeIcon } from '@/components/gamification/badgeIcon';
import { enrollmentStatusKey, enrollmentStatusMeta, requiredLessonsLabel, type EnrollmentStatusKey } from '@/lib/enrollment';
import { firstName, formatDate, formatLongDay } from '@/lib/format';
import type { EnrollmentSummaryDto } from '@/types/enrollment.types';

type Tab = 'todos' | EnrollmentStatusKey;

const tabLabels: { value: Tab; label: string }[] = [
  { value: 'todos', label: 'Todos' },
  { value: 'active', label: 'Activos' },
  { value: 'completed', label: 'Completados' },
  { value: 'abandoned', label: 'Abandonados' },
];

const byEnrolledDesc = (a: EnrollmentSummaryDto, b: EnrollmentSummaryDto) =>
  new Date(b.enrolledAt).getTime() - new Date(a.enrolledAt).getTime();

function DashboardSkeleton() {
  return (
    <div className="flex flex-col gap-8" aria-busy="true">
      <div className="space-y-2">
        <Skeleton className="h-4 w-48" />
        <Skeleton className="h-8 w-64" />
      </div>
      <Skeleton className="h-48 rounded-2xl" />
      <div className="grid gap-6 md:grid-cols-3">
        <Skeleton className="h-64 rounded-2xl md:col-span-2" />
        <div className="space-y-6">
          <Skeleton className="h-48 rounded-2xl" />
          <Skeleton className="h-40 rounded-2xl" />
        </div>
      </div>
    </div>
  );
}

function EnrollmentRow({ enrollment }: { enrollment: EnrollmentSummaryDto }) {
  const status = enrollmentStatusKey(enrollment.status);
  const meta = enrollmentStatusMeta[status];
  const pct = Math.round(enrollment.progressPercent);

  return (
    <li className="flex items-center gap-4 py-4">
      <CourseThumbnail
        src={enrollment.courseThumbnailUrl}
        title={enrollment.courseTitle}
        className="hidden w-28 shrink-0 rounded-lg sm:block"
      />
      <div className="min-w-0 flex-1 space-y-2">
        <div className="flex flex-wrap items-center gap-2">
          <Link to={`/courses/${enrollment.courseId}`} className="truncate text-sm font-semibold text-foreground hover:text-brand-foreground">
            {enrollment.courseTitle}
          </Link>
          <Badge tone={meta.tone}>{meta.label}</Badge>
        </div>
        <p className="text-xs text-subtle-foreground">
          Inscrito el {formatDate(enrollment.enrolledAt)}
          {enrollment.deadlineAt && ` · Fecha límite: ${formatDate(enrollment.deadlineAt)}`}
        </p>
        <ProgressBar
          value={pct}
          size="sm"
          showValue
          tone={status === 'abandoned' ? 'warning' : 'success'}
          label={requiredLessonsLabel(enrollment.completedLessons, enrollment.requiredLessons)}
        />
      </div>
      <ButtonLink to={`/courses/${enrollment.courseId}`} variant="secondary" size="sm" className="hidden sm:inline-flex">
        {meta.cta}
      </ButtonLink>
    </li>
  );
}

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);
  const enrollmentsQuery = useMyEnrollments();
  const badgesQuery = useMyBadges();
  const certificatesQuery = useMyCertificates();
  const notificationsQuery = useNotifications();
  const certificateDownload = useDownloadCertificate();
  const [tab, setTab] = useState<Tab>('todos');
  const navigate = useNavigate();
  // Instante de referencia para las fechas límite, fijo durante la vida de la vista.
  const [now] = useState(() => Date.now());

  const enrollments = useMemo(() => [...(enrollmentsQuery.data ?? [])].sort(byEnrolledDesc), [enrollmentsQuery.data]);
  const active = enrollments.filter((e) => enrollmentStatusKey(e.status) === 'active');
  const current = active[0];
  const nextDeadline = active
    .map((e) => e.deadlineAt)
    .filter((d): d is string => !!d && new Date(d).getTime() > now)
    .sort()[0];

  const visible = tab === 'todos' ? enrollments : enrollments.filter((e) => enrollmentStatusKey(e.status) === tab);
  const tabs = tabLabels.map((t) => ({
    ...t,
    count: t.value === 'todos' ? enrollments.length : enrollments.filter((e) => enrollmentStatusKey(e.status) === t.value).length,
  }));

  const badges = badgesQuery.data ?? [];
  const certificates = certificatesQuery.data ?? [];
  const notifications = (notificationsQuery.data ?? []).slice(0, 3);
  const isEmpty = enrollments.length === 0;

  const subtitle = isEmpty
    ? 'Bienvenido a ELearning. Empieza inscribiéndote en tu primer curso.'
    : active.length === 0
      ? 'No tienes cursos activos en este momento.'
      : `Tienes ${active.length} ${active.length === 1 ? 'curso activo' : 'cursos activos'}.` +
        (nextDeadline ? ` Tu próxima fecha límite es el ${formatDate(nextDeadline)}.` : '');

  return (
    <main className="mx-auto flex w-full max-w-7xl flex-col gap-8 px-4 py-8 sm:px-6">
      {enrollmentsQuery.isLoading ? (
        <DashboardSkeleton />
      ) : enrollmentsQuery.isError ? (
        <>
          <h1 className="text-3xl font-semibold text-foreground">Hola, {firstName(user?.fullName)}</h1>
          <StateBlock
            tipo="error"
            title="No pudimos cargar tu dashboard"
            text="Revisa tu conexión e inténtalo de nuevo. Tus cursos y tu progreso están a salvo."
            onAction={() => enrollmentsQuery.refetch()}
          />
        </>
      ) : (
        <>
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{formatLongDay()}</p>
            <h1 className="text-3xl font-semibold text-foreground">Hola, {firstName(user?.fullName)}</h1>
            <p className="text-sm text-muted-foreground">{subtitle}</p>
          </div>

          {current ? (
            <Card padding="none" className="overflow-hidden">
              <div className="grid md:grid-cols-3">
                <CourseThumbnail src={current.courseThumbnailUrl} title={current.courseTitle} showTitleOnFallback className="h-full min-h-48" />
                <div className="flex flex-col gap-4 p-6 md:col-span-2">
                  <div className="space-y-1">
                    <p className="text-xs font-medium tracking-wider text-brand-foreground uppercase">Continuar aprendiendo</p>
                    <h2 className="text-xl font-semibold text-foreground">{current.courseTitle}</h2>
                  </div>
                  <ProgressBar
                    value={Math.round(current.progressPercent)}
                    showValue
                    tone="success"
                    label={requiredLessonsLabel(current.completedLessons, current.requiredLessons)}
                  />
                  <div className="flex flex-wrap items-center gap-3">
                    <ButtonLink to={`/courses/${current.courseId}`}>
                      <PlayCircleIcon /> Continuar curso
                    </ButtonLink>
                    <div className="flex-1" />
                    {current.deadlineAt && (
                      <span className="flex items-center gap-1.5 text-xs text-subtle-foreground">
                        <ClockIcon className="size-3" aria-hidden="true" /> Fecha límite: {formatDate(current.deadlineAt)}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </Card>
          ) : isEmpty ? (
            <StateBlock
              icon={GraduationCapIcon}
              title="Aún no estás inscrito en ningún curso"
              text="Explora el catálogo e inscríbete para empezar a aprender. Tu progreso aparecerá aquí."
              action="Explorar cursos"
              onAction={() => navigate('/courses')}
            />
          ) : null}

          <div className="grid gap-6 md:grid-cols-3">
            <Card className="min-w-0 md:col-span-2">
              <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h2 className="text-lg font-semibold text-foreground">Mis cursos</h2>
                  <p className="text-sm text-muted-foreground">
                    {enrollments.length} {enrollments.length === 1 ? 'inscripción' : 'inscripciones'}
                  </p>
                </div>
                {!isEmpty && <SegmentedTabs aria-label="Filtrar por estado" options={tabs} value={tab} onChange={setTab} />}
              </div>
              {isEmpty ? (
                <>
                  <p className="text-sm text-muted-foreground">
                    Cuando te inscribas, verás aquí el estado, el progreso y las fechas de cada curso.
                  </p>
                  <div className="mt-6 space-y-3" aria-hidden="true">
                    <div className="h-14 rounded-xl border border-dashed border-border" />
                    <div className="h-14 rounded-xl border border-dashed border-border" />
                  </div>
                </>
              ) : visible.length === 0 ? (
                <p className="py-10 text-center text-sm text-muted-foreground">No tienes cursos en este estado.</p>
              ) : (
                <ul className="divide-y divide-border">
                  {visible.map((e) => (
                    <EnrollmentRow key={e.enrollmentId} enrollment={e} />
                  ))}
                </ul>
              )}
            </Card>

            <div className="flex min-w-0 flex-col gap-6">
              <Card>
                <div className="flex items-center justify-between gap-2">
                  <h2 className="text-lg font-semibold text-foreground">Medallas</h2>
                  {badges.length > 3 && (
                    <ButtonLink to="/profile" variant="ghost" size="sm">
                      Ver todas
                    </ButtonLink>
                  )}
                </div>
                {badgesQuery.isLoading ? (
                  <Skeleton className="mt-4 h-24 rounded-xl" />
                ) : badgesQuery.isError ? (
                  <p className="mt-2 text-sm text-muted-foreground">No pudimos cargar tus medallas.</p>
                ) : badges.length === 0 ? (
                  <p className="mt-2 text-sm text-muted-foreground">Completa lecciones y exámenes para ganar tu primera medalla.</p>
                ) : (
                  <>
                    <p className="text-sm text-muted-foreground">
                      {badges.length} {badges.length === 1 ? 'obtenida' : 'obtenidas'}
                    </p>
                    <ul className="mt-4 space-y-3">
                      {badges.slice(0, 3).map((badge) => {
                        const Icon = badgeIcon(badge.code);
                        return (
                          <li key={badge.id} className="flex items-start gap-3">
                            <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-warning/10 text-warning-foreground">
                              <Icon className="size-5" aria-hidden="true" />
                            </span>
                            <div className="min-w-0">
                              <p className="text-sm font-medium text-foreground">{badge.name}</p>
                              {badge.description && <p className="text-xs text-muted-foreground">{badge.description}</p>}
                              <p className="mt-0.5 text-xs text-subtle-foreground">Obtenida el {formatDate(badge.obtainedAt)}</p>
                            </div>
                          </li>
                        );
                      })}
                    </ul>
                  </>
                )}
              </Card>

              <Card>
                <h2 className="text-lg font-semibold text-foreground">Certificados</h2>
                {certificatesQuery.isLoading ? (
                  <Skeleton className="mt-4 h-14 rounded-xl" />
                ) : certificatesQuery.isError ? (
                  <p className="mt-2 text-sm text-muted-foreground">No pudimos cargar tus certificados.</p>
                ) : certificates.length === 0 ? (
                  <p className="mt-2 text-sm text-muted-foreground">
                    Completa un curso y aprueba su examen final para obtener tu primer certificado.
                  </p>
                ) : (
                  <ul className="mt-4 space-y-3">
                    {certificates.map((certificate) => (
                      <li key={certificate.id} className="flex items-center gap-3">
                        <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-success/10 text-success-foreground">
                          <AwardIcon className="size-5" aria-hidden="true" />
                        </span>
                        <div className="min-w-0 flex-1">
                          <p className="truncate text-sm font-medium text-foreground">{certificate.courseName}</p>
                          <p className="text-xs text-subtle-foreground">Completado el {formatDate(certificate.completedAt)}</p>
                        </div>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => certificateDownload.download(certificate)}
                          disabled={certificateDownload.downloadingId === certificate.id}
                          aria-label={`PDF: descargar el certificado de ${certificate.courseName}`}
                        >
                          <DownloadIcon /> PDF
                        </Button>
                      </li>
                    ))}
                  </ul>
                )}
                {certificateDownload.error && (
                  <p className="mt-3 text-xs text-destructive-foreground" role="alert">{certificateDownload.error}</p>
                )}
              </Card>

              <Card>
                <h2 className="text-lg font-semibold text-foreground">Notificaciones</h2>
                {notificationsQuery.isLoading ? (
                  <Skeleton className="mt-4 h-20 rounded-xl" />
                ) : notifications.length === 0 ? (
                  <p className="mt-2 text-sm text-muted-foreground">No tienes notificaciones recientes.</p>
                ) : (
                  <ul className="mt-3 divide-y divide-border">
                    {notifications.map((n) => (
                      <li key={n.id} className="flex gap-3 py-3">
                        <span
                          aria-hidden="true"
                          className={`mt-1.5 block size-1.5 shrink-0 rounded-full ${n.isRead ? 'bg-transparent' : 'bg-primary'}`}
                        />
                        <div className="min-w-0">
                          <p className="text-sm text-foreground">{n.title}</p>
                          <p className="text-xs text-subtle-foreground">{formatDate(n.createdAt)}</p>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </Card>
            </div>
          </div>
        </>
      )}
    </main>
  );
}
