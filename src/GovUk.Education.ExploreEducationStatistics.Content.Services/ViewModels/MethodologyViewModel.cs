using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class MethodologyViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? Updated { get; set; }

        public List<ContentSectionViewModel> Content { get; set; }

        public List<ContentSectionViewModel> Annexes { get; set; }
    }
}
