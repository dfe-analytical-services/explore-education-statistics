#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class PermalinkResultSubjectMeta
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; init; } = new();

        public List<FootnoteViewModel> Footnotes { get; init; } = new();

        public List<IndicatorMetaViewModel> Indicators { get; init; } = new();

        /// <summary>
        /// Hierarchical locations field.
        /// </summary>
        /// <remarks>
        /// TODO EES-2943: This could potentially be renamed back to 'Locations' but requires a migration of
        /// old Permalinks which already have a legacy 'Locations' field in their JSON serialization of a different type.
        /// See <see cref="PermalinkResultSubjectMetaJsonConverter"/> which is doing the conversion.
        /// </remarks>
        public Dictionary<string, List<LocationAttributeViewModel>> LocationsHierarchical { get; set; } = new();

        public List<BoundaryLevelViewModel> BoundaryLevels { get; init; } = new();

        public string PublicationName { get; init; } = string.Empty;

        public string SubjectName { get; init; } = string.Empty;

        public List<TimePeriodMetaViewModel> TimePeriodRange { get; init; } = new();

        public bool GeoJsonAvailable { get; init; }

        public PermalinkResultSubjectMeta()
        {
        }

        public PermalinkResultSubjectMeta(SubjectResultMetaViewModel subjectResultMeta)
        {
            Filters = subjectResultMeta.Filters;
            Footnotes = subjectResultMeta.Footnotes;
            Indicators = subjectResultMeta.Indicators;
            LocationsHierarchical = subjectResultMeta.Locations;
            BoundaryLevels = subjectResultMeta.BoundaryLevels;
            PublicationName = subjectResultMeta.PublicationName;
            SubjectName = subjectResultMeta.SubjectName;
            TimePeriodRange = subjectResultMeta.TimePeriodRange;
            GeoJsonAvailable = subjectResultMeta.GeoJsonAvailable;
        }
    }
}
