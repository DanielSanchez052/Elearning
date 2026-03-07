import { useEffect, useState, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

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

import { useCourseDetail } from '@/hooks/useCourses';
import { useAdminCountries } from '@/hooks/useAdmin';
import {
  useCreateCourse,
  useUpdateCourse,
  useAssignCountries,
  useCreateLesson,
  useUpdateLesson,
  useDeleteLesson,
  useReorderLessons,
} from '@/hooks/useAdmin';
import { coursesApi } from '@/api/admin/courses';
// import { useAuthStore } from '../../store/authStore';
import Drawer from '@/components/ui/Drawer';
import FileUploadField from '@/components/admin/FileUploadField';
import { getApiErrorMessage } from '../../lib/axios';
import type { LessonDto } from '../../types';

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

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CourseFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEditing = !!id;
  const navigate = useNavigate();
  // const user = useAuthStore((s) => s.user);

  const { data: course, isLoading } = useCourseDetail(id ?? '');
  const { data: countries } = useAdminCountries();

  const [thumbnailUrl, setThumbnailUrl] = useState('');
  const [selectedCountries, setSelectedCountries] = useState<number[]>([]);
  const [saveError, setSaveError] = useState('');
  const [lessonDrawer, setLessonDrawer] = useState<{
    open: boolean;
    lesson: LessonDto | null;
  }>({ open: false, lesson: null });

  // Local lesson list for optimistic drag & drop ordering
  const [localLessons, setLocalLessons] = useState<LessonDto[]>([]);
  const [activeDragId, setActiveDragId] = useState<string | null>(null);

  const createCourse = useCreateCourse();
  const updateCourse = useUpdateCourse(id ?? '');
  const assignCountries = useAssignCountries(id ?? '');
  const reorderLessons = useReorderLessons(id ?? '');

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isDirty } } =
    useForm<CourseFormData>({
      resolver: zodResolver(courseSchema),
      defaultValues: { isGlobal: false },
    });

  const isGlobal = watch('isGlobal');

  // Sync course data on load/update
  useEffect(() => {
    if (course) {
      reset({
        title: course.title,
        description: course.description ?? '',
        isGlobal: course.isGlobal,
      });
      setThumbnailUrl(course.thumbnailUrl ?? '');
      setSelectedCountries(course.countries.map((c) => c.id));

      // Only init localLessons if not mid-drag
      if (!activeDragId) {
        setLocalLessons(
          [...course.lessons].sort((a, b) => a.orderIndex - b.orderIndex)
        );
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [course, reset]);

  // dnd-kit sensors
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 5 }, // prevent accidental drags
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragStart = useCallback((event: DragStartEvent) => {
    setActiveDragId(String(event.active.id));
  }, []);

  const handleDragEnd = useCallback(async (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveDragId(null);

    if (!over || active.id === over.id) return;

    setLocalLessons((prev) => {
      const oldIndex = prev.findIndex((l) => l.id === active.id);
      const newIndex = prev.findIndex((l) => l.id === over.id);
      const reordered = arrayMove(prev, oldIndex, newIndex);

      // Fire API call with new order (1-based)
      const orders = reordered.map((l, i) => ({
        lessonId: l.id,
        newOrder: i + 1,
      }));
      reorderLessons.mutate({ orders });

      return reordered;
    });
  }, [reorderLessons]);

  const onSubmit = async (data: CourseFormData) => {
    setSaveError('');
    try {
      const payload = {
        title: data.title.trim(),
        description: data.description?.trim() || undefined,
        thumbnailUrl: thumbnailUrl || undefined,
        isGlobal: data.isGlobal,
      };

      let courseId = id;

      if (isEditing) {
        await updateCourse.mutateAsync(payload);
      } else {
        courseId = await createCourse.mutateAsync(payload);
      }

      if (!data.isGlobal && selectedCountries.length > 0 && courseId) {
        await assignCountries.mutateAsync(selectedCountries);
      }

      navigate(`/courses/${courseId}/edit`);
    } catch (e) {
      setSaveError(getApiErrorMessage(e));
    }
  };

  const isPending = createCourse.isPending || updateCourse.isPending;

  const activeDragLesson = activeDragId
    ? localLessons.find((l) => l.id === activeDragId)
    : null;

  if (isEditing && isLoading) return <CourseFormSkeleton />;

  return (
    <div className="min-h-screen bg-[#0a0a0f] text-white">

      {/* Header */}
      <div className="border-b border-white/[0.06] bg-[#0a0a0f]/80 backdrop-blur-sm sticky top-0 z-10">
        <div className="max-w-4xl mx-auto px-6 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate(-1)}
              className="text-zinc-400 hover:text-white transition-colors"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
            </button>
            <h1 className="text-lg font-semibold">
              {isEditing ? 'Editar curso' : 'Nuevo curso'}
            </h1>
          </div>
          <button
            onClick={handleSubmit(onSubmit)}
            disabled={isPending || (!isDirty && isEditing && !thumbnailUrl)}
            className="px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-medium transition-colors"
          >
            {isPending ? 'Guardando...' : isEditing ? 'Guardar cambios' : 'Crear curso'}
          </button>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8 space-y-8">

        {/* ── Sección 1: Información del curso ── */}
        <section className="bg-[#111118] border border-white/[0.06] rounded-2xl p-6 space-y-5">
          <h2 className="text-sm font-semibold text-zinc-300 uppercase tracking-wider">
            Información del curso
          </h2>

          {/* Título */}
          <div>
            <label className="block text-sm font-medium text-zinc-300 mb-1.5">
              Título <span className="text-red-400">*</span>
            </label>
            <input
              {...register('title')}
              placeholder="Ej: Excel para Contadores"
              className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
            />
            {errors.title && <p className="mt-1.5 text-xs text-red-400">{errors.title.message}</p>}
          </div>

          {/* Descripción */}
          <div>
            <label className="block text-sm font-medium text-zinc-300 mb-1.5">
              Descripción
            </label>
            <textarea
              {...register('description')}
              rows={4}
              placeholder="Describe de qué trata el curso, a quién va dirigido y qué aprenderán..."
              className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all resize-none"
            />
            {errors.description && (
              <p className="mt-1.5 text-xs text-red-400">{errors.description.message}</p>
            )}
          </div>

          {/* Thumbnail */}
          <FileUploadField
            label="Imagen de portada"
            accept="image/jpeg,image/png,image/webp"
            maxMB={25}
            currentUrl={thumbnailUrl}
            hint="JPEG, PNG o WebP · Máx. 25 MB"
            onUpload={async (file, onProgress) => {
              const res = await coursesApi.uploadThumbnail(file, onProgress);
              const url = res.data.url;
              setThumbnailUrl(url);
              return url;
            }}
            onClear={() => setThumbnailUrl('')}
          />

          {/* Alcance */}
          <div>
            <label className="block text-sm font-medium text-zinc-300 mb-3">
              Alcance del curso
            </label>
            <div className="grid grid-cols-2 gap-3">
              {[
                {
                  value: false,
                  label: 'Por país',
                  desc: 'Solo visible para países asignados',
                  icon: (
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064" />
                    </svg>
                  ),
                },
                {
                  value: true,
                  label: 'Global',
                  desc: 'Visible en todos los países',
                  icon: (
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                    </svg>
                  ),
                },
              ].map(({ value, label, desc, icon }) => (
                <button
                  key={String(value)}
                  type="button"
                  onClick={() =>
                    setValue('isGlobal', value, { shouldDirty: true })
                  }
                  className={`flex items-start gap-3 p-4 rounded-xl border text-left transition-all ${isGlobal === value
                    ? 'bg-indigo-600/15 border-indigo-500/40'
                    : 'bg-white/[0.02] border-white/[0.06] hover:border-white/[0.12]'
                    }`}
                >
                  <span className={`mt-0.5 ${isGlobal === value ? 'text-indigo-400' : 'text-zinc-600'}`}>
                    {icon}
                  </span>
                  <div>
                    <p className={`text-sm font-medium ${isGlobal === value ? 'text-white' : 'text-zinc-400'}`}>
                      {label}
                    </p>
                    <p className="text-xs text-zinc-600 mt-0.5">{desc}</p>
                  </div>
                </button>
              ))}
            </div>
          </div>

          {/* Asignación de países */}
          {!isGlobal && countries && (
            <div>
              <label className="block text-sm font-medium text-zinc-300 mb-2">
                Países donde estará disponible
              </label>
              <div className="flex flex-wrap gap-2">
                {countries.filter((c) => c.isActive).map((c) => {
                  const selected = selectedCountries.includes(c.id);
                  return (
                    <button
                      key={c.id}
                      type="button"
                      onClick={() =>
                        setSelectedCountries((prev) =>
                          selected ? prev.filter((id) => id !== c.id) : [...prev, c.id]
                        )
                      }
                      className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg border text-xs font-medium transition-all ${selected
                        ? 'bg-indigo-600/20 border-indigo-500/40 text-indigo-300'
                        : 'bg-white/[0.03] border-white/[0.08] text-zinc-500 hover:border-white/[0.16]'
                        }`}
                    >
                      <span className="font-mono">{c.code}</span>
                      {c.name}
                    </button>
                  );
                })}
              </div>
              {!isGlobal && selectedCountries.length === 0 && (
                <p className="mt-2 text-xs text-amber-500/80">
                  Selecciona al menos un país para que el curso sea visible.
                </p>
              )}
            </div>
          )}
        </section>

        {/* Error general */}
        {saveError && (
          <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
            {saveError}
          </div>
        )}

        {/* ── Sección 2: Lecciones con drag & drop ── */}
        {isEditing && course && (
          <section className="bg-[#111118] border border-white/[0.06] rounded-2xl p-6 space-y-5">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-sm font-semibold text-zinc-300 uppercase tracking-wider">
                  Lecciones
                </h2>
                <p className="text-xs text-zinc-600 mt-0.5">
                  {localLessons.length} {localLessons.length === 1 ? 'lección' : 'lecciones'}
                  {localLessons.length > 1 && (
                    <span className="ml-2 text-zinc-700">· Arrastra para reordenar</span>
                  )}
                </p>
              </div>
              <button
                type="button"
                onClick={() => setLessonDrawer({ open: true, lesson: null })}
                className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-indigo-600/20 border border-indigo-500/30 text-indigo-300 hover:bg-indigo-600/30 text-sm transition-all"
              >
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Agregar lección
              </button>
            </div>

            {localLessons.length === 0 ? (
              <div className="text-center py-10 rounded-xl border border-dashed border-white/[0.08]">
                <p className="text-zinc-600 text-sm">No hay lecciones aún.</p>
                <button
                  type="button"
                  onClick={() => setLessonDrawer({ open: true, lesson: null })}
                  className="mt-2 text-indigo-400 text-sm hover:text-indigo-300 transition-colors"
                >
                  Agrega la primera lección →
                </button>
              </div>
            ) : (
              <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                modifiers={[restrictToVerticalAxis, restrictToParentElement]}
                onDragStart={handleDragStart}
                onDragEnd={handleDragEnd}
              >
                <SortableContext
                  items={localLessons.map((l) => l.id)}
                  strategy={verticalListSortingStrategy}
                >
                  <div className="space-y-2">
                    {localLessons.map((lesson, index) => (
                      <SortableLessonRow
                        key={lesson.id}
                        lesson={lesson}
                        index={index}
                        onEdit={() => setLessonDrawer({ open: true, lesson })}
                        courseId={id!}
                        isDragging={activeDragId === lesson.id}
                      />
                    ))}
                  </div>
                </SortableContext>

                {/* Drag overlay — renders the floating ghost item */}
                <DragOverlay dropAnimation={{
                  duration: 150,
                  easing: 'cubic-bezier(0.18, 0.67, 0.6, 1.22)',
                }}>
                  {activeDragLesson && (
                    <LessonRowGhost lesson={activeDragLesson} />
                  )}
                </DragOverlay>
              </DndContext>
            )}

            {/* Reorder saving indicator */}
            {reorderLessons.isPending && (
              <div className="flex items-center gap-2 text-xs text-zinc-600">
                <svg className="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
                Guardando orden...
              </div>
            )}
            {reorderLessons.isError && (
              <p className="text-xs text-red-400">
                Error al guardar el orden. El orden se revirtió.
              </p>
            )}
          </section>
        )}

        {/* Nota si es creación */}
        {!isEditing && (
          <div className="flex items-start gap-3 px-4 py-3 rounded-xl bg-indigo-500/5 border border-indigo-500/20">
            <svg className="w-4 h-4 text-indigo-400 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p className="text-sm text-zinc-400">
              Después de crear el curso podrás agregar lecciones desde la misma página de edición.
            </p>
          </div>
        )}
      </div>

      {/* Drawer de lección */}
      {isEditing && (
        <Drawer
          open={lessonDrawer.open}
          onClose={() => setLessonDrawer({ open: false, lesson: null })}
          title={lessonDrawer.lesson ? 'Editar lección' : 'Nueva lección'}
        >
          <LessonForm
            courseId={id!}
            lesson={lessonDrawer.lesson}
            onSuccess={() => setLessonDrawer({ open: false, lesson: null })}
          />
        </Drawer>
      )}
    </div>
  );
}

// ── Sortable Lesson Row ───────────────────────────────────────────────────────

function SortableLessonRow({
  lesson,
  index,
  onEdit,
  courseId,
  isDragging,
}: {
  lesson: LessonDto;
  index: number;
  onEdit: () => void;
  courseId: string;
  isDragging: boolean;
}) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isSorting,
  } = useSortable({ id: lesson.id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition: isSorting ? transition : undefined,
    opacity: isDragging ? 0.4 : 1,
    zIndex: isDragging ? 0 : 'auto',
  };

  return (
    <div ref={setNodeRef} style={style}>
      <LessonRowContent
        lesson={lesson}
        index={index}
        onEdit={onEdit}
        courseId={courseId}
        dragHandleProps={{ ...attributes, ...listeners }}
      />
    </div>
  );
}

// ── Ghost overlay (floating item during drag) ─────────────────────────────────

function LessonRowGhost({ lesson }: { lesson: LessonDto }) {
  return (
    <div className="shadow-2xl shadow-black/50 scale-[1.02]">
      <LessonRowContent
        lesson={lesson}
        index={0}
        onEdit={() => { }}
        courseId=""
        isDragOverlay
      />
    </div>
  );
}

// ── Shared Lesson Row Content ─────────────────────────────────────────────────

const TYPE_ICONS: Record<string, React.ReactNode> = {
  video: (
    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  ),
  pdf: (
    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
    </svg>
  ),
  quiz: (
    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  ),
};

const TYPE_COLORS: Record<string, string> = {
  video: 'text-indigo-400 bg-indigo-500/10',
  pdf: 'text-amber-400  bg-amber-500/10',
  quiz: 'text-emerald-400 bg-emerald-500/10',
};

function LessonRowContent({
  lesson,
  index,
  onEdit,
  courseId,
  dragHandleProps,
  isDragOverlay,
}: {
  lesson: LessonDto;
  index: number;
  onEdit: () => void;
  courseId: string;
  dragHandleProps?: React.HTMLAttributes<HTMLElement>;
  isDragOverlay?: boolean;
}) {
  const deleteLesson = useDeleteLesson(courseId);
  const [confirmDelete, setConfirm] = useState(false);

  return (
    <div className={`
      flex items-center gap-3 px-4 py-3 rounded-xl border group transition-all
      ${isDragOverlay
        ? 'bg-[#1a1a28] border-indigo-500/30 cursor-grabbing'
        : 'bg-white/[0.02] border-white/[0.06] hover:border-white/[0.10] cursor-default'
      }
    `}>

      {/* Drag handle */}
      <button
        type="button"
        {...dragHandleProps}
        className={`
          flex-shrink-0 p-1 rounded-md text-zinc-700
          hover:text-zinc-400 hover:bg-white/[0.06]
          focus:outline-none focus:ring-1 focus:ring-indigo-500/50
          transition-all cursor-grab active:cursor-grabbing
          ${isDragOverlay ? 'cursor-grabbing text-zinc-500' : ''}
        `}
        aria-label="Arrastrar para reordenar"
        title="Arrastrar para reordenar"
      >
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
            d="M4 8h16M4 16h16" />
        </svg>
      </button>

      {/* Index badge */}
      <span className="w-6 h-6 rounded-md bg-white/[0.04] flex items-center justify-center text-xs text-zinc-600 flex-shrink-0 tabular-nums">
        {index + 1}
      </span>

      {/* Type badge */}
      <span className={`p-1.5 rounded-lg flex-shrink-0 ${TYPE_COLORS[lesson.type] ?? 'text-zinc-400 bg-white/[0.06]'}`}>
        {TYPE_ICONS[lesson.type]}
      </span>

      {/* Title */}
      <span className="flex-1 text-sm text-zinc-300 truncate">{lesson.title}</span>

      {/* Required */}
      {lesson.isRequired && (
        <span className="text-xs text-zinc-600 hidden sm:block">Requerida</span>
      )}

      {/* Actions — hidden on drag overlay */}
      {!isDragOverlay && (
        <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            type="button"
            onClick={onEdit}
            className="p-1.5 rounded-lg hover:bg-white/[0.08] text-zinc-500 hover:text-white transition-all"
            aria-label="Editar lección"
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
            </svg>
          </button>

          {!confirmDelete ? (
            <button
              type="button"
              onClick={() => setConfirm(true)}
              className="p-1.5 rounded-lg hover:bg-red-500/10 text-zinc-500 hover:text-red-400 transition-all"
              aria-label="Eliminar lección"
            >
              <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          ) : (
            <div className="flex items-center gap-1">
              <button
                type="button"
                onClick={() => deleteLesson.mutate(lesson.id)}
                disabled={deleteLesson.isPending}
                className="px-2 py-1 rounded-lg bg-red-600 text-white text-xs disabled:opacity-50"
              >
                {deleteLesson.isPending ? '...' : 'Confirmar'}
              </button>
              <button
                type="button"
                onClick={() => setConfirm(false)}
                className="px-2 py-1 rounded-lg bg-white/[0.04] text-zinc-400 text-xs"
              >
                Cancelar
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

// ── Lesson Form (Drawer) ──────────────────────────────────────────────────────

function LessonForm({
  courseId,
  lesson,
  onSuccess,
}: {
  courseId: string;
  lesson: LessonDto | null;
  onSuccess: () => void;
}) {
  const isEditing = !!lesson;
  const createLesson = useCreateLesson(courseId);
  const updateLesson = useUpdateLesson(courseId);

  const [contentUrl, setContentUrl] = useState(lesson?.contentUrl ?? '');
  const [error, setError] = useState('');

  const { register, handleSubmit, watch, formState: { errors } } =
    useForm<LessonFormData>({
      resolver: zodResolver(lessonSchema),
      defaultValues: {
        title: lesson?.title ?? '',
        type: (lesson?.type as LessonFormData['type']) ?? 'video',
        isRequired: lesson?.isRequired ?? true,
      },
    });

  const type = watch('type');

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
      onSuccess();
    } catch (e) {
      setError(getApiErrorMessage(e));
    }
  };

  const isPending = createLesson.isPending || updateLesson.isPending;

  const uploadFn = type === 'video'
    ? coursesApi.uploadVideo
    : coursesApi.uploadPdf;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">

      {/* Título */}
      <div>
        <label className="block text-sm font-medium text-zinc-300 mb-1.5">
          Título <span className="text-red-400">*</span>
        </label>
        <input
          {...register('title')}
          placeholder="Ej: Clase 1 — Introducción"
          className="w-full px-4 py-2.5 rounded-xl bg-white/[0.04] border border-white/[0.08] text-white placeholder-zinc-600 text-sm focus:outline-none focus:border-indigo-500/60 transition-all"
        />
        {errors.title && <p className="mt-1.5 text-xs text-red-400">{errors.title.message}</p>}
      </div>

      {/* Tipo — solo al crear */}
      {!isEditing && (
        <div>
          <label className="block text-sm font-medium text-zinc-300 mb-2">Tipo</label>
          <div className="grid grid-cols-3 gap-2">
            {(['video', 'pdf', 'quiz'] as const).map((t) => (
              <label
                key={t}
                className={`flex flex-col items-center gap-1.5 p-3 rounded-xl border cursor-pointer transition-all ${type === t
                  ? 'bg-indigo-600/15 border-indigo-500/40'
                  : 'bg-white/[0.02] border-white/[0.06] hover:border-white/[0.12]'
                  }`}
              >
                <input {...register('type')} type="radio" value={t} className="sr-only" />
                <span className={`text-xs font-medium capitalize ${type === t ? 'text-indigo-300' : 'text-zinc-500'}`}>
                  {t === 'quiz' ? 'Quiz' : t.toUpperCase()}
                </span>
              </label>
            ))}
          </div>
        </div>
      )}

      {/* Upload de contenido */}
      {type !== 'quiz' && (
        <FileUploadField
          label={type === 'video' ? 'Archivo de video' : 'Archivo PDF'}
          accept={type === 'video' ? 'video/mp4' : 'application/pdf'}
          maxMB={type === 'video' ? 500 : 25}
          currentUrl={contentUrl || undefined}
          hint={type === 'video' ? 'MP4 · Máx. 500 MB' : 'PDF · Máx. 25 MB'}
          onUpload={async (file, onProgress) => {
            const res = await uploadFn(file, onProgress);
            const url = res.data.url;
            setContentUrl(url);
            return url;
          }}
          onClear={() => setContentUrl('')}
        />
      )}

      {type === 'quiz' && (
        <div className="px-4 py-3 rounded-xl bg-amber-500/5 border border-amber-500/20 text-amber-400/80 text-sm">
          Los quizzes se configurarán en el MVP 2.
        </div>
      )}

      {/* Required toggle */}
      <label className="flex items-center gap-3 cursor-pointer group">
        <div className="relative">
          <input {...register('isRequired')} type="checkbox" className="sr-only peer" />
          <div className="w-9 h-5 rounded-full bg-white/[0.08] border border-white/[0.12] peer-checked:bg-indigo-600 peer-checked:border-indigo-600 transition-all" />
          <div className="absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white peer-checked:translate-x-4 transition-transform" />
        </div>
        <span className="text-sm text-zinc-300">Lección requerida para completar el curso</span>
      </label>

      {error && (
        <div className="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
          {error}
        </div>
      )}

      <button
        type="submit"
        disabled={isPending}
        className="w-full py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white text-sm font-medium transition-colors"
      >
        {isPending ? 'Guardando...' : isEditing ? 'Guardar cambios' : 'Agregar lección'}
      </button>
    </form>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function CourseFormSkeleton() {
  return (
    <div className="min-h-screen bg-[#0a0a0f]">
      <div className="border-b border-white/[0.06] h-14" />
      <div className="max-w-4xl mx-auto px-6 py-8 space-y-4 animate-pulse">
        <div className="h-48 rounded-2xl bg-white/[0.04]" />
        <div className="h-32 rounded-2xl bg-white/[0.04]" />
      </div>
    </div>
  );
}
