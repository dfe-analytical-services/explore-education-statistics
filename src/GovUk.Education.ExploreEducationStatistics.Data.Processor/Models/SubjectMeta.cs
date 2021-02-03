using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class SubjectMeta
    {
        public IEnumerable<(Filter Filter, string Column, string FilterGroupingColumn)> Filters { get; set; }
        public IEnumerable<(Indicator Indicator, string Column)> Indicators { get; set; }
    }
}