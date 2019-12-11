using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class CreateFootnoteViewModel
    {
        public string Content { get; set; }
        public IEnumerable<Guid> Filters { get; set; }
        public IEnumerable<Guid> FilterGroups { get; set; }
        public IEnumerable<Guid> FilterItems { get; set; }
        public IEnumerable<Guid> Indicators { get; set; }
        public IEnumerable<Guid> Subjects { get; set; }
    }
}