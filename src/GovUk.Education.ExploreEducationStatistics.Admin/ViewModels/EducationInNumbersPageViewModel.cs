#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class EducationInNumbersPageViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateTime? Published { get; set; }
}
