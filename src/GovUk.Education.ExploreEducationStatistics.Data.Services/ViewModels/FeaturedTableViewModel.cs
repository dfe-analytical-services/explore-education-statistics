#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

public record FeaturedTableViewModel(
    Guid Id,
    string Name,
    string? Description,
    Guid SubjectId,
    Guid DataBlockId,
    int Order);
