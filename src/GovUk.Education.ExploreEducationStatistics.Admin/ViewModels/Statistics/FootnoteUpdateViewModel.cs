using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteUpdateViewModel
    {
        public string Content { get; set; }
        public IReadOnlyCollection<Guid> Filters { get; set; }
        public IReadOnlyCollection<Guid> FilterGroups { get; set; }
        public IReadOnlyCollection<Guid> FilterItems { get; set; }
        public IReadOnlyCollection<Guid> Indicators { get; set; }
        public IReadOnlyCollection<Guid> Subjects { get; set; }
    }
}