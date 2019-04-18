using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Models
{
    public class SubjectMeta
    {
        public IEnumerable<(Filter Filter, string FilterGroupingColumn)> Filters { get; set; }
        public IEnumerable<IndicatorGroup> IndicatorGroups { get; set; }
    }
}