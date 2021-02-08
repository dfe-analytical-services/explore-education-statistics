using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public Dictionary<Guid, FootnoteSubjectViewModel> Subjects { get; set; }
    }
}