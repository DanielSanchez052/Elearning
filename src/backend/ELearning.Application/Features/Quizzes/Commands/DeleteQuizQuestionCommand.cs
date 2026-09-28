using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record DeleteQuizQuestionCommand(
    Guid QuestionId
) : ICommand;

public sealed class DeleteQuizQuestionHandler : ICommandHandler<DeleteQuizQuestionCommand>
{
    private readonly IQuizRepository _quizzes;
    private readonly ILessonRepository _lessons;

    public DeleteQuizQuestionHandler(IQuizRepository quizzes, ILessonRepository lessons)
    {
        _quizzes = quizzes;
        _lessons = lessons;
    }

    public async Task<Result> HandleAsync(DeleteQuizQuestionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.QuestionId == Guid.Empty)
            return Result.ValidationFailure("QuestionId es requerido");

        var question = await _quizzes.GetQuestionByIdAsync(cmd.QuestionId, ct);
        if (question is null)
            return Result.NotFound("Pregunta no encontrada");

        // Solo bloqueamos si borrar dejaría una lección OBLIGATORIA sin preguntas
        // (el gate de "lecciones requeridas" nunca podría satisfacerse). Un quiz
        // opcional, o el examen final del curso (su "obligatoriedad" depende de que
        // tenga preguntas: sin ninguna, simplemente deja de exigirse), pueden vaciarse.
        if (question.LessonId is not null)
        {
            var lesson = await _lessons.GetByIdAsync(question.LessonId.Value, ct);
            if (lesson is { IsRequired: true })
            {
                var scopeQuestions = await _quizzes.GetQuestionsByLessonAsync(question.LessonId.Value, ct);
                if (scopeQuestions.Count <= 1)
                    return Result.ValidationFailure("No puedes eliminar la última pregunta de esta lección obligatoria. Márcala como no obligatoria, o elimina la lección completa, primero.");
            }
        }

        await _quizzes.DeleteQuestionAsync(cmd.QuestionId, ct);
        await _quizzes.SaveChangesAsync(ct);

        return Result.Success();
    }
}
