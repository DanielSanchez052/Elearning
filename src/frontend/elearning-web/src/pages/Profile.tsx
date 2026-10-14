import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { useMyEnrollments } from '@/hooks/useEnrollments';
import { useDownloadCertificate, useMyCertificates } from '@/hooks/useCertificates';
import { useMyBadges } from '@/hooks/useBadges';
import { Avatar, Badge, Button, Card, Skeleton, StateBlock } from '@/components/ui';
import { AwardIcon, CheckCircleIcon, DownloadIcon, GraduationCapIcon, MailIcon } from '@/components/ui/icons';
import { ButtonLink } from '@/components/ButtonLink';
import { badgeIcon } from '@/components/gamification/badgeIcon';
import { enrollmentStatusKey } from '@/lib/enrollment';
import { formatDate } from '@/lib/format';
import { roleTone } from '@/lib/roles';
import { ROLES } from '@/types/user.types';

/** Estados de carga y error de una sección del perfil. */
function SectionState({ isLoading, isError, onRetry }: { isLoading: boolean; isError: boolean; onRetry: () => void }) {
  if (isLoading) return <Skeleton className="mt-4 h-24 rounded-xl" />;
  if (isError)
    return (
      <div className="mt-3 flex flex-wrap items-center gap-3">
        <p className="text-sm text-muted-foreground">No pudimos cargar esta sección.</p>
        <Button variant="secondary" size="sm" onClick={onRetry}>
          Reintentar
        </Button>
      </div>
    );
  return null;
}

export const ProfilePage = () => {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();
  const enrollmentsQuery = useMyEnrollments();
  const certificatesQuery = useMyCertificates();
  const badgesQuery = useMyBadges();
  const certificateDownload = useDownloadCertificate();

  if (!user) return null;

  const completed = (enrollmentsQuery.data ?? []).filter((e) => enrollmentStatusKey(e.status) === 'completed');
  const badges = badgesQuery.data ?? [];
  const certificates = certificatesQuery.data ?? [];

  return (
    <main className="mx-auto flex w-full max-w-7xl flex-col gap-4 px-4 py-6 md:gap-6 md:px-6 md:py-8">
      <Card>
        <div className="flex flex-wrap items-center gap-4 md:gap-5">
          <Avatar name={user.fullName} size="lg" />
          <div className="min-w-0 flex-1">
            <h1 className="text-xl font-semibold text-foreground md:text-2xl">{user.fullName}</h1>
            <p className="mt-0.5 flex min-w-0 flex-wrap items-center gap-x-1.5 text-sm text-muted-foreground">
              <MailIcon className="size-4 shrink-0" aria-hidden="true" /> <span className="truncate">{user.email}</span>
              {user.country && <span className="text-subtle-foreground">· {user.country}</span>}
            </p>
          </div>
          <Badge tone={roleTone[user.role]}>{ROLES[user.role] ?? user.role}</Badge>
        </div>
      </Card>

      <div className="grid gap-4 md:grid-cols-3 md:gap-6">
        <Card className="min-w-0 md:col-span-2">
          <h2 className="text-lg font-semibold text-foreground">Cursos completados</h2>
          <SectionState isLoading={enrollmentsQuery.isLoading} isError={enrollmentsQuery.isError} onRetry={() => enrollmentsQuery.refetch()} />
          {enrollmentsQuery.isSuccess &&
            (completed.length === 0 ? (
              <StateBlock
                className="mt-4"
                compact
                icon={GraduationCapIcon}
                title="Aún no completas ningún curso"
                text="Cuando completes las lecciones requeridas de un curso, aparecerá aquí."
                action="Explorar cursos"
                onAction={() => navigate('/courses')}
              />
            ) : (
              <ul className="mt-3 divide-y divide-border">
                {completed.map((e) => (
                  <li key={e.enrollmentId} className="flex items-center gap-4 py-3">
                    <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-success/10 text-success-foreground">
                      <CheckCircleIcon className="size-5" aria-hidden="true" />
                    </span>
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-foreground">{e.courseTitle}</p>
                      <p className="text-xs text-subtle-foreground">
                        {e.completedAt ? `Completado el ${formatDate(e.completedAt)} · ` : ''}
                        {e.completedLessons} / {e.requiredLessons} lecciones
                      </p>
                    </div>
                    <ButtonLink to={`/courses/${e.courseId}`} variant="ghost" size="sm">
                      Ver curso
                    </ButtonLink>
                  </li>
                ))}
              </ul>
            ))}
        </Card>

        <Card>
          <h2 className="text-lg font-semibold text-foreground">Medallas</h2>
          <SectionState isLoading={badgesQuery.isLoading} isError={badgesQuery.isError} onRetry={() => badgesQuery.refetch()} />
          {badgesQuery.isSuccess &&
            (badges.length === 0 ? (
              <p className="mt-2 text-sm text-muted-foreground">
                Aún no tienes medallas. Inicia sesión, completa un curso o aprueba un examen final al primer intento para ganarlas.
              </p>
            ) : (
              <ul className="mt-4 space-y-4">
                {badges.map((badge) => {
                  const Icon = badgeIcon(badge.code);
                  return (
                    <li key={badge.id} className="flex items-start gap-3">
                      <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-warning/10 text-warning-foreground">
                        <Icon className="size-5" aria-hidden="true" />
                      </span>
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-foreground">{badge.name}</p>
                        {badge.description && <p className="text-xs text-muted-foreground">{badge.description}</p>}
                        {badge.courseTitle && <p className="text-xs text-muted-foreground">{badge.courseTitle}</p>}
                        <p className="mt-0.5 text-xs text-subtle-foreground">{formatDate(badge.obtainedAt)}</p>
                      </div>
                    </li>
                  );
                })}
              </ul>
            ))}
        </Card>
      </div>

      <Card>
        <h2 className="text-lg font-semibold text-foreground">Certificados</h2>
        <SectionState isLoading={certificatesQuery.isLoading} isError={certificatesQuery.isError} onRetry={() => certificatesQuery.refetch()} />
        {certificatesQuery.isSuccess &&
          (certificates.length === 0 ? (
            <p className="mt-2 text-sm text-muted-foreground">Aprueba el examen final de un curso para obtener tu primer certificado.</p>
          ) : (
            <ul className="mt-4 grid gap-3 md:grid-cols-2">
              {certificates.map((certificate) => (
                <li key={certificate.id}>
                  <Card variant="subtle" padding="sm">
                    <div className="flex flex-wrap items-center gap-3">
                      <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-success/10 text-success-foreground">
                        <AwardIcon className="size-5" aria-hidden="true" />
                      </span>
                      <div className="min-w-0 flex-1">
                        <p className="text-sm font-medium text-foreground">{certificate.courseName}</p>
                        <p className="text-xs text-subtle-foreground">Completado el {formatDate(certificate.completedAt)}</p>
                      </div>
                      <Button
                        variant="outline"
                        size="sm"
                        className="w-full sm:w-auto"
                        onClick={() => certificateDownload.download(certificate)}
                        disabled={certificateDownload.downloadingId === certificate.id}
                      >
                        <DownloadIcon /> {certificateDownload.downloadingId === certificate.id ? 'Descargando…' : 'Descargar PDF'}
                      </Button>
                    </div>
                  </Card>
                </li>
              ))}
            </ul>
          ))}
        {certificateDownload.error && (
          <p className="mt-3 text-xs text-destructive-foreground" role="alert">{certificateDownload.error}</p>
        )}
      </Card>
    </main>
  );
};
