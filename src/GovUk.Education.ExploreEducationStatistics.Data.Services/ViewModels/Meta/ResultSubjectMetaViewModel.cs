#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record ResultSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; init; } = new();

        public List<FootnoteViewModel> Footnotes { get; init; } = new();

        public List<IndicatorMetaViewModel> Indicators { get; init; } = new();

        // TODO EES-2917 Work out how to maintain backwards compatibility for Locations with the hierarchical response type
        // * Backwards compatibility required with existing Permalinks unless we migrate them.
        // * Backwards compatibility required with the current UI expecting locations in this form until it's updated in EES-2777.
        // For now hierarchical locations are populated in a separate field 'LocationsHierarchical' when the feature is turned on.
        public List<ObservationalUnitMetaViewModel> Locations { get; init; } = new();

        public Dictionary<string, List<LocationAttributeViewModel>> LocationsHierarchical { get; init; } = new();

        public List<BoundaryLevelViewModel> BoundaryLevels { get; init; } = new();

        public string PublicationName { get; init; } = string.Empty;

        public string SubjectName { get; init; } = string.Empty;

        public List<TimePeriodMetaViewModel> TimePeriodRange { get; init; } = new();

        public bool GeoJsonAvailable { get; init; }
    }
}
