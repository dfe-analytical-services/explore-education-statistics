using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;

public class BoundaryLevelViewModel
{
    public long Id { get; set; }

    public string Level { get; set; }

    public string Label { get; set; }

    public DateTime Published { get; set; }
}
