#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels
{
    public class GlossaryEntryViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }

    public class GlossaryCategoryViewModel
    {
        public string Heading { get; set; } = string.Empty;

        public List<GlossaryEntryViewModel> Entries { get; set; }
    }
}
