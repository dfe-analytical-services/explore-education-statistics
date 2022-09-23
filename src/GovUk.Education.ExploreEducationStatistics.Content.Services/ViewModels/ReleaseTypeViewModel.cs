#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    [Obsolete("TODO EES-3038 - Replace with ReleaseType. Requires a content cache refresh", false)]
    public record ReleaseTypeViewModel
    {
        public string Title { get; init; } = string.Empty;
    }
}
