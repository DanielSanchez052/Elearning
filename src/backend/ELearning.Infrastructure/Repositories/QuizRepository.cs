using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly ApplicationDbContext _context;

    public QuizRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── Questions ──────────────────────────────────────────────────────────────

    public async Task<QuizQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct = default)
    {
        return await _context.QuizQuestions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);
    }

    public async Task<IReadOnlyList<QuizQuestion>> GetQuestionsByLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        return await _context.QuizQuestions
            .Where(q => q.LessonId == lessonId && q.Type == QuizType.PerLesson)
            .Include(q => q.Options)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<QuizQuestion>> GetQuestionsByCourseAsync(Guid courseId, CancellationToken ct = default)
    {
        return await _context.QuizQuestions
            .Where(q => q.CourseId == courseId && q.Type == QuizType.CourseExam)
            .Include(q => q.Options)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task<QuizQuestion> CreateQuestionAsync(QuizQuestion question, CancellationToken ct = default)
    {
        await _context.QuizQuestions.AddAsync(question, ct);
        return question;
    }

    public async Task UpdateQuestionAsync(QuizQuestion question, CancellationToken ct = default)
    {
        _context.QuizQuestions.Update(question);
        await Task.CompletedTask;
    }

    public async Task DeleteQuestionAsync(Guid questionId, CancellationToken ct = default)
    {
        var question = await GetQuestionByIdAsync(questionId, ct);
        if (question is not null)
        {
            _context.QuizQuestions.Remove(question);
        }
    }

    // ── Options ────────────────────────────────────────────────────────────────

    public async Task<QuizOption?> GetOptionByIdAsync(Guid optionId, CancellationToken ct = default)
    {
        return await _context.QuizOptions
            .FirstOrDefaultAsync(o => o.Id == optionId, ct);
    }

    public async Task<IReadOnlyList<QuizOption>> GetOptionsByQuestionAsync(Guid questionId, CancellationToken ct = default)
    {
        return await _context.QuizOptions
            .Where(o => o.QuestionId == questionId)
            .OrderBy(o => o.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task<QuizOption> CreateOptionAsync(QuizOption option, CancellationToken ct = default)
    {
        await _context.QuizOptions.AddAsync(option, ct);
        return option;
    }

    public async Task UpdateOptionAsync(QuizOption option, CancellationToken ct = default)
    {
        _context.QuizOptions.Update(option);
        await Task.CompletedTask;
    }

    public async Task DeleteOptionAsync(Guid optionId, CancellationToken ct = default)
    {
        var option = await GetOptionByIdAsync(optionId, ct);
        if (option is not null)
        {
            _context.QuizOptions.Remove(option);
        }
    }

    // ── User Attempts ──────────────────────────────────────────────────────────

    public async Task<UserQuizAttempt> CreateAttemptAsync(UserQuizAttempt attempt, CancellationToken ct = default)
    {
        await _context.UserQuizAttempts.AddAsync(attempt, ct);
        return attempt;
    }

    public async Task<IReadOnlyList<UserQuizAttempt>> GetAttemptsAsync(Guid userId, Guid questionId, CancellationToken ct = default)
    {
        return await _context.UserQuizAttempts
            .Where(a => a.UserId == userId && a.QuestionId == questionId)
            .OrderBy(a => a.AttemptNumber)
            .ToListAsync(ct);
    }

    // ── User Results ───────────────────────────────────────────────────────────

    public async Task<UserQuizResult> CreateResultAsync(UserQuizResult result, CancellationToken ct = default)
    {
        await _context.UserQuizResults.AddAsync(result, ct);
        return result;
    }

    public async Task<UserQuizResult?> GetLatestLessonResultAsync(Guid userId, Guid lessonId, CancellationToken ct = default)
    {
        return await _context.UserQuizResults
            .Where(r => r.UserId == userId && r.LessonId == lessonId)
            .OrderByDescending(r => r.AttemptNumber)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserQuizResult?> GetLatestCourseExamResultAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        return await _context.UserQuizResults
            .Where(r => r.UserId == userId && r.CourseId == courseId)
            .OrderByDescending(r => r.AttemptNumber)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<UserQuizResult>> GetLessonAttemptsAsync(Guid userId, Guid lessonId, CancellationToken ct = default)
    {
        return await _context.UserQuizResults
            .Where(r => r.UserId == userId && r.LessonId == lessonId)
            .OrderByDescending(r => r.AttemptNumber)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserQuizResult>> GetCourseExamAttemptsAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        return await _context.UserQuizResults
            .Where(r => r.UserId == userId && r.CourseId == courseId)
            .OrderByDescending(r => r.AttemptNumber)
            .ToListAsync(ct);
    }

    // ── Persistence ────────────────────────────────────────────────────────────

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
