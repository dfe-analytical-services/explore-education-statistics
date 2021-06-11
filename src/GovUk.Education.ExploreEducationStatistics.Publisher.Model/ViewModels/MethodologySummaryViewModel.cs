using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class MethodologySummaryViewModel
    {
        public Guid Id { get; set; }

        public string Slug { get; set; }

        // TODO SOW4 EES-2375 check if this is used as it won't be mapped
        public string Summary { get; set; }

        // TODO SOW4 EES-2375 check if this is used as it won't be mapped
        public string Title { get; set; }
    }
}
