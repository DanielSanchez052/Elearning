using System;
using System.Collections.Generic;
using System.Text;

namespace ELearning.Application.Features.Quizzes.DTOs;

public sealed record QuizOptionDto(
    Guid Id,
    string OptionText,
    int OrderIndex
);
