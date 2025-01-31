#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

public class SubjectMeta
{
    public List<(Filter Filter, string Column)> Filters { get; set; } = new();
    public List<(Indicator Indicator, string Column)> Indicators { get; set; } = new();
}
