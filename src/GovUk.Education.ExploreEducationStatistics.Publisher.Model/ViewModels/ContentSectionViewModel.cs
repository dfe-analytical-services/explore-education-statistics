using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class ContentSectionViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<IContentBlockViewModel> Content { get; set; }
    }
}