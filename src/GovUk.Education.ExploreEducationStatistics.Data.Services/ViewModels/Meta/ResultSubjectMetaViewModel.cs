#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public record ResultSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; init; } = new();

        public List<FootnoteViewModel> Footnotes { get; init; } = new();

        public List<IndicatorMetaViewModel> Indicators { get; init; } = new();

        /// <summary>
        /// Legacy locations field replaced by <see cref="LocationsHierarchical"/>.
        /// </summary>
        /// <remarks>
        /// This should be safe to drop after the feature to add hierarchical locations in Table Result Subject Metadata
        /// becomes permanent, since a transformation from data in old Permalinks is made during deserialization by
        /// <see cref="ResultSubjectMetaViewModelJsonConverter"/>.
        /// </remarks>
        [Obsolete("TODO EES-2902 - Remove with SOW8 after EES-2777", false)]
        public List<ObservationalUnitMetaViewModel> Locations { get; init; } = new();

        /// <summary>
        /// Hierarchical locations field.
        /// </summary>
        /// <remarks>
        /// EES-2943: This could potentially be renamed back to 'Locations' but requires a migration of
        /// old Permalinks which already have a legacy 'Locations' field in their JSON serialization of type <see cref="List{ObservationalUnitMetaViewModel}"/>.
        /// </remarks>
        public Dictionary<string, List<LocationAttributeViewModel>> LocationsHierarchical { get; set; } = new();

        public List<BoundaryLevelViewModel> BoundaryLevels { get; init; } = new();

        public string PublicationName { get; init; } = string.Empty;

        public string SubjectName { get; init; } = string.Empty;

        public List<TimePeriodMetaViewModel> TimePeriodRange { get; init; } = new();

        public bool GeoJsonAvailable { get; init; }
    }
}
