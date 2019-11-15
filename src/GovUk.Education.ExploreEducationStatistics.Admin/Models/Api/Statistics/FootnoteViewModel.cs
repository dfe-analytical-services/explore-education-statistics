using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnoteViewModel
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public Dictionary<long, FootnoteSubjectViewModel> Subjects { get; set; }
    }
}