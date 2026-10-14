import { useMemo, useState } from 'react';
import { Controller, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { QuizType, type QuizQuestion } from '@/types/quiz.types';
import { Alert, Button, Field, Input, Label, Modal, Select, Switch, Textarea } from '@/components/ui';
import { PlusIcon, XIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';

const formSchema = z.object({
  type: z.enum([String(QuizType.PerLesson), String(QuizType.CourseExam)]),
  lessonId: z.string().optional(),
  questionText: z.string().min(10, 'La pregunta debe tener al menos 10 caracteres'),
  passScore: z.coerce
    .number()
    .min(0, 'El puntaje debe ser mayor o igual a 0')
    .max(100, 'El puntaje no puede exceder 100'),
  maxAttempts: z.coerce.number().min(1, 'Debe permitir al menos 1 intento').max(10, 'Máximo 10 intentos'),
  isRequired: z.boolean(),
});

type FormValues = z.input<typeof formSchema>;

interface OptionDraft {
  id: string;
  optionId?: string;
  text: string;
  isCorrect: boolean;
}

export interface QuestionComposerSubmit {
  question: {
    lessonId?: string | null;
    courseId?: string | null;
    type: 0 | 1;
    questionText: string;
    passScore: number;
    maxAttempts: number;
    isRequired: boolean;
  };
  options: Array<{
    optionId?: string;
    optionText: string;
    isCorrect: boolean;
    orderIndex: number;
  }>;
}

interface QuestionComposerModalProps {
  lessonId?: string;
  courseId?: string;
  lessonOptions?: Array<{
    id: string;
    title: string;
    orderIndex: number;
    type?: string;
  }>;
  mode?: 'create' | 'edit';
  initialQuestion?: QuizQuestion;
  onClose: () => void;
  onSubmit: (data: QuestionComposerSubmit) => Promise<void>;
  isLoading?: boolean;
  /** Error de la API al guardar. */
  error?: string;
}

function createEmptyOptions(): OptionDraft[] {
  return [
    { id: crypto.randomUUID(), text: '', isCorrect: true },
    { id: crypto.randomUUID(), text: '', isCorrect: false },
    { id: crypto.randomUUID(), text: '', isCorrect: false },
  ];
}

function mapQuestionToOptions(question?: QuizQuestion): OptionDraft[] {
  if (!question || question.options.length === 0) {
    return createEmptyOptions();
  }

  const ordered: OptionDraft[] = [...question.options]
    .sort((a, b) => a.orderIndex - b.orderIndex)
    .map((option) => ({
      id: crypto.randomUUID(),
      optionId: option.id,
      text: option.optionText,
      isCorrect: Boolean(option.isCorrect),
    }));

  if (!ordered.some((option) => option.isCorrect)) {
    ordered[0] = { ...ordered[0], isCorrect: true };
  }

  while (ordered.length < 3) {
    ordered.push({ id: crypto.randomUUID(), text: '', isCorrect: false });
  }

  return ordered;
}

/**
 * Modal para crear o editar una pregunta de opción única con sus opciones (mínimo 3, exactamente 1 correcta).
 * El padre lo monta cada vez que se abre, así que el estado inicial sale de `initialQuestion`.
 */
export function QuestionComposerModal({
  lessonId,
  courseId,
  lessonOptions = [],
  mode = 'create',
  initialQuestion,
  onClose,
  onSubmit,
  isLoading = false,
  error,
}: QuestionComposerModalProps) {
  const [options, setOptions] = useState<OptionDraft[]>(() => mapQuestionToOptions(initialQuestion));
  const [optionsError, setOptionsError] = useState('');
  const [didAttemptSubmit, setDidAttemptSubmit] = useState(false);

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: initialQuestion
      ? {
          type: String(initialQuestion.type),
          lessonId: initialQuestion.lessonId ?? '',
          questionText: initialQuestion.questionText,
          passScore: initialQuestion.passScore,
          maxAttempts: initialQuestion.maxAttempts,
          isRequired: initialQuestion.isRequired,
        }
      : {
          type: lessonId ? String(QuizType.PerLesson) : String(QuizType.CourseExam),
          lessonId: lessonId ?? '',
          questionText: '',
          passScore: 60,
          maxAttempts: 3,
          isRequired: true,
        },
  });

  const filledOptions = useMemo(() => options.filter((option) => option.text.trim().length > 0), [options]);
  const correctCount = useMemo(() => filledOptions.filter((option) => option.isCorrect).length, [filledOptions]);

  const canRemoveOption = options.length > 3;
  const hasValidOptions = filledOptions.length >= 3 && correctCount === 1;
  const selectedType = useWatch({ control, name: 'type' });
  const isPerLessonType = selectedType === String(QuizType.PerLesson);
  const needsLessonSelection = mode === 'create' && !lessonId && isPerLessonType;

  const addOption = () => {
    setOptions((prev) => [...prev, { id: crypto.randomUUID(), text: '', isCorrect: false }]);
  };

  const removeOption = (id: string) => {
    if (!canRemoveOption) return;
    setOptions((prev) => {
      const target = prev.find((option) => option.id === id);
      const next = prev.filter((option) => option.id !== id);
      if (target?.isCorrect && next.length > 0) {
        next[0] = { ...next[0], isCorrect: true };
      }
      return next;
    });
  };

  const changeOptionText = (id: string, text: string) => {
    setOptions((prev) => prev.map((option) => (option.id === id ? { ...option, text } : option)));
  };

  const setCorrectOption = (id: string) => {
    setOptions((prev) => prev.map((option) => ({ ...option, isCorrect: option.id === id })));
  };

  const onFormSubmit = async (data: FormValues) => {
    const parsed = formSchema.parse(data);
    setDidAttemptSubmit(true);
    setOptionsError('');

    const chosenLessonId = lessonId ?? (parsed.lessonId?.trim() || null);

    if (parsed.type === String(QuizType.PerLesson) && !chosenLessonId) {
      setOptionsError('Debes seleccionar la lección para esta evaluación.');
      return;
    }

    if (filledOptions.length < 3) {
      setOptionsError('Debes completar al menos 3 opciones con texto.');
      return;
    }

    if (correctCount !== 1) {
      setOptionsError('Debes marcar exactamente 1 opción correcta.');
      return;
    }

    await onSubmit({
      question: {
        type: Number(parsed.type) as 0 | 1,
        questionText: parsed.questionText,
        passScore: parsed.passScore,
        maxAttempts: parsed.maxAttempts,
        isRequired: parsed.isRequired,
        lessonId: parsed.type === String(QuizType.PerLesson) ? chosenLessonId : null,
        courseId: parsed.type === String(QuizType.CourseExam) ? (courseId ?? null) : null,
      },
      options: filledOptions.map((option, index) => ({
        optionId: option.optionId,
        optionText: option.text.trim(),
        isCorrect: option.isCorrect,
        orderIndex: index + 1,
      })),
    });
  };

  return (
    <Modal
      open
      onClose={onClose}
      size="lg"
      title={mode === 'create' ? 'Nueva pregunta' : 'Editar pregunta'}
      description="Pregunta de opción única: el estudiante elige una sola respuesta."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="question-composer-form" disabled={isLoading || !hasValidOptions}>
            {isLoading ? 'Guardando…' : mode === 'create' ? 'Guardar pregunta' : 'Guardar cambios'}
          </Button>
        </>
      }
    >
      <form id="question-composer-form" onSubmit={handleSubmit(onFormSubmit)} noValidate className="flex flex-col gap-5">
        {(mode === 'create' && !lessonId) || mode === 'edit' ? (
          <div className="grid gap-4 md:grid-cols-2">
            <Field
              label="Tipo de evaluación"
              hint={mode === 'edit' ? 'El tipo no se modifica en la edición.' : undefined}
              error={errors.type?.message}
            >
              <Select {...register('type')} disabled={mode === 'edit'}>
                <option value={String(QuizType.PerLesson)}>Quiz de lección</option>
                <option value={String(QuizType.CourseExam)}>Examen final</option>
              </Select>
            </Field>
            {needsLessonSelection && (
              <Field
                label="Lección"
                hint={lessonOptions.length === 0 ? 'Este curso no tiene lecciones para asociar la evaluación.' : undefined}
              >
                <Select {...register('lessonId')}>
                  <option value="">Selecciona una lección</option>
                  {lessonOptions
                    .slice()
                    .sort((a, b) => a.orderIndex - b.orderIndex)
                    .map((lessonOption) => (
                      <option key={lessonOption.id} value={lessonOption.id}>
                        {lessonOption.orderIndex}. {lessonOption.title}
                      </option>
                    ))}
                </Select>
              </Field>
            )}
          </div>
        ) : null}

        <Field label="Texto de la pregunta" hint="Mínimo 10 caracteres." error={errors.questionText?.message}>
          <Textarea rows={3} {...register('questionText')} placeholder="Escribe una pregunta clara para el estudiante" />
        </Field>

        <div className="grid gap-4 md:grid-cols-2">
          <Field label="Puntaje mínimo para aprobar (%)" hint="Entre 0 y 100." error={errors.passScore?.message}>
            <Input type="number" min={0} max={100} {...register('passScore')} />
          </Field>
          <Field label="Intentos máximos" hint="Entre 1 y 10." error={errors.maxAttempts?.message}>
            <Input type="number" min={1} max={10} {...register('maxAttempts')} />
          </Field>
        </div>

        <Controller
          control={control}
          name="isRequired"
          render={({ field }) => (
            <Switch
              className="rounded-xl border border-border px-4 py-3"
              checked={field.value}
              onCheckedChange={field.onChange}
              label="Requerida"
              description="Cuenta para aprobar la evaluación."
            />
          )}
        />

        <div>
          <div className="mb-2 flex items-center justify-between">
            <Label className="mb-0">Opciones</Label>
            <span className="text-xs text-subtle-foreground">Marca la opción correcta</span>
          </div>
          <div role="radiogroup" aria-label="Opción correcta" className="flex flex-col gap-2">
            {options.map((option, index) => {
              const label = `Opción ${index + 1}`;
              return (
                <div key={option.id} className="flex items-center gap-3">
                  <input
                    type="radio"
                    name="correctOption"
                    checked={option.isCorrect}
                    onChange={() => setCorrectOption(option.id)}
                    aria-label={`Marcar ${label.toLowerCase()} como correcta`}
                    className="size-5 shrink-0 cursor-pointer appearance-none rounded-full border border-border-strong transition-colors checked:border-[6px] checked:border-success focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  />
                  <Input
                    value={option.text}
                    onChange={(e) => changeOptionText(option.id, e.target.value)}
                    placeholder={label}
                    aria-label={label}
                    aria-invalid={didAttemptSubmit && option.text.trim().length === 0 ? true : undefined}
                    className="flex-1"
                  />
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label={`Quitar ${label.toLowerCase()}`}
                    disabled={!canRemoveOption}
                    onClick={() => removeOption(option.id)}
                  >
                    <XIcon />
                  </Button>
                </div>
              );
            })}
          </div>
          <div className="mt-3 flex flex-wrap items-center justify-between gap-3">
            <Button variant="ghost" size="sm" onClick={addOption}>
              <PlusIcon /> Agregar opción
            </Button>
            <p className={cn('text-xs', hasValidOptions ? 'text-success-foreground' : 'text-warning-foreground')}>
              {hasValidOptions
                ? `Listo: ${filledOptions.length} opciones y 1 correcta.`
                : 'Necesitas al menos 3 opciones y exactamente 1 correcta.'}
            </p>
          </div>
        </div>

        {(optionsError || error) && <Alert tone="danger">{optionsError || error}</Alert>}
      </form>
    </Modal>
  );
}
