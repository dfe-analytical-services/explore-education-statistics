#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Footnote : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }

        public List<IndicatorFootnote> Indicators { get; set; } = new();

        public List<FilterFootnote> Filters { get; set; } = new();

        public List<FilterGroupFootnote> FilterGroups { get; set; } = new();

        public List<FilterItemFootnote> FilterItems { get; set; } = new();

        public List<SubjectFootnote> Subjects { get; set; } = new();

        public List<ReleaseFootnote> Releases { get; set; } = new();
    }
}
