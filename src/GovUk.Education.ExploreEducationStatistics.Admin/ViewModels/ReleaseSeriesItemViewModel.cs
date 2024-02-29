#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ReleaseSeriesItemViewModel
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsLegacy { get; set; }

    public bool IsDraft { get; set; }

    public bool IsAmendment { get; set; }

    public bool IsLatest { get; set; }

    public string Url { get; set; } = string.Empty;

    public int Order { get; set; }
}
