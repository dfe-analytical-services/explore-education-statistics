#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public interface IContentBlockViewModel
    {
        Guid Id { get; set; }

        int Order { get; set; }

        string Type { get; }
    }
}
