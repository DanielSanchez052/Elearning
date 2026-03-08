import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { QuizType, type QuizQuestion } from '@/types/quiz.types';

const formSchema = z.object({
  type: z.enum([String(QuizType.PerLesson), String(QuizType.CourseExam)]),
  lessonId: z.string().optional(),
  questionText: z
    .string()
    .min(10, 'La pregunta debe tener al menos 10 caracteres'),
  passScore: z.coerce
    .number()
    .min(0, 'El puntaje debe ser mayor o igual a 0')
    .max(100, 'El puntaje no puede exceder 100'),
  maxAttempts: z.coerce
    .number()
    .min(1, 'Debe permitir al menos 1 intento')
    .max(10, 'Maximo 10 intentos'),
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

  const hasCorrect = ordered.some((option) => option.isCorrect);
  if (!hasCorrect) {
    ordered[0] = { ...ordered[0], isCorrect: true };
  }

  while (ordered.length < 3) {
    ordered.push({
      id: crypto.randomUUID(),
      text: '',
      isCorrect: false,
    });
  }

  return ordered;
}

export function QuestionComposerModal({
  lessonId,
  courseId,
  lessonOptions = [],
  mode = 'create',
  initialQuestion,
  onClose,
  onSubmit,
  isLoading = false,
}: QuestionComposerModalProps) {
  const [options, setOptions] = useState<OptionDraft[]>(() =>
    mapQuestionToOptions(initialQuestion)
  );
  const [optionsError, setOptionsError] = useState('');
  const [didAttemptSubmit, setDidAttemptSubmit] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    watch,
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

  useEffect(() => {
    reset(
      initialQuestion
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
          }
    );
    setOptions(mapQuestionToOptions(initialQuestion));
    setOptionsError('');
    setDidAttemptSubmit(false);
  }, [initialQuestion, lessonId, courseId, reset]);

  const filledOptions = useMemo(
    () => options.filter((option) => option.text.trim().length > 0),
    [options]
  );

  const correctCount = useMemo(
    () => filledOptions.filter((option) => option.isCorrect).length,
    [filledOptions]
  );

  const canRemoveOption = options.length > 3;
  const hasValidOptions = filledOptions.length >= 3 && correctCount === 1;
  const selectedType = watch('type');
  const isPerLessonType = selectedType === String(QuizType.PerLesson);
  const needsLessonSelection = mode === 'create' && !lessonId && isPerLessonType;

  const addOption = () => {
    setOptions((prev) => [
      ...prev,
      { id: crypto.randomUUID(), text: '', isCorrect: false },
    ]);
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
    setOptions((prev) =>
      prev.map((option) => (option.id === id ? { ...option, text } : option))
    );
  };

  const setCorrectOption = (id: string) => {
    setOptions((prev) =>
      prev.map((option) => ({ ...option, isCorrect: option.id === id }))
    );
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
      setOptionsError('Debes marcar exactamente 1 opcion correcta.');
      return;
    }

    await onSubmit({
      question: {
        type: Number(parsed.type) as 0 | 1,
        questionText: parsed.questionText,
        passScore: parsed.passScore,
        maxAttempts: parsed.maxAttempts,
        isRequired: parsed.isRequired,
        lessonId:
          parsed.type === String(QuizType.PerLesson) ? chosenLessonId : null,
        courseId:
          parsed.type === String(QuizType.CourseExam) ? (courseId ?? null) : null,
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
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
      onClick={onClose}
    >
      <div className="w-full max-w-4xl rounded-2xl border border-white/[0.08] bg-[#111118] shadow-2xl">
        <div className="flex items-center justify-between border-b border-white/[0.06] px-6 py-4">
          <div>
            <h2 className="text-lg font-semibold text-white">Nueva pregunta</h2>
            <p className="mt-1 text-sm text-zinc-400">
              {mode === 'create'
                ? 'Crea la pregunta y define sus respuestas en un solo flujo'
                : 'Edita la pregunta y administra sus respuestas en un solo flujo'}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-2 text-zinc-500 transition hover:bg-white/[0.06] hover:text-white"
            aria-label="Cerrar modal"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <form
          onSubmit={handleSubmit(onFormSubmit)}
          className="space-y-6 px-6 py-5"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            <div className="md:col-span-2">
              <label className="mb-2 block text-sm font-medium text-white">Tipo de quiz</label>
              <select
                {...register('type')}
                disabled={mode === 'edit'}
                className="w-full rounded-xl border border-white/[0.08] bg-white/[0.03] px-3 py-2.5 text-sm text-white focus:border-indigo-500/60 focus:outline-none"
              >
                <option value={String(QuizType.PerLesson)}>Por leccion</option>
                <option value={String(QuizType.CourseExam)}>Examen final</option>
              </select>
              {errors.type && (
                <p className="mt-1 text-xs text-red-400">{errors.type.message}</p>
              )}
              {mode === 'edit' && (
                <p className="mt-1 text-xs text-zinc-500">
                  El tipo de quiz no se modifica en la edicion.
                </p>
              )}
            </div>

            {needsLessonSelection && (
              <div className="md:col-span-2">
                <label className="mb-2 block text-sm font-medium text-white">Lección</label>
                <select
                  {...register('lessonId')}
                  className="w-full rounded-xl border border-white/[0.08] bg-white/[0.03] px-3 py-2.5 text-sm text-white focus:border-indigo-500/60 focus:outline-none"
                >
                  <option value="">Selecciona una lección</option>
                  {lessonOptions
                    .slice()
                    .sort((a, b) => a.orderIndex - b.orderIndex)
                    .map((lessonOption) => (
                      <option key={lessonOption.id} value={lessonOption.id}>
                        #{lessonOption.orderIndex} - {lessonOption.title}
                      </option>
                    ))}
                </select>
                {lessonOptions.length === 0 && (
                  <p className="mt-1 text-xs text-amber-400">
                    Este curso no tiene lecciones disponibles para asociar esta evaluación.
                  </p>
                )}
              </div>
            )}

            <div>
              <label className="mb-2 block text-sm font-medium text-white">Puntaje minimo</label>
              <input
                type="number"
                min={0}
                max={100}
                {...register('passScore')}
                className="w-full rounded-xl border border-white/[0.08] bg-white/[0.03] px-3 py-2.5 text-sm text-white focus:border-indigo-500/60 focus:outline-none"
              />
              {errors.passScore && (
                <p className="mt-1 text-xs text-red-400">{errors.passScore.message}</p>
              )}
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-white">Intentos maximos</label>
              <input
                type="number"
                min={1}
                max={10}
                {...register('maxAttempts')}
                className="w-full rounded-xl border border-white/[0.08] bg-white/[0.03] px-3 py-2.5 text-sm text-white focus:border-indigo-500/60 focus:outline-none"
              />
              {errors.maxAttempts && (
                <p className="mt-1 text-xs text-red-400">{errors.maxAttempts.message}</p>
              )}
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-white">Pregunta</label>
            <textarea
              rows={3}
              {...register('questionText')}
              placeholder="Escribe una pregunta clara para el estudiante"
              className="w-full resize-none rounded-xl border border-white/[0.08] bg-white/[0.03] px-3 py-2.5 text-sm text-white placeholder-zinc-600 focus:border-indigo-500/60 focus:outline-none"
            />
            {errors.questionText && (
              <p className="mt-1 text-xs text-red-400">{errors.questionText.message}</p>
            )}
          </div>

          <div className="flex items-center gap-2">
            <input
              id="isRequired"
              type="checkbox"
              {...register('isRequired')}
              className="h-4 w-4 rounded accent-indigo-600"
            />
            <label htmlFor="isRequired" className="text-sm text-zinc-300">
              Pregunta obligatoria
            </label>
          </div>

          <div className="rounded-2xl border border-white/[0.06] bg-white/[0.02] p-4">
            <div className="mb-3 flex items-center justify-between">
              <div>
                <h3 className="text-sm font-semibold text-white">Respuestas</h3>
                <p className="text-xs text-zinc-500">
                  Minimo 3 opciones y una sola marcada como correcta
                </p>
                <p className="mt-1 text-xs text-zinc-500">
                  {filledOptions.length} con texto - {correctCount} correcta
                </p>
              </div>
              <button
                type="button"
                onClick={addOption}
                className="rounded-lg border border-white/[0.1] px-3 py-1.5 text-xs text-zinc-300 transition hover:bg-white/[0.06] hover:text-white"
              >
                + Agregar opcion
              </button>
            </div>

            <div className="space-y-2">
              {options.map((option, index) => (
                <div
                  key={option.id}
                  className={`grid grid-cols-[28px_1fr_auto_auto] items-center gap-3 rounded-xl border bg-[#0f0f16] px-3 py-2 ${
                    didAttemptSubmit && option.text.trim().length === 0
                      ? 'border-red-500/30'
                      : 'border-white/[0.06]'
                  }`}
                >
                  <span className="text-xs font-semibold text-zinc-500">
                    {String.fromCharCode(65 + index)}
                  </span>
                  <input
                    value={option.text}
                    onChange={(e) => changeOptionText(option.id, e.target.value)}
                    placeholder={`Respuesta ${index + 1}`}
                    className="w-full border-none bg-transparent text-sm text-white placeholder-zinc-600 focus:outline-none"
                  />

                  <label className="flex items-center gap-2 text-xs text-zinc-400">
                    <input
                      type="radio"
                      name="correctOption"
                      checked={option.isCorrect}
                      onChange={() => setCorrectOption(option.id)}
                      className="h-4 w-4 accent-emerald-500"
                    />
                    Correcta
                  </label>

                  <button
                    type="button"
                    disabled={!canRemoveOption}
                    onClick={() => removeOption(option.id)}
                    className="rounded-md px-2 py-1 text-xs text-red-400 transition hover:bg-red-500/10 disabled:cursor-not-allowed disabled:opacity-30"
                  >
                    Eliminar
                  </button>
                </div>
              ))}
            </div>

            {optionsError && <p className="mt-3 text-xs text-red-400">{optionsError}</p>}
          </div>

          <div className="flex items-center justify-end gap-3 border-t border-white/[0.06] pt-4">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg px-4 py-2 text-sm text-zinc-300 transition hover:bg-white/[0.06] hover:text-white"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isLoading || !hasValidOptions}
              className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:opacity-60"
            >
              {isLoading
                ? 'Guardando...'
                : mode === 'create'
                  ? 'Guardar pregunta'
                  : 'Guardar cambios'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
