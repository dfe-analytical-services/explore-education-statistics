#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreePublicationViewModel
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Slug { get; init; } = string.Empty;

        public bool IsSuperseded { get; set; }

        public PublicationSupersededByViewModel? SupersededBy { get; set; }

        public bool LatestReleaseHasData { get; set; }

        public bool AnyLiveReleaseHasData { get; set; }
    }
}