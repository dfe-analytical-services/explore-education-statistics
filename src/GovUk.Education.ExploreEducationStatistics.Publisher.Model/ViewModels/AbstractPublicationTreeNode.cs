using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class AbstractPublicationTreeNode : IPublicationTreeNode
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }
    }
}