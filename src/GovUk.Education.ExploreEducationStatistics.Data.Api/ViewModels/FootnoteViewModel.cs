using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class FootnoteViewModel : IdLabelViewModel
    {
        public IEnumerable<string> Indicators { get; set; }
    }
}