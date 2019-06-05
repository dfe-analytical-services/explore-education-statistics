using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels
{
    public class TopicTree
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }

        public List<PublicationTree> Publications { get; set; }
    }
}