#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class GlossaryEntry
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public Guid CreatedById { get; set; }

        public User CreatedBy { get; set; } = null!;
    }
}
