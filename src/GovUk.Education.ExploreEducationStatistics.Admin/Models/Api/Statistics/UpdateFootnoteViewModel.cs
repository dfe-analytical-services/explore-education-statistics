using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class UpdateFootnoteViewModel
    {
        public string Content { get; set; }
        public IReadOnlyCollection<Guid> Filters { get; set; }
        public IReadOnlyCollection<Guid> FilterGroups { get; set; }
        public IReadOnlyCollection<Guid> FilterItems { get; set; }
        public IReadOnlyCollection<Guid> Indicators { get; set; }
        public IReadOnlyCollection<Guid> Subjects { get; set; }
    }
}