#nullable enable

using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class GlossaryEntry
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Body { get; set; }

        public DateTime Created { get; set; }

        public Guid CreatedById { get; set; }

        public User CreatedBy { get; set; }
    }
}
