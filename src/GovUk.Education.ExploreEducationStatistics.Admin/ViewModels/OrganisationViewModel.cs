#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record OrganisationViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }
}
