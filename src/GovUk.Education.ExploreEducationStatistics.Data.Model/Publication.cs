using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Publication
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public Topic Topic { get; set; }
        public Guid TopicId { get; set; }
        public IEnumerable<Release> Releases { get; set; }
    }
}