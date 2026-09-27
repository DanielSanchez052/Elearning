namespace ELearning.Domain.Enums;

/// <summary>
/// Outcome of <c>IQuizRepository.TryAddExamSessionAsync</c>. Distinguishes which unique index
/// (if any) rejected the insert of a new <c>exam_sessions</c> row, so callers can react
/// differently to each race instead of collapsing every 23505 into a single generic failure.
/// </summary>
public enum ExamSessionInsertResult
{
    /// <summary>The session was inserted successfully.</summary>
    Inserted,

    /// <summary>
    /// Rejected by <c>idx_exam_session_one_open_per_user_course</c>: another request already has
    /// an open (not submitted) session for this user+course. Expected concurrency — the caller
    /// should re-fetch the open session and return it.
    /// </summary>
    OpenSessionRace,

    /// <summary>
    /// Rejected by <c>idx_exam_session_user_course_attempt</c>: a session (open or already
    /// submitted) already exists for this exact attempt number — e.g. because a quiz result was
    /// reset/deleted, or the attempt number desynced from the exam-session table for some other
    /// reason. There is no open session to resume. Re-deriving the next attempt number from the
    /// same inputs would return the identical value within one request, so the caller's only
    /// recovery is to skip past the colliding number with exactly one retry (bounded by the
    /// attempt limit), then surface a conflict if that retry collides too.
    /// </summary>
    AttemptNumberCollision
}
