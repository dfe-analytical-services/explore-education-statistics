using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnotesViewModel
    {
        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }
        public IEnumerable<IdLabel> Subjects { get; set; }
    }
}