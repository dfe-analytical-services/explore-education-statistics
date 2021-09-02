#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public record ReleaseTypeViewModel
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

    }
}