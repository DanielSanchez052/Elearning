using ELearning.Application.Features.Gamification.DTOs;
using ELearning.Domain.Entities;

namespace ELearning.Application.Features.Gamification.Services;

/// <summary>
/// Called AFTER the triggering handler's own SaveChangesAsync (LoginHandler,
/// MarkLessonCompleteHandler, SubmitQuizHandler). Awarding is best-effort and
/// must never fail the user's actual action: implementations log failures and
/// return an empty list, and callers still wrap each call in try/catch as a
/// second guard (also covering cancellation, which is allowed to propagate).
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
