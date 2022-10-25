#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public enum SubjectMetaQueryStep
    {
        Locations,
        TimePeriods,
        FilterItems,
        Indicators,
    }

    public record IncludeInResponseQuery
    {
        public SubjectMetaQueryStep Step { get; set; }
    }
}
