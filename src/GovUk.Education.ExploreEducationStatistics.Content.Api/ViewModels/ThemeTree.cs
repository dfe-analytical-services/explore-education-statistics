using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels
{
    public class ThemeTree
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public List<TopicTree> Topics { get; set; }
    }
}