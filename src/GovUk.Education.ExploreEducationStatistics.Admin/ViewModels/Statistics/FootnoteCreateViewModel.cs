using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteCreateViewModel
    {
        public string Content { get; set; }
        public IReadOnlyCollection<Guid> Filters { get; set; }
        public IReadOnlyCollection<Guid> FilterGroups { get; set; }
        public IReadOnlyCollection<Guid> FilterItems { get; set; }
        public IReadOnlyCollection<Guid> Indicators { get; set; }
        public IReadOnlyCollection<Guid> Locations { get; set; }
        public IReadOnlyCollection<FootnoteTimePeriodViewModel> TimePeriods { get; set; }
        public IReadOnlyCollection<Guid> Subjects { get; set; }
    }
}
