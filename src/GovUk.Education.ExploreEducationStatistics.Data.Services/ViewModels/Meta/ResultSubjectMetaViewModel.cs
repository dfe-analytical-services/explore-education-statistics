#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record ResultSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; init; } = new();

        public List<FootnoteViewModel> Footnotes { get; init; } = new();

        public List<IndicatorMetaViewModel> Indicators { get; init; } = new();

        /// <summary>
        /// Hierarchical locations field.
        /// </summary>
        /// <remarks>
        /// TODO EES-2943: This could potentially be renamed back to 'Locations' but requires a migration of
        /// old Permalinks which already have a legacy 'Locations' field in their JSON serialization of type <see cref="List{ObservationalUnitMetaViewModel}"/>.
        /// TODO EES-3106: This could also be renamed back to 'Locations' in Permalink responses independently of EES-2943
        /// but will require a new view model to be split from the model used by serialization.
        /// </remarks>
        public Dictionary<string, List<LocationAttributeViewModel>> LocationsHierarchical { get; set; } = new();

        public List<BoundaryLevelViewModel> BoundaryLevels { get; init; } = new();

        public string PublicationName { get; init; } = string.Empty;

        public string SubjectName { get; init; } = string.Empty;

        public List<TimePeriodMetaViewModel> TimePeriodRange { get; init; } = new();

        public bool GeoJsonAvailable { get; init; }
    }
}
