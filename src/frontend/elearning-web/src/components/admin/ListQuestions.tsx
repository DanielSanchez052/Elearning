import { type QuizQuestion } from '@/types/quiz.types';
import { Badge, Button, Card } from '@/components/ui';
import { CheckCircleIcon, PencilIcon, TrashIcon, XCircleIcon } from '@/components/ui/icons';
import { cn } from '@/lib/utils';

interface QuestionAction {
  questionId: string;
  action: 'edit' | 'delete';
}

interface ListQuestionsProps {
  questions: QuizQuestion[];
  onAction?: (payload: QuestionAction) => void;
  isLoading?: boolean;
}

/** Preguntas de una evaluación (admin): texto, requerida y opciones con la correcta resaltada. */
export function ListQuestions({ questions, onAction, isLoading = false }: ListQuestionsProps) {
  const ordered = [...questions].sort((a, b) => a.orderIndex - b.orderIndex);

  return (
    <ol className="flex flex-col gap-4">
      {ordered.map((question, index) => {
        const options = [...question.options].sort((a, b) => a.orderIndex - b.orderIndex);
        return (
          <li key={question.id}>
            <Card>
              <div className="flex items-start gap-4">
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="text-xs font-medium text-subtle-foreground">Pregunta {index + 1}</span>
                    {question.isRequired && <Badge>Requerida</Badge>}
                  </div>
                  <p className="mt-1 text-base font-medium text-foreground">{question.questionText}</p>
                </div>
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label={`Editar pregunta ${index + 1}`}
                    title="Editar pregunta y opciones"
                    disabled={isLoading}
                    onClick={() => onAction?.({ questionId: question.id, action: 'edit' })}
                  >
                    <PencilIcon />
                  </Button>
                  <Button
                    variant="danger"
                    size="icon"
                    aria-label={`Eliminar pregunta ${index + 1}`}
                    title="Eliminar"
                    disabled={isLoading}
                    onClick={() => onAction?.({ questionId: question.id, action: 'delete' })}
                  >
                    <TrashIcon />
                  </Button>
                </div>
              </div>
              <ul className="mt-4 grid gap-2 md:grid-cols-2">
                {options.map((option) => (
                  <li
                    key={option.id}
                    className={cn(
                      'flex items-center gap-3 rounded-xl border px-4 py-2.5 text-sm',
                      option.isCorrect ? 'border-success/30 bg-success/10 text-success-foreground' : 'border-border text-foreground-soft'
                    )}
                  >
                    {option.isCorrect ? (
                      <CheckCircleIcon className="size-4 shrink-0" aria-hidden="true" />
                    ) : (
                      <XCircleIcon className="size-4 shrink-0 text-subtle-foreground" aria-hidden="true" />
                    )}
                    <span className="flex-1">{option.optionText}</span>
                    {option.isCorrect && <span className="text-xs font-medium">Correcta</span>}
                  </li>
                ))}
              </ul>
            </Card>
          </li>
        );
      })}
    </ol>
  );
}
