#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    /// <summary>
    /// SubjectMetaViewModel supporting hierarchical locations
    /// </summary>
    public record SubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; } = new();

        public Dictionary<string, IndicatorsMetaViewModel> Indicators { get; set; } = new();

        public Dictionary<string, LocationsMetaViewModel> Locations { get; set; } = new();

        public TimePeriodsMetaViewModel TimePeriod { get; set; } = new();
    }
}
