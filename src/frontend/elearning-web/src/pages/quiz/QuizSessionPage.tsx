import { useEffect, useMemo, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import {
  useCourseExam,
  useCourseExamResults,
  useLessonQuizzes,
  useLessonResults,
  useSubmitCourseExam,
  useSubmitLessonQuiz,
} from '@/hooks/quizzes';
import { useMarkLessonComplete } from '@/hooks/useEnrollments';
import type { QuizQuestion, QuizResultDto } from '@/types/quiz.types';
import { useQuizSessionStore } from '@/store/quizSessionStore';

function formatTime(totalSeconds: number) {
  const mins = Math.floor(totalSeconds / 60)
    .toString()
    .padStart(2, '0');
  const secs = Math.floor(totalSeconds % 60)
    .toString()
    .padStart(2, '0');
  return `${mins}:${secs}`;
}

function getDurationByQuestions(count: number) {
  return Math.max(120, count * 45);
}

export default function QuizSessionPage() {
  const { id: courseId, lessonId } = useParams<{ id: string; lessonId?: string }>();
  const isLessonQuiz = Boolean(lessonId);

  const lessonQuery = useLessonQuizzes(lessonId ?? '', isLessonQuiz);
  const examQuery = useCourseExam(courseId ?? '', !isLessonQuiz && Boolean(courseId));

  const lessonResultsQuery = useLessonResults(lessonId ?? '', isLessonQuiz);
  const examResultsQuery = useCourseExamResults(courseId ?? '', !isLessonQuiz && Boolean(courseId));

  const submitLessonQuiz = useSubmitLessonQuiz();
  const submitCourseExam = useSubmitCourseExam();
  const markLessonComplete = useMarkLessonComplete();

  const questionsRaw = isLessonQuiz ? lessonQuery.data : examQuery.data;
  const questions = useMemo(
    () => [...(questionsRaw ?? [])].sort((a, b) => a.orderIndex - b.orderIndex),
    [questionsRaw]
  );

  const attempts = isLessonQuiz
    ? lessonResultsQuery.data ?? []
    : examResultsQuery.data ?? [];

  const maxAttempts = questions[0]?.maxAttempts ?? 1;
  const attemptsUsed = attempts.length;

  const [result, setResult] = useState<QuizResultDto | null>(null);
  const [submissionError, setSubmissionError] = useState('');
  const [progressFeedback, setProgressFeedback] = useState('');
  const timeoutSubmitRef = useRef(false);

  const {
    selectedByQuestion,
    durationSec,
    timeLeftSec,
    isRunning,
    startSession,
    resetSession,
    tick,
    setAnswer,
    stop,
  } = useQuizSessionStore();

  useEffect(() => {
    if (!questions.length) return;
    startSession(getDurationByQuestions(questions.length));
    setResult(null);
    setSubmissionError('');
    timeoutSubmitRef.current = false;
    return () => resetSession();
  }, [questions.length, startSession, resetSession]);

  useEffect(() => {
    if (!isRunning || result) return;
    const id = window.setInterval(() => tick(), 1000);
    return () => window.clearInterval(id);
  }, [isRunning, result, tick]);

  const selectedCount = useMemo(
    () => questions.filter((q) => Boolean(selectedByQuestion[q.id])).length,
    [questions, selectedByQuestion]
  );

  const computedAttemptsUsed = attemptsUsed + (result ? 1 : 0);
  const attemptsLeft = Math.max(0, maxAttempts - computedAttemptsUsed);

  const canRetry = Boolean(result && !result.isPassed && attemptsLeft > 0);

  const buildSelectedOptionIds = (fillMissingWithFirstOption: boolean) => {
    const ids: string[] = [];

    for (const question of questions) {
      const selectedId = selectedByQuestion[question.id];
      if (selectedId) {
        ids.push(selectedId);
        continue;
      }

      if (!fillMissingWithFirstOption) {
        return null;
      }

      if (!question.options.length) {
        return null;
      }

      ids.push(question.options[0].id);
    }

    return ids;
  };

  const submitQuiz = async (fillMissingWithFirstOption = false) => {
    if (!courseId) return;
    setSubmissionError('');

    if (attemptsLeft <= 0) {
      setSubmissionError('Ya agotaste el número máximo de intentos para esta evaluación.');
      return;
    }

    const selectedOptionIds = buildSelectedOptionIds(fillMissingWithFirstOption);
    if (!selectedOptionIds) {
      setSubmissionError('Debes responder todas las preguntas antes de enviar.');
      return;
    }

    try {
      stop();
      setProgressFeedback('');
      const response = isLessonQuiz
        ? await submitLessonQuiz.mutateAsync({
            lessonId: lessonId ?? '',
            selectedOptionIds,
          })
        : await submitCourseExam.mutateAsync({
            courseId,
            selectedOptionIds,
          });
      setResult(response.data);

      if (isLessonQuiz && response.data.isPassed && lessonId) {
        try {
          const completeResponse = await markLessonComplete.mutateAsync({
            courseId,
            lessonId,
          });

          setProgressFeedback(
            completeResponse.data.lessonWasAlreadyComplete
              ? 'Esta lección ya estaba marcada como completada.'
              : 'Lección marcada como completada en tu progreso.'
          );
        } catch {
          setProgressFeedback(
            'Aprobaste la evaluación, pero no se pudo sincronizar el progreso de la lección.'
          );
        }
      }
    } catch (error: any) {
      const message =
        error?.response?.data?.message ||
        error?.response?.data?.error ||
        'No se pudo enviar la evaluación. Intenta nuevamente.';
      setSubmissionError(message);
    }
  };

  useEffect(() => {
    if (
      durationSec <= 0 ||
      timeLeftSec > 0 ||
      !questions.length ||
      result ||
      timeoutSubmitRef.current
    ) {
      return;
    }

    timeoutSubmitRef.current = true;
    submitQuiz(true);
  }, [durationSec, timeLeftSec, questions.length, result]);

  const startRetry = () => {
    startSession(durationSec || getDurationByQuestions(questions.length));
    setResult(null);
    setSubmissionError('');
    timeoutSubmitRef.current = false;
  };

  const isLoading = lessonQuery.isLoading || examQuery.isLoading;

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[#0a0a0f] flex items-center justify-center">
        <p className="text-zinc-400">Cargando evaluación...</p>
      </div>
    );
  }

  if (!questions.length) {
    return (
      <div className="min-h-screen bg-[#0a0a0f] flex items-center justify-center p-6">
        <div className="w-full max-w-xl rounded-2xl border border-white/[0.08] bg-[#111118] p-8 text-center">
          <p className="text-zinc-300 mb-2">No hay preguntas configuradas para esta evaluación.</p>
          <Link to={`/courses/${courseId}`} className="text-indigo-400 hover:text-indigo-300 text-sm">
            Volver al curso
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#0a0a0f] text-white">
      <div className="max-w-4xl mx-auto px-6 py-8">
        <div className="mb-6 flex items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-semibold text-white">
              {isLessonQuiz ? 'Evaluación de lección' : 'Examen final del curso'}
            </h1>
            <p className="text-sm text-zinc-400 mt-1">
              {selectedCount} / {questions.length} preguntas respondidas
            </p>
            <p className="text-xs text-zinc-500 mt-1">
              Intentos restantes: {attemptsLeft} de {maxAttempts}
            </p>
          </div>

          <div
            className={`rounded-xl border px-4 py-2 text-sm font-semibold ${
              timeLeftSec <= 30
                ? 'border-red-500/40 bg-red-500/10 text-red-300'
                : 'border-indigo-500/30 bg-indigo-500/10 text-indigo-300'
            }`}
          >
            {formatTime(timeLeftSec)}
          </div>
        </div>

        {result ? (
          <div className="rounded-2xl border border-white/[0.08] bg-[#111118] p-6">
            <div className="flex items-center gap-3">
              <div
                className={`h-10 w-10 rounded-xl flex items-center justify-center ${
                  result.isPassed ? 'bg-emerald-500/20 text-emerald-300' : 'bg-red-500/20 text-red-300'
                }`}
              >
                {result.isPassed ? '✓' : '!'}
              </div>
              <div>
                <h2 className="text-xl font-semibold">
                  {result.isPassed ? 'Aprobaste la evaluación' : 'No aprobaste esta vez'}
                </h2>
                <p className="text-sm text-zinc-400">{result.feedback}</p>
              </div>
            </div>

            <div className="mt-6 grid grid-cols-2 md:grid-cols-4 gap-3">
              <ResultStat label="Puntaje" value={`${Number(result.score).toFixed(1)}%`} />
              <ResultStat label="Mínimo" value={`${Number(result.passScore).toFixed(0)}%`} />
              <ResultStat label="Correctas" value={`${result.correctAnswers}/${result.totalQuestions}`} />
              <ResultStat label="Intento" value={`${computedAttemptsUsed}/${maxAttempts}`} />
            </div>

            <div className="mt-6 flex flex-wrap gap-3">
              {canRetry && (
                <button
                  onClick={startRetry}
                  className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 transition"
                >
                  Reintentar evaluación
                </button>
              )}
              <Link
                to={`/courses/${courseId}`}
                className="rounded-lg border border-white/[0.12] px-4 py-2 text-sm text-zinc-300 hover:bg-white/[0.04] transition"
              >
                Volver al curso
              </Link>
            </div>

            {progressFeedback && (
              <p className="mt-4 rounded-lg border border-emerald-500/30 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-300">
                {progressFeedback}
              </p>
            )}
          </div>
        ) : (
          <>
            <div className="space-y-4">
              {questions.map((question, index) => (
                <QuestionCard
                  key={question.id}
                  index={index}
                  question={question}
                  selectedOptionId={selectedByQuestion[question.id]}
                  onSelectOption={(optionId) => setAnswer(question.id, optionId)}
                />
              ))}
            </div>

            {submissionError && (
              <p className="mt-4 rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2 text-sm text-red-300">
                {submissionError}
              </p>
            )}

            <div className="mt-6 flex items-center justify-between">
              <Link to={`/courses/${courseId}`} className="text-sm text-zinc-400 hover:text-zinc-300">
                Cancelar y volver
              </Link>
              <button
                onClick={() => submitQuiz(false)}
                disabled={submitLessonQuiz.isPending || submitCourseExam.isPending || attemptsLeft <= 0}
                className="rounded-lg bg-emerald-600 px-5 py-2.5 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50 transition"
              >
                {submitLessonQuiz.isPending || submitCourseExam.isPending
                  ? 'Enviando...'
                  : 'Enviar evaluación'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

function QuestionCard({
  index,
  question,
  selectedOptionId,
  onSelectOption,
}: {
  index: number;
  question: QuizQuestion;
  selectedOptionId?: string;
  onSelectOption: (optionId: string) => void;
}) {
  const options = [...question.options].sort((a, b) => a.orderIndex - b.orderIndex);

  return (
    <div className="rounded-2xl border border-white/[0.08] bg-[#111118] p-5">
      <p className="text-xs text-zinc-500 mb-2">Pregunta {index + 1}</p>
      <h3 className="text-sm md:text-base text-white font-medium mb-4">{question.questionText}</h3>

      <div className="space-y-2">
        {options.map((option, optIdx) => {
          const isActive = selectedOptionId === option.id;
          return (
            <button
              key={option.id}
              type="button"
              onClick={() => onSelectOption(option.id)}
              className={`w-full rounded-xl border px-3 py-2 text-left transition ${
                isActive
                  ? 'border-indigo-500/50 bg-indigo-500/15 text-white'
                  : 'border-white/[0.08] bg-white/[0.02] text-zinc-300 hover:bg-white/[0.05]'
              }`}
            >
              <span className="text-xs text-zinc-500 mr-2">{String.fromCharCode(65 + optIdx)}.</span>
              <span className="text-sm">{option.optionText}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}

function ResultStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-white/[0.08] bg-white/[0.02] p-3">
      <p className="text-xs text-zinc-500">{label}</p>
      <p className="mt-1 text-base font-semibold text-white">{value}</p>
    </div>
  );
}
