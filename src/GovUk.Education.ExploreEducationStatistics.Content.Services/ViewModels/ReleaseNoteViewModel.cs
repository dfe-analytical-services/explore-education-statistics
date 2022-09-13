#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record ReleaseNoteViewModel
{
    public Guid Id { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime On { get; set; }
}
