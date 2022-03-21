#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record LocationsMetaViewModel
    {
        public string Legend { get; init; } = string.Empty;
        public List<LocationAttributeViewModel> Options { get; init; } = new();
    }

    public record LocationAttributeViewModel : LabelValue
    {
        public Guid? Id { get; set; }

        public dynamic? GeoJson { get; init; }

        public string? Level { get; init; }

        public List<LocationAttributeViewModel>? Options { get; init; }
    }
}
