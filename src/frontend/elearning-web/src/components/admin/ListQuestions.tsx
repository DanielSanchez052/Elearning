import { type QuizQuestion, QuizType } from '@/types/quiz.types';

interface QuestionAction {
  questionId: string;
  action: 'edit' | 'delete';
}

interface ListQuestionsProps {
  questions: QuizQuestion[];
  onAction?: (payload: QuestionAction) => void;
  isLoading?: boolean;
}

export function ListQuestions({
  questions,
  onAction,
  isLoading = false,
}: ListQuestionsProps) {
  if (questions.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="text-zinc-400 mb-3">
          <svg
            className="w-12 h-12 mx-auto opacity-30"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>
        <p className="text-zinc-400">No hay preguntas creadas</p>
        <p className="text-zinc-500 text-sm mt-1">
          Crea tu primera pregunta para comenzar
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {questions.map((question) => (
        <div
          key={question.id}
          className="bg-white/[0.03] border border-white/[0.06] rounded-xl p-4 hover:bg-white/[0.05] transition"
        >
          <div className="flex items-start justify-between gap-4">
            <div className="flex-1">
              <div className="flex items-center gap-3 mb-2">
                <span className="text-sm px-2 py-1 bg-indigo-600/20 text-indigo-300 rounded-md">
                  {question.type === QuizType.PerLesson
                    ? 'Por Lección'
                    : 'Examen Final'}
                </span>
                {question.isRequired && (
                  <span className="text-xs px-2 py-1 bg-red-600/20 text-red-300 rounded-md">
                    Obligatoria
                  </span>
                )}
              </div>

              <p className="text-white font-medium mb-2">
                {question.questionText}
              </p>

              <div className="flex flex-wrap gap-4 text-sm text-zinc-400">
                <div>
                  <span className="text-zinc-500">Puntaje Mínimo:</span>{' '}
                  {question.passScore}%
                </div>
                <div>
                  <span className="text-zinc-500">Intentos:</span>{' '}
                  {question.maxAttempts}
                </div>
                <div>
                  <span className="text-zinc-500">Opciones:</span>{' '}
                  {question.options.length}
                </div>
              </div>
            </div>

            <div className="flex gap-2 flex-shrink-0">
              <button
                onClick={() =>
                  onAction?.({ questionId: question.id, action: 'edit' })
                }
                disabled={isLoading}
                title="Editar pregunta y opciones"
                className="px-3 py-2 text-zinc-400 hover:bg-white/[0.05] rounded-lg transition disabled:opacity-50"
              >
                <svg
                  className="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                  />
                </svg>
              </button>

              <button
                onClick={() =>
                  onAction?.({ questionId: question.id, action: 'delete' })
                }
                disabled={isLoading}
                title="Eliminar"
                className="px-3 py-2 text-red-400 hover:bg-red-500/10 rounded-lg transition disabled:opacity-50"
              >
                <svg
                  className="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                  />
                </svg>
              </button>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
