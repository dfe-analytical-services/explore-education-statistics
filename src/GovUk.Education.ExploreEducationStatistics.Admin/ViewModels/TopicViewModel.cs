using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class TopicViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public Guid ThemeId { get; set; }

        public string Summary { get; set; }

        public List<MyPublicationViewModel> Publications { get; set; }
    }
}