using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public Theme Theme { get; set; }
        public Guid ThemeId { get; set; }
        public IEnumerable<Publication> Publications { get; set; }
    }
}