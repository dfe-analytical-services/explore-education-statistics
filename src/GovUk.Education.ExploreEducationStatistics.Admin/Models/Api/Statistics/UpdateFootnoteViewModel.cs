using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class UpdateFootnoteViewModel
    {
        public string Content { get; set; }
        public IEnumerable<long> Filters { get; set; }
        public IEnumerable<long> FilterGroups { get; set; }
        public IEnumerable<long> FilterItems { get; set; }
        public IEnumerable<long> Indicators { get; set; }
    }
}