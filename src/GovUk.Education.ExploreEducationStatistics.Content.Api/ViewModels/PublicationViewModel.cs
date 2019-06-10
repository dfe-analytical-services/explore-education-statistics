using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public string Theme { get; set; }

        public ContactViewModel Contact { get; set; }
    }
}