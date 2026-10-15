using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.Commands;
using ELearning.Application.Features.Quizzes.Queries;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Quizzes;

/// <summary>
/// Los quizzes siguen las reglas de acceso del curso (CourseAccess): el instructor solo los
/// de cursos que él creó, el admin los de cursos de su país, el superadmin todos.
/// </summary>
public class QuizAuthoringOwnershipTests
{
    private readonly Mock<IQuizRepository> _quizzesMock = new();
    private readonly Mock<ICourseRepository> _coursesMock = new();
    private readonly Mock<ILessonRepository> _lessonsMock = new();

    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherInstructorId = Guid.NewGuid();
    private const int CountryId = 57;
    private const int OtherCountryId = 52;

    private readonly Course _course;
    private readonly Lesson _lesson;
    private readonly QuizQuestion _lessonQuestion;
    private readonly QuizQuestion _examQuestion;
    private readonly QuizOption _option;

    public QuizAuthoringOwnershipTests()
    {
        _course = Course.Create("Curso", "Desc", null, OwnerId, isGlobal: false);
        _course.CourseCountries.Add(CourseCountry.Create(_course.Id, CountryId));
        _lesson = Lesson.Create(_course.Id, "Lección quiz", LessonType.Quiz, null, 1, isRequired: false);
        _lessonQuestion = QuizQuestion.CreatePerLesson(_lesson.Id, "¿Pregunta?", 60m, 3, 1, false);
        _examQuestion = QuizQuestion.CreateCourseExam(_course.Id, "¿Examen?", 70m, 3, 1, false);
        _option = QuizOption.Create(_lessonQuestion.Id, "Opción", true, 1);

        _coursesMock.Setup(r => r.GetByIdAsync(_course.Id, default)).ReturnsAsync(_course);
        _lessonsMock.Setup(r => r.GetByIdAsync(_lesson.Id, default)).ReturnsAsync(_lesson);
        _quizzesMock.Setup(r => r.GetQuestionByIdAsync(_lessonQuestion.Id, default)).ReturnsAsync(_lessonQuestion);
        _quizzesMock.Setup(r => r.GetQuestionByIdAsync(_examQuestion.Id, default)).ReturnsAsync(_examQuestion);
        _quizzesMock.Setup(r => r.GetOptionByIdAsync(_option.Id, default)).ReturnsAsync(_option);
        _quizzesMock.Setup(r => r.GetQuestionsByLessonAsync(_lesson.Id, default)).ReturnsAsync(new List<QuizQuestion> { _lessonQuestion });
        _quizzesMock.Setup(r => r.GetQuestionsByCourseAsync(_course.Id, default)).ReturnsAsync(new List<QuizQuestion> { _examQuestion });
        _quizzesMock.Setup(r => r.GetOptionsByQuestionAsync(_lessonQuestion.Id, default))
            .ReturnsAsync(new List<QuizOption> { _option, QuizOption.Create(_lessonQuestion.Id, "Otra", true, 2) });
    }

    private static void AssertForbidden(Result result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Forbidden, result.ErrorType);
    }

    // ── Preguntas ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateQuestion_PerLesson_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new CreateQuizQuestionHandler(_lessonsMock.Object, _coursesMock.Object, _quizzesMock.Object);

        var result = await handler.HandleAsync(new CreateQuizQuestionCommand(
            _lesson.Id, null, (int)QuizType.PerLesson, "Pregunta", 60m, 3, false, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateQuestion_PerLesson_InstructorOwner_Succeeds()
    {
        var handler = new CreateQuizQuestionHandler(_lessonsMock.Object, _coursesMock.Object, _quizzesMock.Object);
        _quizzesMock
            .Setup(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default))
            .ReturnsAsync((QuizQuestion q, CancellationToken _) => q);

        var result = await handler.HandleAsync(new CreateQuizQuestionCommand(
            _lesson.Id, null, (int)QuizType.PerLesson, "Pregunta", 60m, 3, false, OwnerId, "instructor", CountryId));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateQuestion_CourseExam_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new CreateQuizQuestionHandler(_lessonsMock.Object, _coursesMock.Object, _quizzesMock.Object);

        var result = await handler.HandleAsync(new CreateQuizQuestionCommand(
            null, _course.Id, (int)QuizType.CourseExam, "Pregunta", 70m, 3, false, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.CreateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateQuestion_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new UpdateQuizQuestionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new UpdateQuizQuestionCommand(
            _lessonQuestion.Id, "Editada", 60m, 3, false, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.UpdateQuestionAsync(It.IsAny<QuizQuestion>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateQuestion_InstructorOwner_Succeeds()
    {
        var handler = new UpdateQuizQuestionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new UpdateQuizQuestionCommand(
            _lessonQuestion.Id, "Editada", 60m, 3, false, OwnerId, "instructor", CountryId));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.UpdateQuestionAsync(_lessonQuestion, default), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_ExamQuestion_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new DeleteQuizQuestionHandler(_quizzesMock.Object, _lessonsMock.Object, _coursesMock.Object);

        var result = await handler.HandleAsync(new DeleteQuizQuestionCommand(
            _examQuestion.Id, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.DeleteQuestionAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    // ── Opciones ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOption_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new CreateQuizOptionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new CreateQuizOptionCommand(
            _lessonQuestion.Id, "Nueva", false, 3, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.CreateOptionAsync(It.IsAny<QuizOption>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateOption_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new UpdateQuizOptionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new UpdateQuizOptionCommand(
            _option.Id, "Editada", false, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.UpdateOptionAsync(It.IsAny<QuizOption>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteOption_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new DeleteQuizOptionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new DeleteQuizOptionCommand(
            _option.Id, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.DeleteOptionAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteOption_InstructorOwner_Succeeds()
    {
        var handler = new DeleteQuizOptionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new DeleteQuizOptionCommand(
            _option.Id, OwnerId, "instructor", CountryId));

        Assert.True(result.IsSuccess);
        _quizzesMock.Verify(r => r.DeleteOptionAsync(_option.Id, default), Times.Once);
    }

    // ── Lectura con respuestas (IsCorrect) ─────────────────────────────────────

    [Fact]
    public async Task GetLessonQuestions_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new GetLessonQuizQuestionsAdminHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new GetLessonQuizQuestionsAdminQuery(
            _lesson.Id, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.GetQuestionsByLessonAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task GetLessonQuestions_InstructorOwner_ReturnsQuestions()
    {
        var handler = new GetLessonQuizQuestionsAdminHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new GetLessonQuizQuestionsAdminQuery(
            _lesson.Id, OwnerId, "instructor", CountryId));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task GetCourseExamQuestions_InstructorNotOwner_ReturnsForbidden()
    {
        var handler = new GetCourseExamQuestionsAdminHandler(_quizzesMock.Object, _coursesMock.Object);

        var result = await handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(
            _course.Id, OtherInstructorId, "instructor", CountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.GetQuestionsByCourseAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    // ── Roles ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCourseExamQuestions_SuperAdmin_SkipsCourseLookup()
    {
        var handler = new GetCourseExamQuestionsAdminHandler(_quizzesMock.Object, _coursesMock.Object);

        var result = await handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(
            _course.Id, OtherInstructorId, "superadmin", OtherCountryId));

        Assert.True(result.IsSuccess);
        _coursesMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task GetCourseExamQuestions_AdminOfCourseCountry_Succeeds()
    {
        var handler = new GetCourseExamQuestionsAdminHandler(_quizzesMock.Object, _coursesMock.Object);

        var result = await handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(
            _course.Id, OtherInstructorId, "admin", CountryId));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateOption_AdminOfAnotherCountry_ReturnsForbidden()
    {
        var handler = new UpdateQuizOptionHandler(_quizzesMock.Object, _coursesMock.Object, _lessonsMock.Object);

        var result = await handler.HandleAsync(new UpdateQuizOptionCommand(
            _option.Id, "Editada", false, Guid.NewGuid(), "admin", OtherCountryId));

        AssertForbidden(result);
        _quizzesMock.Verify(r => r.UpdateOptionAsync(It.IsAny<QuizOption>(), default), Times.Never);
    }

    [Theory]
    [InlineData("student")]
    [InlineData("1")]          // valor numérico de UserRole.Admin: no debe aceptarse
    [InlineData("")]
    public async Task GetCourseExamQuestions_NonStaffRole_ReturnsForbidden(string role)
    {
        var handler = new GetCourseExamQuestionsAdminHandler(_quizzesMock.Object, _coursesMock.Object);

        var result = await handler.HandleAsync(new GetCourseExamQuestionsAdminQuery(
            _course.Id, OwnerId, role, CountryId));

        AssertForbidden(result);
    }
}
