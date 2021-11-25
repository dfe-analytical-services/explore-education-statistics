#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record LocationsMetaViewModel
    {
        public string Legend { get; init; } = string.Empty;
        public List<LocationAttributeViewModel> Options { get; init; } = new();
    }

    public record LocationAttributeViewModel
    {
        public dynamic? GeoJson { get; init; }
        public string Label { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string? Level { get; init; }
        public List<LocationAttributeViewModel>? Options { get; init; }
    }
}
