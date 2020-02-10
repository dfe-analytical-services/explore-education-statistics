using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public interface IContentBlockViewModel
    {
        Guid Id { get; set; }

        int Order { get; set; }

        string Type { get; }
    }
}