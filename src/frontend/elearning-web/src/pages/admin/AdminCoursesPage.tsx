import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAdminCourses, useToggleCourseStatus, useDeleteCourse } from '@/hooks/admin/courses';
import { useAdminCountries } from '@/hooks/admin/countries';
import { useAuthStore } from '../../store/authStore';
import {
  Alert,
  Badge,
  Button,
  Card,
  Modal,
  Pagination,
  SearchInput,
  SegmentedTabs,
  Select,
  Skeleton,
  StateBlock,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui';
import { BookOpenIcon, EyeIcon, GlobeIcon, ListChecksIcon, PencilIcon, PlusIcon, ShieldIcon, TrashIcon } from '@/components/ui/icons';
import { ButtonLink } from '@/components/ButtonLink';
import { CourseThumbnail } from '@/components/courses/CourseThumbnail';
import { getApiErrorMessage } from '../../lib/axios';
import { canManageListedCourse } from '@/lib/courseAccess';
import type { CourseSummaryDto } from '../../types/course.types';

const PAGE_SIZE = 20;

type StatusTab = 'todos' | 'publicados' | 'borradores';

const statusTabs: { value: StatusTab; label: string }[] = [
  { value: 'todos', label: 'Todos' },
  { value: 'publicados', label: 'Publicados' },
  { value: 'borradores', label: 'Borradores' },
];

export default function AdminCoursesPage() {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();
  const isSuperAdmin = user?.role === 'superadmin';
  const isInstructor = user?.role === 'instructor';

  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [status, setStatus] = useState<StatusTab>('todos');
  const [countryId, setCountryId] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [toDelete, setToDelete] = useState<CourseSummaryDto | null>(null);

  // Busca mientras se escribe, con una pausa corta.
  useEffect(() => {
    const t = setTimeout(() => {
      setQuery(search.trim());
      setPage(1);
    }, 300);
    return () => clearTimeout(t);
  }, [search]);

  const { data, isLoading, isError, refetch, isPlaceholderData } = useAdminCourses({
    search: query || undefined,
    isActive: status === 'todos' ? undefined : status === 'publicados',
    countryId,
    page,
    pageSize: PAGE_SIZE,
  });

  // El filtro por país solo lo usa el superadmin; el resto ya ve únicamente los cursos de su alcance.
  const { data: countries } = useAdminCountries({ enabled: isSuperAdmin });
  const toggle = useToggleCourseStatus();
  const [rowError, setRowError] = useState('');

  const courses = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const hasFilters = Boolean(query || status !== 'todos' || countryId);

  const resetFilters = () => {
    setSearch('');
    setQuery('');
    setStatus('todos');
    setCountryId(undefined);
    setPage(1);
  };

  const handleToggle = async (course: CourseSummaryDto) => {
    setRowError('');
    try {
      await toggle.mutateAsync(course.id);
    } catch (e) {
      setRowError(getApiErrorMessage(e));
    }
  };

  const scopeText = isInstructor
    ? 'Tus cursos como instructor'
    : `${data ? `${total} ${total === 1 ? 'curso' : 'cursos'}` : 'Cursos'}${isSuperAdmin ? ' en total' : ' en tu país'}`;

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Cursos</h1>
          <p className="mt-1 text-sm text-muted-foreground">{scopeText}</p>
        </div>
        <ButtonLink to="/admin/courses/new">
          <PlusIcon /> Nuevo curso
        </ButtonLink>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <SearchInput
          className="min-w-64 flex-1"
          aria-label="Buscar por título"
          placeholder="Buscar por título"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <SegmentedTabs
          aria-label="Estado de publicación"
          options={statusTabs}
          value={status}
          onChange={(v) => {
            setStatus(v);
            setPage(1);
          }}
        />
        {isSuperAdmin && countries && (
          <Select
            aria-label="País"
            className="w-full sm:w-48"
            value={countryId ?? ''}
            onChange={(e) => {
              setCountryId(e.target.value ? Number(e.target.value) : undefined);
              setPage(1);
            }}
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

      {rowError && <Alert tone="danger">{rowError}</Alert>}

      <Card padding="none" className="overflow-hidden">
        {isError ? (
          <StateBlock bordered={false} tipo="error" title="No pudimos cargar los cursos" onAction={() => refetch()} />
        ) : !isLoading && courses.length === 0 ? (
          hasFilters ? (
            <StateBlock
              bordered={false}
              icon={BookOpenIcon}
              title="Ningún curso coincide con los filtros"
              text="Prueba con otro título o quita algún filtro."
              action="Limpiar filtros"
              actionVariant="secondary"
              onAction={resetFilters}
            />
          ) : (
            <StateBlock
              bordered={false}
              icon={BookOpenIcon}
              title="Todavía no hay cursos"
              text="Crea el primer curso, agrega sus lecciones y publícalo cuando esté listo."
              action="Nuevo curso"
              onAction={() => navigate('/admin/courses/new')}
            />
          )
        ) : (
          <>
            <Table className={isPlaceholderData ? 'opacity-60' : undefined} aria-busy={isLoading || undefined}>
              <TableHead>
                <TableRow>
                  <TableHeader>Curso</TableHeader>
                  <TableHeader>Instructor</TableHeader>
                  <TableHeader>Lecciones</TableHeader>
                  <TableHeader>Estado</TableHeader>
                  <TableHeader>Alcance</TableHeader>
                  <TableHeader className="text-right">
                    <span className="sr-only">Acciones</span>
                  </TableHeader>
                </TableRow>
              </TableHead>
              <TableBody>
                {isLoading
                  ? Array.from({ length: 6 }, (_, i) => (
                      <TableRow key={i}>
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <Skeleton className="h-9 w-16" />
                            <Skeleton className="h-3 w-48" />
                          </div>
                        </TableCell>
                        <TableCell><Skeleton className="h-3 w-24" /></TableCell>
                        <TableCell><Skeleton className="h-3 w-8" /></TableCell>
                        <TableCell><Skeleton className="h-5 w-20 rounded-full" /></TableCell>
                        <TableCell><Skeleton className="h-5 w-20 rounded-full" /></TableCell>
                        <TableCell />
                      </TableRow>
                    ))
                  : courses.map((course) => (
                      <TableRow key={course.id} className="hover:bg-secondary">
                        <TableCell className="py-3">
                          <div className="flex items-center gap-3">
                            <CourseThumbnail src={course.thumbnailUrl} title={course.title} className="w-16 shrink-0 rounded-md" />
                            <p className="line-clamp-2 min-w-40 font-medium text-foreground">{course.title}</p>
                          </div>
                        </TableCell>
                        <TableCell className="py-3 whitespace-nowrap">{course.instructorName}</TableCell>
                        <TableCell className="py-3 tabular-nums">{course.lessonCount}</TableCell>
                        <TableCell className="py-3">
                          <Badge tone={course.isActive ? 'success' : 'neutral'}>{course.isActive ? 'Publicado' : 'Borrador'}</Badge>
                        </TableCell>
                        <TableCell className="py-3">
                          {course.isGlobal ? (
                            <Badge tone="brand"><GlobeIcon /> Global</Badge>
                          ) : (
                            <Badge><ShieldIcon /> Por país</Badge>
                          )}
                        </TableCell>
                        <TableCell className="py-3">
                          <div className="flex items-center justify-end gap-1">
                            {canManageListedCourse(user, course) && (
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleToggle(course)}
                                disabled={toggle.isPending && toggle.variables === course.id}
                              >
                                {course.isActive ? 'Despublicar' : 'Publicar'}
                              </Button>
                            )}
                            <ButtonLink to={`/courses/${course.id}`} variant="ghost" size="icon" aria-label={`Ver ${course.title}`} title="Ver como estudiante">
                              <EyeIcon />
                            </ButtonLink>
                            {canManageListedCourse(user, course) ? (
                              <>
                                <ButtonLink to={`/admin/courses/${course.id}/edit`} variant="ghost" size="icon" aria-label={`Editar ${course.title}`} title="Editar">
                                  <PencilIcon />
                                </ButtonLink>
                                <ButtonLink to={`/admin/courses/${course.id}/quizzes`} variant="ghost" size="icon" aria-label={`Examen final de ${course.title}`} title="Examen final">
                                  <ListChecksIcon />
                                </ButtonLink>
                                <Button variant="danger" size="icon" aria-label={`Eliminar ${course.title}`} title="Eliminar" onClick={() => setToDelete(course)}>
                                  <TrashIcon />
                                </Button>
                              </>
                            ) : (
                              <span className="px-2 text-xs text-subtle-foreground" title="Los cursos globales solo los gestiona el superadmin">
                                Solo lectura
                              </span>
                            )}
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
              </TableBody>
            </Table>
            {data && (
              <Pagination
                className="border-t border-border px-6 py-3"
                page={page}
                totalPages={data.totalPages}
                onPageChange={setPage}
                summary={`Mostrando ${(page - 1) * PAGE_SIZE + 1}–${Math.min(page * PAGE_SIZE, total)} de ${total}`}
              />
            )}
          </>
        )}
      </Card>

      <DeleteCourseModal course={toDelete} onClose={() => setToDelete(null)} />
    </div>
  );
}

// ── Delete confirmation ───────────────────────────────────────────────────────

function DeleteCourseModal({ course, onClose }: { course: CourseSummaryDto | null; onClose: () => void }) {
  const destroy = useDeleteCourse();
  const [error, setError] = useState('');

  const close = () => {
    setError('');
    onClose();
  };

  const handleDelete = async () => {
    if (!course) return;
    setError('');
    try {
      await destroy.mutateAsync(course.id);
      close();
    } catch (e) {
      setError(getApiErrorMessage(e));
    }
  };

  return (
    <Modal
      open={!!course}
      onClose={close}
      size="sm"
      title="Eliminar curso"
      description="Esta acción no se puede deshacer."
      footer={
        <>
          <Button variant="secondary" onClick={close}>
            Cancelar
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={destroy.isPending}>
            <TrashIcon /> {destroy.isPending ? 'Eliminando…' : 'Eliminar curso'}
          </Button>
        </>
      }
    >
      {course && (
        <div className="flex flex-col gap-3">
          <p className="text-sm text-foreground-soft">
            Se eliminará «{course.title}», sus {course.lessonCount} {course.lessonCount === 1 ? 'lección' : 'lecciones'}, sus quizzes y el progreso de los
            estudiantes inscritos.
          </p>
          {error && <Alert tone="danger">{error}</Alert>}
        </div>
      )}
    </Modal>
  );
}
