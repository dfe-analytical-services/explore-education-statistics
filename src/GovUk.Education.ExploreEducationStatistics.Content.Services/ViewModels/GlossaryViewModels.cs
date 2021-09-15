#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class GlossaryEntryViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Slug { get; init; } = string.Empty;

        public string Body { get; init; } = string.Empty;
    }

    public class GlossaryCategoryViewModel
    {
        public string Heading { get; init; } = string.Empty;

        public List<GlossaryEntryViewModel> Entries { get; init; }
    }
}
