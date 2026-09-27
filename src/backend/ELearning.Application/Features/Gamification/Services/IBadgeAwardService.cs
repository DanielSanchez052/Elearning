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
    /// TODO(Track A): wire SpeedsterRule once ExamSession/Duration land.
    /// Track A hasn't shipped UserQuizResult.Duration yet, so there is
    /// nothing to evaluate the 10-minute threshold against. This is a
    /// documented no-op until Integration implements SpeedsterRule against
    /// IExamPassedBadgeRule and wires it in here.
    /// </summary>
    Task<IReadOnlyList<AwardedBadgeDto>> OnCourseExamPassedAsync(UserQuizResult result, CancellationToken ct = default);
}
