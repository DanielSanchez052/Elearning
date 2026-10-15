import { useMemo, useState, useCallback } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { Controller, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient } from '@tanstack/react-query';

// dnd-kit
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
  DragOverlay,
  type DragStartEvent,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
  arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { restrictToVerticalAxis, restrictToParentElement } from '@dnd-kit/modifiers';

import { courseKeys, useCourseDetail } from '@/hooks/useCourses';
import { useAdminCountries } from '@/hooks/admin/countries';
import {
  useCreateCourse,
  useUpdateCourse,
  useAssignCountries,
  useCreateLesson,
  useUpdateLesson,
  useDeleteLesson,
  useReorderLessons,
  useToggleCourseStatus,
} from '@/hooks/admin/courses';
import { coursesApi } from '@/api/admin/courses';
import {
  Alert,
  Badge,
  Button,
  Card,
  Checkbox,
  Drawer,
  Field,
  Input,
  Label,
  Modal,
  RadioCard,
  Skeleton,
  Spinner,
  StateBlock,
  Textarea,
} from '@/components/ui';
import { ArrowLeftIcon, ClipboardCheckIcon, GripVerticalIcon, ListChecksIcon, PencilIcon, PlusIcon, ShieldIcon, TrashIcon } from '@/components/ui/icons';
import { ButtonLink } from '@/components/ButtonLink';
import { LessonRow } from '@/components/courses/LessonRow';
import FileUploadField from '@/components/admin/FileUploadField';
import { getApiErrorMessage } from '../../lib/axios';
import { canManageCourse, canToggleCourseCountry, courseAccessDeniedReason, isSuperAdmin } from '@/lib/courseAccess';
import { useAuthStore } from '@/store/authStore';
import type { CourseDetailDto, LessonDto } from '../../types';
import type { CurrentUser } from '@/types/user.types';

// ── Schemas ───────────────────────────────────────────────────────────────────

const courseSchema = z.object({
  title: z.string().min(1, 'El título es requerido.').max(200, 'Máximo 200 caracteres.'),
  description: z.string().max(2000, 'Máximo 2000 caracteres.').optional(),
  isGlobal: z.boolean(),
});

const lessonSchema = z.object({
  title: z.string().min(1, 'El título es requerido.').max(200, 'Máximo 200 caracteres.'),
  type: z.enum(['video', 'pdf', 'quiz']),
  isRequired: z.boolean(),
});

type CourseFormData = z.infer<typeof courseSchema>;
type LessonFormData = z.infer<typeof lessonSchema>;

const sameIds = (a: number[], b: number[]) => a.length === b.length && [...a].sort().join() === [...b].sort().join();

type CountryOption = { id: number; name: string; code: string };

/** Para admin e instructor: los países que ya tiene el curso más el país propio (el único que pueden tocar). */
function ownScopeCountries(course: CourseDetailDto | undefined, user: CurrentUser | null): CountryOption[] {
  const options: CountryOption[] = course?.countries.map(({ id, name, code }) => ({ id, name, code })) ?? [];
  if (user && !options.some((c) => c.id === user.countryId)) {
    options.push({ id: user.countryId, name: user.country, code: '' });
  }
  return options;
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CourseFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEditing = !!id;
  const courseQuery = useCourseDetail(id ?? '');
  const user = useAuthStore((s) => s.user);

  if (isEditing && courseQuery.isLoading) return <CourseFormSkeleton />;

  if (isEditing && (courseQuery.isError || !courseQuery.data)) {
    return (
      <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
        <BackToCourses />
        <StateBlock tipo="error" title="No pudimos cargar el curso" onAction={() => courseQuery.refetch()} />
      </div>
    );
  }

  if (isEditing && courseQuery.data && !canManageCourse(user, courseQuery.data)) {
    return (
      <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
        <BackToCourses />
        <StateBlock
          icon={ShieldIcon}
          title="No puedes editar este curso"
          text={courseAccessDeniedReason(user, courseQuery.data)}
        />
      </div>
    );
  }

  // El editor se monta con el curso como estado inicial; al crear uno nuevo, la navegación a su
  // página de edición cambia la key y lo vuelve a montar con los datos del servidor.
  return <CourseEditor key={courseQuery.data?.id ?? 'new'} course={isEditing ? courseQuery.data : undefined} />;
}

function BackToCourses() {
  return (
    <Link to="/admin/courses" className="flex w-fit items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground">
      <ArrowLeftIcon className="size-4" aria-hidden="true" /> Cursos
    </Link>
  );
}

// ── Editor ────────────────────────────────────────────────────────────────────

function CourseEditor({ course }: { course?: CourseDetailDto }) {
  const isEditing = !!course;
  const id = course?.id ?? '';
  const navigate = useNavigate();
  const qc = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const superAdmin = isSuperAdmin(user);
  // Solo el superadmin elige entre todos los países (y `/admin/countries` no está abierto al instructor).
  const { data: countries } = useAdminCountries({ enabled: superAdmin });

  // Los cursos nuevos se crean en el país del creador; el superadmin puede cambiarlo antes de guardar.
  const initialCountries = useMemo(
    () => course?.countries.map((c) => c.id) ?? (superAdmin || !user ? [] : [user.countryId]),
    [course, superAdmin, user]
  );
  const [thumbnailUrl, setThumbnailUrl] = useState(course?.thumbnailUrl ?? '');
  const [savedThumbnail, setSavedThumbnail] = useState(course?.thumbnailUrl ?? '');
  const [selectedCountries, setSelectedCountries] = useState<number[]>(initialCountries);
  const [savedCountries, setSavedCountries] = useState<number[]>(initialCountries);
  const [saveError, setSaveError] = useState('');
  const [notice, setNotice] = useState('');
  const [lessonDrawer, setLessonDrawer] = useState<{ open: boolean; lesson: LessonDto | null }>({ open: false, lesson: null });
  const [lessonToDelete, setLessonToDelete] = useState<LessonDto | null>(null);

  // Orden provisional tras arrastrar (optimista). Si el guardado falla se descarta y se ve el orden del servidor.
  const [pendingOrder, setPendingOrder] = useState<string[] | null>(null);
  const [activeDragId, setActiveDragId] = useState<string | null>(null);

  const createCourse = useCreateCourse();
  const updateCourse = useUpdateCourse(id);
  const assignCountries = useAssignCountries(id);
  const reorderLessons = useReorderLessons(id);
  const toggleStatus = useToggleCourseStatus();

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors, isDirty },
  } = useForm<CourseFormData>({
    resolver: zodResolver(courseSchema),
    defaultValues: {
      title: course?.title ?? '',
      description: course?.description ?? '',
      isGlobal: course?.isGlobal ?? false,
    },
  });

  const isGlobal = useWatch({ control, name: 'isGlobal' });

  const lessons = useMemo(() => {
    const sorted = [...(course?.lessons ?? [])].sort((a, b) => a.orderIndex - b.orderIndex);
    if (!pendingOrder) return sorted;
    const byId = new Map(sorted.map((l) => [l.id, l]));
    const ordered = pendingOrder.map((lessonId) => byId.get(lessonId)).filter((l): l is LessonDto => Boolean(l));
    return [...ordered, ...sorted.filter((l) => !pendingOrder.includes(l.id))];
  }, [course, pendingOrder]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }), // evita arrastres accidentales
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

  const handleDragStart = useCallback((event: DragStartEvent) => {
    setActiveDragId(String(event.active.id));
  }, []);

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveDragId(null);
    if (!over || active.id === over.id) return;

    const oldIndex = lessons.findIndex((l) => l.id === active.id);
    const newIndex = lessons.findIndex((l) => l.id === over.id);
    const reordered = arrayMove(lessons, oldIndex, newIndex);
    setPendingOrder(reordered.map((l) => l.id));
    reorderLessons.mutate(
      { orders: reordered.map((l, i) => ({ lessonId: l.id, newOrder: i + 1 })) },
      { onError: () => setPendingOrder(null) }
    );
  };

  const countriesChanged = !sameIds(selectedCountries, savedCountries);
  const hasChanges = isDirty || thumbnailUrl !== savedThumbnail || countriesChanged;

  const onSubmit = async (data: CourseFormData) => {
    setSaveError('');
    setNotice('');
    try {
      const payload = {
        title: data.title.trim(),
        description: data.description?.trim() || undefined,
        thumbnailUrl: thumbnailUrl || undefined,
        isGlobal: data.isGlobal,
      };

      if (isEditing) {
        await updateCourse.mutateAsync(payload);
        if (!data.isGlobal && selectedCountries.length > 0 && countriesChanged) {
          await assignCountries.mutateAsync(selectedCountries);
        }
        reset(data);
        setSavedThumbnail(thumbnailUrl);
        setSavedCountries(selectedCountries);
        setNotice('Cambios guardados.');
        return;
      }

      const courseId = await createCourse.mutateAsync(payload);
      // El backend ya lo asigna al país del creador; solo el superadmin puede elegir otros.
      if (superAdmin && !data.isGlobal && selectedCountries.length > 0 && courseId) {
        // useAssignCountries se ata al id de la ruta (vacío al crear): se llama a la API directamente.
        // La navegación siguiente vuelve a montar el editor con los datos frescos del servidor.
        await coursesApi.assignCountries(courseId, { countryIds: selectedCountries });
      }
      navigate(`/admin/courses/${courseId}/edit`);
    } catch (e) {
      setSaveError(getApiErrorMessage(e));
    }
  };

  const handleToggleStatus = () => {
    if (!course) return;
    setSaveError('');
    toggleStatus.mutate(course.id, {
      onSuccess: () => qc.invalidateQueries({ queryKey: courseKeys.detail(course.id) }),
      onError: (e) => setSaveError(getApiErrorMessage(e)),
    });
  };

  const isPending = createCourse.isPending || updateCourse.isPending || assignCountries.isPending;
  const activeDragLesson = activeDragId ? lessons.find((l) => l.id === activeDragId) : null;
  const countryOptions = superAdmin
    ? countries?.filter((c) => c.isActive || selectedCountries.includes(c.id))
    : ownScopeCountries(course, user);

  // Fuera del superadmin solo se toca el país propio, y nunca se deja el curso sin países:
  // al crear ya va asignado al país del creador, y al editar hace falta que quede algún otro.
  const isCountryEditable = (countryId: number) =>
    superAdmin ||
    (isEditing &&
      canToggleCourseCountry(user, countryId) &&
      selectedCountries.some((selected) => selected !== countryId));
  // El admin gestiona por país: si quita el suyo, pierde el acceso (el instructor lo conserva como creador).
  const leavesOwnCountry =
    user?.role === 'admin' && isEditing && savedCountries.includes(user.countryId) && !selectedCountries.includes(user.countryId);

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
      <BackToCourses />

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold text-foreground">{isEditing ? 'Editar curso' : 'Nuevo curso'}</h1>
            {course && <Badge tone={course.isActive ? 'success' : 'neutral'}>{course.isActive ? 'Publicado' : 'Borrador'}</Badge>}
          </div>
          <p className="mt-1 text-sm text-muted-foreground">
            {isEditing
              ? course?.isActive
                ? 'El curso está visible para los estudiantes de su alcance.'
                : 'Los cambios se guardan como borrador hasta que publiques el curso.'
              : 'Después de crear el curso podrás agregar sus lecciones y el examen final.'}
          </p>
        </div>
        <div className="flex gap-2">
          {course && (
            <Button variant="outline" onClick={handleToggleStatus} disabled={toggleStatus.isPending}>
              {toggleStatus.isPending ? 'Procesando…' : course.isActive ? 'Despublicar' : 'Publicar'}
            </Button>
          )}
          <Button onClick={handleSubmit(onSubmit)} disabled={isPending || (isEditing && !hasChanges)}>
            {isPending ? 'Guardando…' : isEditing ? 'Guardar cambios' : 'Crear curso'}
          </Button>
        </div>
      </div>

      {saveError && <Alert tone="danger">{saveError}</Alert>}
      {notice && !hasChanges && <Alert tone="success">{notice}</Alert>}

      <div className="grid gap-6 md:grid-cols-3">
        <div className="flex min-w-0 flex-col gap-6 md:col-span-2">
          <Card>
            <h2 className="text-base font-semibold text-foreground">Información del curso</h2>
            <form id="course-form" onSubmit={handleSubmit(onSubmit)} noValidate className="mt-5 flex flex-col gap-4">
              <Field label="Título" error={errors.title?.message}>
                <Input {...register('title')} placeholder="Ej. Excel Avanzado" />
              </Field>
              <Field label="Descripción" hint="Se muestra en el catálogo y en el detalle del curso." error={errors.description?.message}>
                <Textarea {...register('description')} rows={4} placeholder="Describe de qué trata el curso, a quién va dirigido y qué aprenderán…" />
              </Field>
              <FileUploadField
                label="Imagen de portada"
                accept="image/jpeg,image/png,image/webp"
                maxMB={25}
                currentUrl={thumbnailUrl}
                hint="JPEG, PNG o WebP · Máx. 25 MB"
                onUpload={async (file, onProgress) => {
                  const res = await coursesApi.uploadThumbnail(file, onProgress);
                  setThumbnailUrl(res.data.url);
                  return res.data.url;
                }}
                onClear={() => setThumbnailUrl('')}
              />
            </form>
          </Card>

          {course && (
            <Card padding="none">
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-border px-6 py-4">
                <div>
                  <h2 className="text-base font-semibold text-foreground">Lecciones</h2>
                  <p className="text-xs text-muted-foreground">
                    {lessons.length} {lessons.length === 1 ? 'lección' : 'lecciones'}
                    {lessons.length > 1 && ' · Arrastra para cambiar el orden.'}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  {reorderLessons.isPending && (
                    <span className="flex items-center gap-2 text-xs text-subtle-foreground">
                      <Spinner size="sm" label="Guardando orden" /> Guardando orden…
                    </span>
                  )}
                  <Button variant="secondary" size="sm" onClick={() => setLessonDrawer({ open: true, lesson: null })}>
                    <PlusIcon /> Nueva lección
                  </Button>
                </div>
              </div>
              {reorderLessons.isError && (
                <Alert tone="danger" className="m-4">
                  No se pudo guardar el orden. Se restauró el orden anterior.
                </Alert>
              )}

              {lessons.length === 0 ? (
                <StateBlock
                  bordered={false}
                  compact
                  icon={ListChecksIcon}
                  title="Este curso aún no tiene lecciones"
                  text="Agrega videos, PDF o quizzes. Puedes reordenarlos después."
                  action="Nueva lección"
                  actionVariant="secondary"
                  onAction={() => setLessonDrawer({ open: true, lesson: null })}
                />
              ) : (
                <DndContext
                  sensors={sensors}
                  collisionDetection={closestCenter}
                  modifiers={[restrictToVerticalAxis, restrictToParentElement]}
                  onDragStart={handleDragStart}
                  onDragEnd={handleDragEnd}
                >
                  <SortableContext items={lessons.map((l) => l.id)} strategy={verticalListSortingStrategy}>
                    <ul className="divide-y divide-border">
                      {lessons.map((lesson, index) => (
                        <SortableLessonRow
                          key={lesson.id}
                          lesson={lesson}
                          index={index}
                          courseId={course.id}
                          isDragging={activeDragId === lesson.id}
                          onEdit={() => setLessonDrawer({ open: true, lesson })}
                          onDelete={() => setLessonToDelete(lesson)}
                        />
                      ))}
                    </ul>
                  </SortableContext>
                  <DragOverlay dropAnimation={{ duration: 150, easing: 'cubic-bezier(0.18, 0.67, 0.6, 1.22)' }}>
                    {activeDragLesson && (
                      <div className="rounded-xl border border-brand/30 bg-card shadow-2xl">
                        <LessonRow
                          index={lessons.indexOf(activeDragLesson) + 1}
                          title={activeDragLesson.title}
                          type={activeDragLesson.type}
                          isRequired={activeDragLesson.isRequired}
                          className="px-4 py-3"
                          dragHandle={<GripVerticalIcon className="size-4 text-subtle-foreground" aria-hidden="true" />}
                        />
                      </div>
                    )}
                  </DragOverlay>
                </DndContext>
              )}
            </Card>
          )}
        </div>

        <div className="flex min-w-0 flex-col gap-6">
          <Card>
            <h2 className="text-base font-semibold text-foreground">Alcance</h2>
            <p className="mt-1 text-xs text-muted-foreground">
              {superAdmin ? 'Define quién puede ver este curso.' : 'Visible para los estudiantes de los países asignados.'}
            </p>
            {superAdmin && (
              <Controller
                control={control}
                name="isGlobal"
                render={({ field }) => (
                  <div role="radiogroup" aria-label="Alcance del curso" className="mt-4 flex flex-col gap-2">
                    <RadioCard
                      name="scope"
                      value="global"
                      checked={field.value}
                      onChange={() => field.onChange(true)}
                      label="Global"
                      description="Visible para estudiantes de todos los países."
                    />
                    <RadioCard
                      name="scope"
                      value="countries"
                      checked={!field.value}
                      onChange={() => field.onChange(false)}
                      label="Países asignados"
                      description="Visible solo en los países que elijas."
                    />
                  </div>
                )}
              />
            )}
            {!isGlobal && (
              <div className={superAdmin ? 'mt-4 border-t border-border pt-4' : 'mt-4'}>
                <Label className="text-xs">Países asignados</Label>
                {countryOptions ? (
                  <div className="flex flex-col gap-2">
                    {countryOptions.map((c) => (
                      <Checkbox
                        key={c.id}
                        className="w-full rounded-lg px-2 py-1.5 hover:bg-secondary"
                        checked={selectedCountries.includes(c.id)}
                        disabled={!isCountryEditable(c.id)}
                        onChange={() =>
                          setSelectedCountries((prev) => (prev.includes(c.id) ? prev.filter((x) => x !== c.id) : [...prev, c.id]))
                        }
                        label={
                          <span className="flex w-full items-center justify-between gap-3">
                            {c.name}
                            {c.code && <span className="font-mono text-xs text-subtle-foreground">{c.code}</span>}
                          </span>
                        }
                      />
                    ))}
                  </div>
                ) : (
                  <Skeleton className="h-24 rounded-xl" />
                )}
                {superAdmin && selectedCountries.length === 0 && (
                  <p className="mt-3 text-xs text-warning-foreground">Selecciona al menos un país para que el curso sea visible.</p>
                )}
                {!superAdmin && (
                  <p className="mt-3 text-xs text-subtle-foreground">
                    {isEditing
                      ? 'Solo puedes añadir o quitar tu país; el resto de países los gestiona el superadmin.'
                      : 'El curso se crea en tu país. Solo el superadmin puede publicarlo en otros países o de forma global.'}
                  </p>
                )}
                {leavesOwnCountry && (
                  <Alert tone="warning" className="mt-3">
                    Si quitas tu país, dejarás de poder gestionar este curso.
                  </Alert>
                )}
              </div>
            )}
          </Card>

          {course && (
            <Card>
              <div className="flex items-center gap-3">
                <span className="flex size-10 items-center justify-center rounded-xl bg-brand/10 text-brand-foreground">
                  <ClipboardCheckIcon className="size-5" aria-hidden="true" />
                </span>
                <div>
                  <h2 className="text-base font-semibold text-foreground">Examen final del curso</h2>
                  <p className="text-xs text-muted-foreground">Se habilita al completar las lecciones requeridas.</p>
                </div>
              </div>
              <ButtonLink to={`/admin/courses/${course.id}/quizzes`} variant="outline" fullWidth className="mt-4">
                Gestionar examen final
              </ButtonLink>
            </Card>
          )}
        </div>
      </div>

      {course && (
        <>
          <Drawer
            open={lessonDrawer.open}
            onClose={() => setLessonDrawer({ open: false, lesson: null })}
            title={lessonDrawer.lesson ? 'Editar lección' : 'Nueva lección'}
          >
            {lessonDrawer.open && (
              <LessonForm
                key={lessonDrawer.lesson?.id ?? 'new'}
                courseId={course.id}
                lesson={lessonDrawer.lesson}
                onDone={() => setLessonDrawer({ open: false, lesson: null })}
              />
            )}
          </Drawer>
          <DeleteLessonModal courseId={course.id} lesson={lessonToDelete} onClose={() => setLessonToDelete(null)} />
        </>
      )}
    </div>
  );
}

// ── Sortable Lesson Row ───────────────────────────────────────────────────────

function SortableLessonRow({
  lesson,
  index,
  courseId,
  isDragging,
  onEdit,
  onDelete,
}: {
  lesson: LessonDto;
  index: number;
  courseId: string;
  isDragging: boolean;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isSorting } = useSortable({ id: lesson.id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition: isSorting ? transition : undefined,
    opacity: isDragging ? 0.4 : 1,
  };

  return (
    <li ref={setNodeRef} style={style}>
      <LessonRow
        index={index + 1}
        title={lesson.title}
        type={lesson.type}
        isRequired={lesson.isRequired}
        className="px-4 py-3"
        dragHandle={
          <button
            type="button"
            {...attributes}
            {...listeners}
            className="flex size-7 shrink-0 cursor-grab items-center justify-center rounded-md text-subtle-foreground transition-colors hover:bg-secondary hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring active:cursor-grabbing"
            aria-label={`Arrastrar «${lesson.title}» para reordenar`}
            title="Arrastrar para reordenar"
          >
            <GripVerticalIcon className="size-4" />
          </button>
        }
        actions={
          <div className="flex items-center gap-1">
            {lesson.type === 'quiz' && (
              <ButtonLink to={`/admin/courses/${courseId}/lessons/${lesson.id}/quizzes`} variant="ghost" size="sm">
                <ListChecksIcon /> Preguntas
              </ButtonLink>
            )}
            <Button variant="ghost" size="icon" aria-label={`Editar «${lesson.title}»`} title="Editar" onClick={onEdit}>
              <PencilIcon />
            </Button>
            <Button variant="danger" size="icon" aria-label={`Eliminar «${lesson.title}»`} title="Eliminar" onClick={onDelete}>
              <TrashIcon />
            </Button>
          </div>
        }
      />
    </li>
  );
}

// ── Delete lesson ─────────────────────────────────────────────────────────────

function DeleteLessonModal({ courseId, lesson, onClose }: { courseId: string; lesson: LessonDto | null; onClose: () => void }) {
  const deleteLesson = useDeleteLesson(courseId);
  const [error, setError] = useState('');

  const close = () => {
    setError('');
    onClose();
  };

  return (
    <Modal
      open={!!lesson}
      onClose={close}
      size="sm"
      title="Eliminar lección"
      description="Esta acción no se puede deshacer."
      footer={
        <>
          <Button variant="secondary" onClick={close}>
            Cancelar
          </Button>
          <Button
            variant="destructive"
            disabled={deleteLesson.isPending}
            onClick={() =>
              lesson &&
              deleteLesson.mutate(lesson.id, {
                onSuccess: close,
                onError: (e) => setError(getApiErrorMessage(e)),
              })
            }
          >
            <TrashIcon /> {deleteLesson.isPending ? 'Eliminando…' : 'Eliminar lección'}
          </Button>
        </>
      }
    >
      {lesson && (
        <div className="flex flex-col gap-3">
          <p className="text-sm text-foreground-soft">Se eliminará «{lesson.title}» y el progreso de los estudiantes en ella.</p>
          {error && <Alert tone="danger">{error}</Alert>}
        </div>
      )}
    </Modal>
  );
}

// ── Lesson Form (Drawer) ──────────────────────────────────────────────────────

const lessonTypes: { value: LessonFormData['type']; label: string; description: string }[] = [
  { value: 'video', label: 'Video', description: 'MP4 · Máx. 500 MB' },
  { value: 'pdf', label: 'PDF', description: 'Documento · Máx. 25 MB' },
  { value: 'quiz', label: 'Quiz', description: 'Preguntas de opción única' },
];

function LessonForm({ courseId, lesson, onDone }: { courseId: string; lesson: LessonDto | null; onDone: () => void }) {
  const isEditing = !!lesson;
  const createLesson = useCreateLesson(courseId);
  const updateLesson = useUpdateLesson(courseId);

  const [contentUrl, setContentUrl] = useState(lesson?.contentUrl ?? '');
  const [error, setError] = useState('');

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<LessonFormData>({
    resolver: zodResolver(lessonSchema),
    defaultValues: {
      title: lesson?.title ?? '',
      type: (lesson?.type as LessonFormData['type']) ?? 'video',
      isRequired: lesson?.isRequired ?? true,
    },
  });

  const type = useWatch({ control, name: 'type' });

  const onSubmit = async (data: LessonFormData) => {
    setError('');

    if (data.type !== 'quiz' && !contentUrl) {
      setError('Sube el archivo de contenido antes de guardar.');
      return;
    }

    try {
      if (isEditing) {
        await updateLesson.mutateAsync({
          lessonId: lesson.id,
          title: data.title.trim(),
          contentUrl: contentUrl || undefined,
          isRequired: data.isRequired,
        });
      } else {
        await createLesson.mutateAsync({
          title: data.title.trim(),
          type: data.type,
          contentUrl: contentUrl || undefined,
          isRequired: data.isRequired,
        });
      }
      onDone();
    } catch (e) {
      setError(getApiErrorMessage(e));
    }
  };

  const isPending = createLesson.isPending || updateLesson.isPending;
  const uploadFn = type === 'video' ? coursesApi.uploadVideo : coursesApi.uploadPdf;

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="flex flex-col gap-5">
      <Field label="Título de la lección" error={errors.title?.message}>
        <Input {...register('title')} placeholder="Ej. Clase 1 — Introducción" />
      </Field>

      {!isEditing && (
        <div>
          <Label>Tipo</Label>
          <div role="radiogroup" aria-label="Tipo de lección" className="flex flex-col gap-2">
            {lessonTypes.map((t) => (
              <RadioCard key={t.value} {...register('type')} value={t.value} label={t.label} description={t.description} />
            ))}
          </div>
        </div>
      )}

      {type !== 'quiz' ? (
        <FileUploadField
          key={type}
          label={type === 'video' ? 'Archivo de video' : 'Archivo PDF'}
          accept={type === 'video' ? 'video/mp4' : 'application/pdf'}
          maxMB={type === 'video' ? 500 : 25}
          currentUrl={contentUrl || undefined}
          hint={type === 'video' ? 'MP4 · Máx. 500 MB' : 'PDF · Máx. 25 MB'}
          onUpload={async (file, onProgress) => {
            const res = await uploadFn(file, onProgress);
            setContentUrl(res.data.url);
            return res.data.url;
          }}
          onClear={() => setContentUrl('')}
        />
      ) : (
        <Alert>Las preguntas del quiz se agregan desde «Preguntas» en la lista de lecciones, después de guardar.</Alert>
      )}

      <Checkbox {...register('isRequired')} label="Lección requerida para completar el curso" />

      {error && <Alert tone="danger">{error}</Alert>}

      <div className="flex gap-2 pt-2">
        <Button variant="secondary" onClick={onDone}>
          Cancelar
        </Button>
        <Button type="submit" className="flex-1" disabled={isPending}>
          {isPending ? 'Guardando…' : isEditing ? 'Guardar cambios' : 'Agregar lección'}
        </Button>
      </div>
    </form>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function CourseFormSkeleton() {
  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8" aria-busy="true">
      <Skeleton className="h-4 w-20" />
      <Skeleton className="h-8 w-56" />
      <div className="grid gap-6 md:grid-cols-3">
        <div className="space-y-6 md:col-span-2">
          <Skeleton className="h-64 rounded-2xl" />
          <Skeleton className="h-64 rounded-2xl" />
        </div>
        <Skeleton className="h-64 rounded-2xl" />
      </div>
    </div>
  );
}
