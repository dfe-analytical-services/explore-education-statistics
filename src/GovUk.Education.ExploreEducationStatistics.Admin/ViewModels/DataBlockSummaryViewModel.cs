#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class DataBlockSummaryViewModel
    {
        public Guid Id { get; set; }

        public string Heading { get; set; } = "";

        public string Name { get; set; } = "";

        public DateTime? Created { get; set; }

        public string? HighlightName { get; set; }

        public string? HighlightDescription { get; set; }

        public string Source { get; set; } = "";

        public bool InContent { get; set; }

        public int ChartsCount { get; set; }
    }
}
