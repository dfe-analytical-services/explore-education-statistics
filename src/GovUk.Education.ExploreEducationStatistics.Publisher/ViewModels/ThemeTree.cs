using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels
{
    public class ThemeTree
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public List<TopicTree> Topics { get; set; }
    }
}