import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import {
  useCreateQuizQuestion,
  useDeleteQuizQuestion,
  useUpdateQuizQuestion,
  useCreateQuizOption,
  useDeleteQuizOption,
  useUpdateQuizOption,
  useAdminLessonQuizzes,
  useAdminCourseExam,
} from '@/hooks/admin/quizzes';
import { ListQuestions } from '@/components/admin/ListQuestions';
import { QuestionComposerModal, type QuestionComposerSubmit } from '@/components/admin/QuestionComposerModal';
import { useCourseDetail } from '@/hooks/useCourses';
import { Alert, Button, Modal, Skeleton, StateBlock } from '@/components/ui';
import { ArrowLeftIcon, ListChecksIcon, PlusIcon, ShieldIcon, TrashIcon } from '@/components/ui/icons';
import { getApiErrorMessage } from '@/lib/axios';
import { canManageCourse, courseAccessDeniedReason } from '@/lib/courseAccess';
import { useAuthStore } from '@/store/authStore';
import type { CreateQuizOptionRequest, CreateQuizQuestionRequest, UpdateQuizQuestionRequest } from '@/types/quiz.types';

type ModalState = { kind: 'create' } | { kind: 'edit'; questionId: string } | { kind: 'delete'; questionId: string } | null;

export function AdminQuizzesPage() {
  const { courseId = '', lessonId } = useParams<{ courseId: string; lessonId?: string }>();

  const [modal, setModal] = useState<ModalState>(null);
  const [saveError, setSaveError] = useState('');
  const [notice, setNotice] = useState<{ tone: 'success' | 'danger'; text: string } | null>(null);

  const courseDetail = useCourseDetail(courseId);
  const user = useAuthStore((s) => s.user);
  const lessons = courseDetail.data?.lessons ?? [];
  const lessonOptions = lessons.map((lesson) => ({
    id: lesson.id,
    title: lesson.title,
    orderIndex: lesson.orderIndex,
    type: lesson.type,
  }));

  // Queries
  // Las preguntas incluyen las respuestas: solo se piden cuando se sabe que el usuario gestiona el curso.
  const canManage = !!courseDetail.data && canManageCourse(user, courseDetail.data);
  const lessonQuizzes = useAdminLessonQuizzes(lessonId || '', canManage && !!lessonId);
  const courseExam = useAdminCourseExam(courseId, canManage && !!courseId && !lessonId);
  const activeQuery = lessonId ? lessonQuizzes : courseExam;
  const questions = activeQuery.data ?? [];

  // Mutations
  const createQuestion = useCreateQuizQuestion();
  const updateQuestion = useUpdateQuizQuestion();
  const deleteQuestion = useDeleteQuizQuestion();
  const createOption = useCreateQuizOption();
  const updateOption = useUpdateQuizOption();
  const deleteOption = useDeleteQuizOption();

  const isMutating =
    createQuestion.isPending ||
    updateQuestion.isPending ||
    deleteQuestion.isPending ||
    createOption.isPending ||
    updateOption.isPending ||
    deleteOption.isPending;

  const selectedQuestion = modal && modal.kind !== 'create' ? questions.find((q) => q.id === modal.questionId) : undefined;

  const closeModal = () => {
    setModal(null);
    setSaveError('');
  };

  const handleCreateQuestion = async (data: { question: CreateQuizQuestionRequest; options: CreateQuizOptionRequest[] }) => {
    setSaveError('');
    try {
      const res = await createQuestion.mutateAsync(data.question);
      const questionId = res.data;
      if (!questionId) {
        throw new Error('El servidor no devolvió el id de la pregunta creada.');
      }
      for (const option of data.options) {
        await createOption.mutateAsync({ questionId, data: option });
      }
      closeModal();
      setNotice({ tone: 'success', text: 'Pregunta creada con sus opciones.' });
    } catch (error) {
      setSaveError(getApiErrorMessage(error) || 'No se pudo crear la pregunta con sus opciones.');
    }
  };

  const handleSaveQuestion = async (data: QuestionComposerSubmit) => {
    if (!selectedQuestion) return;
    const questionId = selectedQuestion.id;
    setSaveError('');

    try {
      const questionPayload: UpdateQuizQuestionRequest = {
        questionText: data.question.questionText,
        passScore: data.question.passScore,
        maxAttempts: data.question.maxAttempts,
        isRequired: data.question.isRequired,
      };
      await updateQuestion.mutateAsync({ questionId, data: questionPayload });

      const submittedOptionIds = new Set(
        data.options.map((option) => option.optionId).filter((optionId): optionId is string => Boolean(optionId))
      );
      for (const option of selectedQuestion.options.filter((o) => !submittedOptionIds.has(o.id))) {
        await deleteOption.mutateAsync({ questionId, optionId: option.id });
      }

      for (const option of data.options) {
        if (option.optionId) {
          await updateOption.mutateAsync({
            questionId,
            optionId: option.optionId,
            data: { optionText: option.optionText, isCorrect: option.isCorrect },
          });
          continue;
        }
        await createOption.mutateAsync({
          questionId,
          data: { optionText: option.optionText, isCorrect: option.isCorrect, orderIndex: option.orderIndex },
        });
      }

      closeModal();
      setNotice({ tone: 'success', text: 'Pregunta y opciones actualizadas.' });
    } catch (error) {
      setSaveError(getApiErrorMessage(error) || 'No se pudo actualizar la pregunta.');
    }
  };

  const handleDeleteQuestion = (questionId: string) => {
    deleteQuestion.mutate(questionId, {
      onSuccess: () => {
        closeModal();
        setNotice({ tone: 'success', text: 'Pregunta eliminada.' });
      },
      onError: (error) => {
        closeModal();
        setNotice({ tone: 'danger', text: getApiErrorMessage(error) });
      },
    });
  };

  const lessonTitle = lessons.find((l) => l.id === lessonId)?.title;
  const kind = lessonId ? 'Quiz de lección' : 'Examen final del curso';
  const title = lessonId ? lessonTitle ?? 'Quiz de lección' : `Examen final${courseDetail.data ? ` · ${courseDetail.data.title}` : ''}`;
  const meta = questions[0]
    ? `Mínimo para aprobar: ${questions[0].passScore}% · Intentos máximos: ${questions[0].maxAttempts} · ${questions.length} ${
        questions.length === 1 ? 'pregunta' : 'preguntas'
      }`
    : null;

  if (courseDetail.data && !canManage) {
    return (
      <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
        <Link to="/admin/courses" className="flex w-fit items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeftIcon className="size-4" aria-hidden="true" /> Cursos
        </Link>
        <StateBlock
          icon={ShieldIcon}
          title="No puedes gestionar las evaluaciones de este curso"
          text={courseAccessDeniedReason(user, courseDetail.data)}
        />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-8">
      <Link
        to={`/admin/courses/${courseId}/edit`}
        className="flex w-fit items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeftIcon className="size-4" aria-hidden="true" /> {courseDetail.data?.title ?? 'Volver al curso'}
      </Link>

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="text-xs font-medium tracking-wider text-brand-foreground uppercase">{kind}</p>
          <h1 className="mt-1 text-2xl font-semibold text-foreground">{title}</h1>
          {meta && <p className="mt-1 text-sm text-muted-foreground">{meta}</p>}
        </div>
        <Button onClick={() => setModal({ kind: 'create' })}>
          <PlusIcon /> Nueva pregunta
        </Button>
      </div>

      {notice && (
        <Alert tone={notice.tone} className="max-w-4xl">
          {notice.text}
        </Alert>
      )}

      <div className="max-w-4xl">
        {courseDetail.isError ? (
          <StateBlock tipo="error" title="No pudimos cargar el curso" onAction={() => courseDetail.refetch()} />
        ) : courseDetail.isLoading || activeQuery.isLoading ? (
          <div className="flex flex-col gap-4" aria-busy="true">
            <Skeleton className="h-48 rounded-2xl" />
            <Skeleton className="h-48 rounded-2xl" />
          </div>
        ) : activeQuery.isError ? (
          <StateBlock tipo="error" title="No pudimos cargar las preguntas" onAction={() => activeQuery.refetch()} />
        ) : questions.length === 0 ? (
          <StateBlock
            icon={ListChecksIcon}
            title="Esta evaluación aún no tiene preguntas"
            text="Agrega preguntas de opción única con al menos 3 opciones y una respuesta correcta."
            action="Nueva pregunta"
            actionVariant="secondary"
            onAction={() => setModal({ kind: 'create' })}
          />
        ) : (
          <ListQuestions
            questions={questions}
            isLoading={isMutating}
            onAction={({ questionId, action }) => {
              setNotice(null);
              setModal(action === 'edit' ? { kind: 'edit', questionId } : { kind: 'delete', questionId });
            }}
          />
        )}
      </div>

      {modal?.kind === 'create' && (
        <QuestionComposerModal
          lessonId={lessonId}
          courseId={courseId}
          lessonOptions={lessonOptions}
          mode="create"
          onClose={closeModal}
          onSubmit={handleCreateQuestion}
          isLoading={createQuestion.isPending || createOption.isPending}
          error={saveError}
        />
      )}

      {modal?.kind === 'edit' && selectedQuestion && (
        <QuestionComposerModal
          key={selectedQuestion.id}
          mode="edit"
          initialQuestion={selectedQuestion}
          lessonId={lessonId}
          courseId={courseId}
          onSubmit={handleSaveQuestion}
          onClose={closeModal}
          isLoading={updateQuestion.isPending || createOption.isPending || updateOption.isPending || deleteOption.isPending}
          error={saveError}
        />
      )}

      <Modal
        open={modal?.kind === 'delete'}
        onClose={closeModal}
        size="sm"
        title="Eliminar pregunta"
        description="Esta acción no se puede deshacer."
        footer={
          <>
            <Button variant="secondary" onClick={closeModal}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              disabled={deleteQuestion.isPending}
              onClick={() => modal?.kind === 'delete' && handleDeleteQuestion(modal.questionId)}
            >
              <TrashIcon /> {deleteQuestion.isPending ? 'Eliminando…' : 'Eliminar pregunta'}
            </Button>
          </>
        }
      >
        <p className="text-sm text-foreground-soft">
          Se eliminará la pregunta{selectedQuestion ? ` «${selectedQuestion.questionText}»` : ''} y todas sus opciones.
        </p>
      </Modal>
    </div>
  );
}
