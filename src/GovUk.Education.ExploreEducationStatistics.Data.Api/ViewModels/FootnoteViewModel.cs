using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class FootnoteViewModel
    {
        public IEnumerable<int> Indicators { get; set; }
        public string Value { get; set; }
    }
}