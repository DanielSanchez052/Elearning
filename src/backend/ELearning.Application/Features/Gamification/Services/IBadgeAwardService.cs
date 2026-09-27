using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Gamification.Services;

/// <summary>
/// Called AFTER the triggering handler's own SaveChangesAsync. Awarding is
/// best-effort and must never fail the user's actual action (login, lesson
/// completion, quiz submission) — the caller is expected to invoke these
/// methods defensively (e.g. wrapped in try/catch) once Integration wires
/// them into LoginHandler / MarkLessonCompleteHandler / SubmitQuizHandler.
/// </summary>
public interface IBadgeAwardService
{
    Task<IReadOnlyList<AwardedBadgeDto>> OnUserLoggedInAsync(User user, CancellationToken ct = default);

    Task<IReadOnlyList<AwardedBadgeDto>> OnCourseCompletedAsync(CourseEnrollment enrollment, CancellationToken ct = default);

    /// <summary>
    /// Evaluates the IExamPassedBadgeRule set (SpeedsterRule) against the
    /// course-exam result the caller just persisted. The rules decide
    /// eligibility (passed, first attempt, Duration under the threshold);
    /// this method only awards idempotently.
    /// </summary>
    Task<IReadOnlyList<AwardedBadgeDto>> OnCourseExamPassedAsync(UserQuizResult result, CancellationToken ct = default);
}
