#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreeThemeViewModel
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Summary { get; init; } = string.Empty;

        public List<PublicationTreeTopicViewModel> Topics { get; init; } = new();
    }
}
