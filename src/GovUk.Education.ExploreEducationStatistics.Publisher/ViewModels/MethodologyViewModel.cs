using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels
{
    public class MethodologyViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? LastUpdated { get; set; }

        public List<ContentSectionViewModel> Content { get; set; }

        public List<ContentSectionViewModel> Annexes { get; set; }
    }
}