import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useLessonQuizzes,
  useCourseExam,
  useCreateQuizQuestion,
  useDeleteQuizQuestion,
  useUpdateQuizQuestion,
  useCreateQuizOption,
  useDeleteQuizOption,
  useUpdateQuizOption,
} from '@/hooks/admin/quizzes';
import { ListQuestions } from '@/components/admin/ListQuestions';
import {
  QuestionComposerModal,
  type QuestionComposerSubmit,
} from '@/components/admin/QuestionComposerModal';
import type {
  CreateQuizOptionRequest,
  CreateQuizQuestionRequest,
  UpdateQuizQuestionRequest,
} from '@/types/quiz.types';

type Modal = 'create-question' | 'edit-question' | null;

interface State {
  currentModal: Modal;
  selectedQuestion: string | null;
}

export function AdminQuizzesPage() {
  const { courseId, lessonId } = useParams<{
    courseId: string;
    lessonId?: string;
  }>();
  const navigate = useNavigate();

  const [state, setState] = useState<State>({
    currentModal: null,
    selectedQuestion: null,
  });

  // Queries
  const lessonQuizzes = useLessonQuizzes(lessonId || '', !!lessonId);
  const courseExam = useCourseExam(courseId || '', !!courseId && !lessonId);

  const questions = lessonId ? lessonQuizzes.data || [] : courseExam.data || [];
  const isLoadingQuestions = lessonQuizzes.isLoading || courseExam.isLoading;

  // Mutations
  const createQuestion = useCreateQuizQuestion();
  const updateQuestion = useUpdateQuizQuestion();
  const deleteQuestion = useDeleteQuizQuestion();
  const createOption = useCreateQuizOption();
  const updateOption = useUpdateQuizOption();
  const deleteOption = useDeleteQuizOption();

  // Get selected question
  const selectedQuestion = questions.find((q) => q.id === state.selectedQuestion);

  // Modal handlers
  const openCreateQuestion = () => {
    setState((s) => ({
      ...s,
      currentModal: 'create-question',
    }));
  };

  const closeModal = () => {
    setState((s) => ({
      ...s,
      currentModal: null,
      selectedQuestion: null,
    }));
  };

  const handleCreateQuestion = async (data: {
    question: CreateQuizQuestionRequest;
    options: CreateQuizOptionRequest[];
  }) => {
    try {
      const res = await createQuestion.mutateAsync(data.question);
      const questionId = res.data.value;

      for (const option of data.options) {
        await createOption.mutateAsync({
          questionId,
          data: option,
        });
      }

      closeModal();
      alert('Pregunta y opciones creadas exitosamente');
    } catch (error) {
      console.error('Error creating question:', error);
      alert('Error al crear la pregunta con sus opciones');
    }
  };

  const handleDeleteQuestion = (questionId: string) => {
    if (
      !window.confirm(
        '¿Estás seguro? Esto eliminará la pregunta y todas sus opciones.'
      )
    ) {
      return;
    }
    deleteQuestion.mutate(questionId, {
      onSuccess: () => {
        alert('Pregunta eliminada exitosamente');
      },
      onError: () => {
        alert('Error al eliminar la pregunta');
      },
    });
  };

  const handleUpdateQuestion = (questionId: string) => {
    setState((s) => ({
      ...s,
      selectedQuestion: questionId,
      currentModal: 'edit-question',
    }));
  };

  const handleSaveQuestion = async (data: QuestionComposerSubmit) => {
    if (!state.selectedQuestion || !selectedQuestion) return;

    try {
      const questionPayload: UpdateQuizQuestionRequest = {
        questionText: data.question.questionText,
        passScore: data.question.passScore,
        maxAttempts: data.question.maxAttempts,
        isRequired: data.question.isRequired,
      };

      await updateQuestion.mutateAsync({
        questionId: state.selectedQuestion,
        data: questionPayload,
      });

      const existingOptions = selectedQuestion.options;
      const submittedOptionIds = new Set(
        data.options
          .map((option) => option.optionId)
          .filter((optionId): optionId is string => Boolean(optionId))
      );

      const optionsToDelete = existingOptions.filter(
        (option) => !submittedOptionIds.has(option.id)
      );

      for (const option of optionsToDelete) {
        await deleteOption.mutateAsync({
          questionId: state.selectedQuestion,
          optionId: option.id,
        });
      }

      for (const option of data.options) {
        if (option.optionId) {
          await updateOption.mutateAsync({
            questionId: state.selectedQuestion,
            optionId: option.optionId,
            data: {
              optionText: option.optionText,
              isCorrect: option.isCorrect,
            },
          });
          continue;
        }

        await createOption.mutateAsync({
          questionId: state.selectedQuestion,
          data: {
            optionText: option.optionText,
            isCorrect: option.isCorrect,
            orderIndex: option.orderIndex,
          },
        });
      }

      closeModal();
      alert('Pregunta y opciones actualizadas exitosamente');
    } catch (error) {
      console.error('Error updating question:', error);
      alert('Error al actualizar la pregunta y sus opciones');
    }
  };

  const handleQuestionAction = (payload: {
    questionId: string;
    action: 'edit' | 'delete';
  }) => {
    switch (payload.action) {
      case 'edit':
        handleUpdateQuestion(payload.questionId);
        break;
      case 'delete':
        handleDeleteQuestion(payload.questionId);
        break;
    }
  };

  const isLoading =
    createQuestion.isPending ||
    updateQuestion.isPending ||
    deleteQuestion.isPending ||
    createOption.isPending ||
    updateOption.isPending ||
    deleteOption.isPending;

  const pageTitle = lessonId
    ? 'Preguntas de Lección'
    : 'Examen Final del Curso';

  return (
    <div className="min-h-screen bg-gradient-to-br from-[#0a0a0f] to-[#1a1a2e] p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-4xl font-bold text-white mb-2">
                {pageTitle}
              </h1>
              <p className="text-zinc-400">
                Gestiona las preguntas y opciones del quiz
              </p>
            </div>
            <button
              onClick={() => navigate(`/admin/courses/${courseId}`)}
              className="px-4 py-2 text-zinc-400 hover:text-white transition"
            >
              ← Volver
            </button>
          </div>
        </div>

        {/* Create Question Button */}
        {!state.currentModal && (
            <div className="mb-6">
              <button
                onClick={openCreateQuestion}
                className="px-6 py-3 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg font-medium transition flex items-center gap-2"
              >
                <span>+</span> Nueva Pregunta
              </button>
            </div>
          )}

        {/* Questions List */}
        <div className="bg-white/[0.03] border border-white/[0.06] rounded-xl p-6">
          <h2 className="text-xl font-semibold text-white mb-6">
            Preguntas ({questions.length})
          </h2>

          {isLoadingQuestions ? (
            <div className="text-center py-12">
              <p className="text-zinc-400">Cargando preguntas...</p>
            </div>
          ) : (
            <ListQuestions
              questions={questions}
              onAction={handleQuestionAction}
              isLoading={isLoading}
            />
          )}
        </div>
      </div>

      {/* Modals */}
      {state.currentModal === 'create-question' && (
        <QuestionComposerModal
          lessonId={lessonId}
          courseId={courseId}
          mode="create"
          onClose={closeModal}
          onSubmit={handleCreateQuestion}
          isLoading={createQuestion.isPending || createOption.isPending}
        />
      )}

      {state.currentModal === 'edit-question' && selectedQuestion && (
        <QuestionComposerModal
          mode="edit"
          initialQuestion={selectedQuestion}
          lessonId={lessonId}
          courseId={courseId}
          onSubmit={handleSaveQuestion}
          onClose={closeModal}
          isLoading={
            updateQuestion.isPending ||
            createOption.isPending ||
            updateOption.isPending ||
            deleteOption.isPending
          }
        />
      )}
    </div>
  );
}
