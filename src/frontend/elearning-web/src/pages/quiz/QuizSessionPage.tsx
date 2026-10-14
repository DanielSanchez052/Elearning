import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import {
  useCourseExamResults,
  useLessonQuizzes,
  useLessonResults,
  useStartCourseExam,
  useSubmitCourseExam,
  useSubmitLessonQuiz,
} from '@/hooks/quizzes';
import { useMarkLessonComplete } from '@/hooks/useEnrollments';
import { useCourseDetail } from '@/hooks/useCourses';
import type { QuizAttemptDto, QuizQuestion, QuizResultDto, StartCourseExamResult } from '@/types/quiz.types';
import { useQuizSessionStore } from '@/store/quizSessionStore';
import {
  Alert,
  Badge,
  Button,
  Card,
  ProgressBar,
  RadioCard,
  Skeleton,
  StateBlock,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui';
import { ArrowLeftIcon, ArrowRightIcon, ClockIcon, ListChecksIcon, TrophyIcon, XCircleIcon } from '@/components/ui/icons';
import { ButtonLink } from '@/components/ButtonLink';
import { formatDate } from '@/lib/format';
import { cn } from '@/lib/utils';

function formatTime(totalSeconds: number) {
  const mins = Math.floor(totalSeconds / 60)
    .toString()
    .padStart(2, '0');
  const secs = Math.floor(totalSeconds % 60)
    .toString()
    .padStart(2, '0');
  return `${mins}:${secs}`;
}

// Lesson quizzes keep their original pace.
function getLessonQuizDurationSec(count: number) {
  return Math.max(120, count * 45);
}

// Course final exam. The badge "Velocista" is earned by passing the first attempt in
// under 10 minutes (measured server-side from /exam/start to submission). The exam's own
// countdown must therefore always be longer than those 10 minutes; otherwise the
// auto-submit would land inside the badge window and any pass of a short exam would
// qualify by construction. A 15-minute floor leaves a 5-minute margin even for tiny exams,
// and 60 s per question lets larger exams grow past it (the old rate was 45 s).
const COURSE_EXAM_MIN_SEC = 15 * 60;
const COURSE_EXAM_SEC_PER_QUESTION = 60;

function getCourseExamDurationSec(count: number) {
  return Math.max(COURSE_EXAM_MIN_SEC, count * COURSE_EXAM_SEC_PER_QUESTION);
}

function getDurationByQuestions(count: number, isLessonQuiz: boolean) {
  return isLessonQuiz ? getLessonQuizDurationSec(count) : getCourseExamDurationSec(count);
}

const START_EXAM_ERROR = 'No se pudo iniciar el examen. Intenta nuevamente.';

function getErrorMessage(error: unknown, fallback: string) {
  const data = (error as { response?: { data?: { message?: string; error?: string } } } | null)
    ?.response?.data;
  return data?.message || data?.error || fallback;
}

/** Segundos ya transcurridos de un intento de examen según el reloj del servidor. */
function examElapsedSec(session: StartCourseExamResult) {
  return Math.max(0, (Date.parse(session.serverNow) - Date.parse(session.startedAt)) / 1000);
}

export default function QuizSessionPage() {
  const { id: courseId, lessonId } = useParams<{ id: string; lessonId?: string }>();
  const isLessonQuiz = Boolean(lessonId);
  const navigate = useNavigate();

  const courseQuery = useCourseDetail(courseId ?? '');
  const lessonQuery = useLessonQuizzes(lessonId ?? '', isLessonQuiz);

  const lessonResultsQuery = useLessonResults(lessonId ?? '', isLessonQuiz);
  const examResultsQuery = useCourseExamResults(courseId ?? '', !isLessonQuiz && Boolean(courseId));

  const submitLessonQuiz = useSubmitLessonQuiz();
  const submitCourseExam = useSubmitCourseExam();
  const startCourseExam = useStartCourseExam();
  const markLessonComplete = useMarkLessonComplete();

  const [result, setResult] = useState<QuizResultDto | null>(null);
  const [submissionError, setSubmissionError] = useState('');
  const [progressFeedback, setProgressFeedback] = useState('');
  const timeoutSubmitRef = useRef(false);

  // Course exams are started server-side: the response carries the questions and the
  // attempt's startedAt. Calling start again while the attempt is open resumes it with
  // the original startedAt, so a reload never grants extra time.
  const [examSession, setExamSession] = useState<StartCourseExamResult | null>(null);
  const [startError, setStartError] = useState('');
  const { mutateAsync: startCourseExamAsync } = startCourseExam;

  const applyStartResult = useCallback(
    (outcome: Promise<StartCourseExamResult>, isCancelled: () => boolean = () => false) =>
      outcome.then(
        (session) => {
          if (isCancelled()) return;
          // A new (or resumed) attempt starts clean.
          setStartError('');
          setResult(null);
          setSubmissionError('');
          setExamSession(session);
        },
        (error) => {
          if (isCancelled()) return;
          setStartError(getErrorMessage(error, START_EXAM_ERROR));
        }
      ),
    []
  );

  // Start (or resume) on page open. Duplicate calls (e.g. StrictMode) are safe:
  // the backend returns the same open session.
  useEffect(() => {
    if (isLessonQuiz || !courseId) return;
    let cancelled = false;
    applyStartResult(startCourseExamAsync(courseId), () => cancelled);
    return () => {
      cancelled = true;
    };
  }, [isLessonQuiz, courseId, startCourseExamAsync, applyStartResult]);

  const questionsRaw = isLessonQuiz ? lessonQuery.data : examSession?.questions;
  const questions = useMemo(
    () => [...(questionsRaw ?? [])].sort((a, b) => a.orderIndex - b.orderIndex),
    [questionsRaw]
  );

  const attempts = isLessonQuiz ? lessonResultsQuery.data ?? [] : examResultsQuery.data ?? [];

  const maxAttempts = questions[0]?.maxAttempts ?? 1;
  const passScore = questions[0]?.passScore;
  const attemptsUsed = attempts.length;

  // Resumed an abandoned exam attempt after its countdown already ran out. The time
  // limit is not a hard server cutoff, and an abandoned attempt must not be burned
  // by an auto-submit the student never saw: let them answer and submit manually.
  const timeExpiredOnLoad = useMemo(
    () =>
      !isLessonQuiz &&
      examSession !== null &&
      questions.length > 0 &&
      getCourseExamDurationSec(questions.length) - examElapsedSec(examSession) <= 0,
    [isLessonQuiz, examSession, questions.length]
  );

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
    const duration = getDurationByQuestions(questions.length, isLessonQuiz);
    timeoutSubmitRef.current = timeExpiredOnLoad;

    if (isLessonQuiz || !examSession) {
      startSession(duration);
    } else {
      // Elapsed time uses the server's clock on both ends (serverNow - startedAt),
      // so a skewed device clock cannot shrink or stretch the countdown.
      startSession(duration, { remainingSec: duration - examElapsedSec(examSession) });
    }

    return () => resetSession();
  }, [questions.length, isLessonQuiz, examSession, timeExpiredOnLoad, startSession, resetSession]);

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
      window.scrollTo({ top: 0 });

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
          setProgressFeedback('Aprobaste la evaluación, pero no se pudo sincronizar el progreso de la lección.');
        }
      }
    } catch (error) {
      setSubmissionError(getErrorMessage(error, 'No se pudo enviar la evaluación. Intenta nuevamente.'));
    }
  };

  // The interval callback always calls the latest submitQuiz (it closes over current answers).
  const submitQuizRef = useRef(submitQuiz);
  useEffect(() => {
    submitQuizRef.current = submitQuiz;
  });

  // Countdown. When it reaches zero, auto-submit once, filling unanswered questions.
  useEffect(() => {
    if (!isRunning || result) return;
    const id = window.setInterval(() => {
      tick();
      const { timeLeftSec: left, durationSec: total } = useQuizSessionStore.getState();
      if (total > 0 && left <= 0 && !timeoutSubmitRef.current) {
        timeoutSubmitRef.current = true;
        submitQuizRef.current(true);
      }
    }, 1000);
    return () => window.clearInterval(id);
  }, [isRunning, result, tick]);

  const startRetry = () => {
    if (!isLessonQuiz) {
      // Opens the next attempt server-side; the new session resets the timer and result.
      if (courseId) applyStartResult(startCourseExamAsync(courseId));
      return;
    }
    startSession(durationSec || getDurationByQuestions(questions.length, true));
    setResult(null);
    setSubmissionError('');
    timeoutSubmitRef.current = false;
    window.scrollTo({ top: 0 });
  };

  const isStartingExam = !isLessonQuiz && !examSession && !startError;
  const isLoading = lessonQuery.isLoading || isStartingExam;
  const isSubmitting = submitLessonQuiz.isPending || submitCourseExam.isPending;

  const course = courseQuery.data;
  const lessonTitle = course?.lessons.find((l) => l.id === lessonId)?.title;
  const kind = isLessonQuiz ? 'Quiz de lección' : 'Examen final';
  const title = isLessonQuiz ? lessonTitle ?? 'Evaluación de lección' : `Examen final${course ? ` · ${course.title}` : ''}`;
  const subtitle =
    questions.length > 0
      ? [
          `${questions.length} ${questions.length === 1 ? 'pregunta' : 'preguntas'} de opción única`,
          passScore !== undefined ? `Mínimo para aprobar: ${passScore}%` : null,
          `Intento ${Math.min(attemptsUsed + 1, maxAttempts)} de ${maxAttempts}`,
        ]
          .filter(Boolean)
          .join(' · ')
      : null;
  const remaining = questions.length - selectedCount;

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-col gap-4 px-4 py-6 md:gap-6 md:px-6 md:py-8">
      <Link
        to={`/courses/${courseId}`}
        className="flex w-fit items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeftIcon className="size-4" aria-hidden="true" /> {course?.title ?? 'Volver al curso'}
      </Link>

      <div>
        <p className="text-xs font-medium tracking-wider text-brand-foreground uppercase">{kind}</p>
        <h1 className="mt-1 text-xl font-semibold text-foreground md:text-2xl">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>}
      </div>

      {isLoading ? (
        <div className="grid gap-6 md:grid-cols-3" aria-busy="true">
          <div className="space-y-4 md:col-span-2">
            <Skeleton className="h-48 rounded-2xl" />
            <Skeleton className="h-48 rounded-2xl" />
          </div>
          <Skeleton className="h-48 rounded-2xl" />
        </div>
      ) : (isLessonQuiz && lessonQuery.isError) || (!isLessonQuiz && !examSession && startError) ? (
        // A course-exam start can fail because the student genuinely can't start a new
        // attempt (already passed, or ran out of attempts) rather than because of a real
        // error. When there are past attempts, show them read-only instead of a dead end.
        <>
          {attempts.length > 0 ? (
            <Alert tone="warning">{startError || 'No se pudo cargar la evaluación.'}</Alert>
          ) : (
            <StateBlock
              tipo="error"
              title="No pudimos cargar la evaluación"
              text={startError || 'Tus respuestas no se han enviado. Inténtalo de nuevo.'}
              onAction={() =>
                isLessonQuiz ? lessonQuery.refetch() : courseId && applyStartResult(startCourseExamAsync(courseId))
              }
            />
          )}
          {attempts.length > 0 && <AttemptsCard attempts={attempts} />}
        </>
      ) : !questions.length ? (
        <StateBlock
          icon={ListChecksIcon}
          title="Esta evaluación aún no tiene preguntas"
          text="El instructor todavía no ha publicado preguntas. Vuelve más tarde."
          action="Volver al curso"
          actionVariant="secondary"
          onAction={() => navigate(`/courses/${courseId}`)}
        />
      ) : result ? (
        <>
          <Card>
            <div className="flex flex-col items-center gap-3 pt-2 text-center md:pt-4">
              <span
                className={cn(
                  'flex size-16 items-center justify-center rounded-full',
                  result.isPassed ? 'bg-success/10 text-success-foreground' : 'bg-destructive/10 text-destructive-foreground'
                )}
              >
                {result.isPassed ? <TrophyIcon className="size-8" aria-hidden="true" /> : <XCircleIcon className="size-8" aria-hidden="true" />}
              </span>
              <h2 className="text-xl font-semibold text-foreground md:text-2xl">
                {result.isPassed ? 'Aprobaste la evaluación' : 'No aprobaste esta vez'}
              </h2>
              <p className="max-w-md text-sm text-muted-foreground">
                {result.feedback ||
                  (result.isPassed
                    ? 'Buen trabajo. Tu progreso quedó registrado.'
                    : `Repasa las lecciones anteriores y vuelve a intentarlo. ${
                        attemptsLeft === 1 ? 'Te queda 1 intento.' : `Te quedan ${attemptsLeft} intentos.`
                      }`)}
              </p>
            </div>
            <dl className="mt-6 grid grid-cols-2 gap-2 md:mt-8 md:grid-cols-4 md:gap-3">
              {[
                ['Puntaje', `${Math.round(result.score)}%`],
                ['Mínimo para aprobar', `${Math.round(result.passScore)}%`],
                ['Correctas', `${result.correctAnswers} / ${result.totalQuestions}`],
                ['Intento', `${computedAttemptsUsed} de ${maxAttempts}`],
              ].map(([k, v]) => (
                <div key={k} className="rounded-xl bg-secondary px-3 py-3 md:px-4 md:py-4">
                  <dt className="text-xs text-muted-foreground">{k}</dt>
                  <dd className="mt-1 text-lg font-semibold text-foreground tabular-nums md:text-xl">{v}</dd>
                </div>
              ))}
            </dl>
            {(progressFeedback || startError) && (
              <div className="mt-6 space-y-2">
                {progressFeedback && <Alert tone="success">{progressFeedback}</Alert>}
                {startError && <Alert tone="danger">{startError}</Alert>}
              </div>
            )}
            <div className="mt-6 flex flex-col gap-2 pb-2 sm:flex-row sm:flex-wrap sm:justify-center sm:gap-3">
              {result.isPassed || !canRetry ? (
                <ButtonLink to={`/courses/${courseId}`} className="w-full sm:w-auto">
                  Continuar con el curso <ArrowRightIcon />
                </ButtonLink>
              ) : (
                <>
                  <Button className="w-full sm:w-auto" onClick={startRetry} disabled={startCourseExam.isPending}>
                    {startCourseExam.isPending ? 'Iniciando…' : 'Reintentar evaluación'}
                  </Button>
                  <ButtonLink to={`/courses/${courseId}`} variant="secondary" className="w-full sm:w-auto">
                    Volver al curso
                  </ButtonLink>
                </>
              )}
            </div>
          </Card>
          {attempts.length > 0 && <AttemptsCard attempts={attempts} />}
        </>
      ) : (
        <div className="grid gap-6 md:grid-cols-3">
          <div className="flex flex-col gap-4 md:col-span-2">
            {timeExpiredOnLoad && (
              <Alert tone="warning">El tiempo sugerido para este intento ya terminó. Aún puedes responder y enviar tus respuestas.</Alert>
            )}
            <ol className="flex flex-col gap-4">
              {questions.map((question, index) => (
                <li key={question.id}>
                  <QuestionCard
                    index={index}
                    total={questions.length}
                    question={question}
                    selectedOptionId={selectedByQuestion[question.id]}
                    onSelectOption={(optionId) => setAnswer(question.id, optionId)}
                  />
                </li>
              ))}
            </ol>
          </div>

          <div>
            <div className="sticky top-24">
              <Card>
                <div className="flex items-center justify-between gap-2">
                  <p className="text-sm font-semibold text-foreground">Tu avance</p>
                  <span
                    className={cn(
                      'flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-medium tabular-nums',
                      timeLeftSec <= 30 && !timeExpiredOnLoad ? 'bg-destructive/10 text-destructive-foreground' : 'bg-secondary text-foreground-soft'
                    )}
                    role="timer"
                    aria-label={`Tiempo restante ${formatTime(timeLeftSec)}`}
                  >
                    <ClockIcon className="size-3.5" aria-hidden="true" /> {formatTime(timeLeftSec)}
                  </span>
                </div>
                <ProgressBar
                  className="mt-3"
                  value={(selectedCount / questions.length) * 100}
                  label={`${selectedCount} / ${questions.length} respondidas`}
                  tone="brand"
                  size="sm"
                />
                <div className="mt-4 grid grid-cols-5 gap-2" aria-hidden="true">
                  {questions.map((q, i) => (
                    <span
                      key={q.id}
                      className={cn(
                        'flex h-8 items-center justify-center rounded-md text-xs font-medium tabular-nums',
                        selectedByQuestion[q.id] ? 'bg-brand/20 text-brand-foreground' : 'bg-secondary text-subtle-foreground'
                      )}
                    >
                      {i + 1}
                    </span>
                  ))}
                </div>
                {submissionError && (
                  <Alert tone="danger" className="mt-4">
                    {submissionError}
                  </Alert>
                )}
                <Button
                  className="mt-5"
                  fullWidth
                  onClick={() => submitQuiz(false)}
                  disabled={isSubmitting || remaining > 0 || attemptsLeft <= 0}
                >
                  {isSubmitting ? 'Enviando…' : 'Enviar evaluación'}
                </Button>
                <p className="mt-3 text-xs text-muted-foreground">
                  {attemptsLeft <= 0
                    ? 'Ya no te quedan intentos para esta evaluación.'
                    : remaining > 0
                      ? `Responde ${remaining === 1 ? 'la pregunta restante' : `las ${remaining} preguntas restantes`} para enviar. Al terminar el tiempo se envían tus respuestas.`
                      : 'Puedes cambiar tus respuestas antes de enviar.'}
                </p>
              </Card>
            </div>
          </div>
        </div>
      )}
    </main>
  );
}

function QuestionCard({
  index,
  total,
  question,
  selectedOptionId,
  onSelectOption,
}: {
  index: number;
  total: number;
  question: QuizQuestion;
  selectedOptionId?: string;
  onSelectOption: (optionId: string) => void;
}) {
  const options = [...question.options].sort((a, b) => a.orderIndex - b.orderIndex);
  const labelId = `question-${question.id}`;

  return (
    <Card>
      <p className="text-xs font-medium text-subtle-foreground">
        Pregunta {index + 1} de {total}
      </p>
      <p id={labelId} className="mt-1 text-base font-medium text-foreground">
        {question.questionText}
      </p>
      <div role="radiogroup" aria-labelledby={labelId} className="mt-4 flex flex-col gap-2">
        {options.map((option) => (
          <RadioCard
            key={option.id}
            name={`question-${question.id}`}
            value={option.id}
            checked={selectedOptionId === option.id}
            onChange={() => onSelectOption(option.id)}
            label={option.optionText}
          />
        ))}
      </div>
    </Card>
  );
}

function AttemptsCard({ attempts }: { attempts: QuizAttemptDto[] }) {
  const sorted = [...attempts].sort((a, b) => b.attemptNumber - a.attemptNumber);

  return (
    <Card padding="none" className="overflow-hidden">
      <div className="border-b border-border px-4 py-4 md:px-6">
        <h2 className="text-base font-semibold text-foreground">Intentos anteriores</h2>
      </div>
      <ul className="divide-y divide-border md:hidden">
        {sorted.map((a) => (
          <li key={a.attemptNumber} className="flex items-center gap-3 px-4 py-3">
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-foreground">
                Intento {a.attemptNumber} · <span className="tabular-nums">{Math.round(a.score)}%</span>
              </p>
              <p className="text-xs text-muted-foreground">{formatDate(a.completedAt)}</p>
            </div>
            <Badge tone={a.isPassed ? 'success' : 'danger'}>{a.isPassed ? 'Aprobado' : 'No aprobado'}</Badge>
          </li>
        ))}
      </ul>
      <div className="hidden md:block">
        <Table>
          <TableHead>
            <TableRow>
              <TableHeader>Intento</TableHeader>
              <TableHeader>Puntaje</TableHeader>
              <TableHeader>Resultado</TableHeader>
              <TableHeader>Fecha</TableHeader>
            </TableRow>
          </TableHead>
          <TableBody>
            {sorted.map((a) => (
              <TableRow key={a.attemptNumber}>
                <TableCell className="text-foreground">Intento {a.attemptNumber}</TableCell>
                <TableCell className="tabular-nums">{Math.round(a.score)}%</TableCell>
                <TableCell>
                  <Badge tone={a.isPassed ? 'success' : 'danger'}>{a.isPassed ? 'Aprobado' : 'No aprobado'}</Badge>
                </TableCell>
                <TableCell className="text-muted-foreground">{formatDate(a.completedAt)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </Card>
  );
}
