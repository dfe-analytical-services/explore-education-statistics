using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class TopicViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public Guid ThemeId { get; set; }

        public string Summary { get; set; }

        public List<PublicationViewModel> Publications { get; set; }
    }
}