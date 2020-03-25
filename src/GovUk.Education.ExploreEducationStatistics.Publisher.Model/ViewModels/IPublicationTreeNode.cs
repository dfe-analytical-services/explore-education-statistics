using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public interface IPublicationTreeNode
    {
        Guid Id { get; set; }

        string Title { get; set; }

        string Slug { get; set; }

        string Summary { get; set; }
    }
}