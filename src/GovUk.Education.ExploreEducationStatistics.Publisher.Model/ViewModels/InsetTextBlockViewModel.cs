using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class InsetTextBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Body { get; set; }

        public string Heading { get; set; }

        public string Type => "InsetTextBlock";
    }
}