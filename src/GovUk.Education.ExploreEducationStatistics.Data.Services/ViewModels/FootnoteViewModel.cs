using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class FootnoteViewModel : IdLabel
    {
        public FootnoteViewModel(Guid id, string label) : base(id, label)
        {
        }

        public FootnoteViewModel()
        {
        }
    }
}