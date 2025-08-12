using System;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class EducationInNumbersExtensions
{
    public static EducationInNumbersPageViewModel ToViewModel(
        this EducationInNumbersPage page,
        Guid? previousVersionId = null)
    {
        return new EducationInNumbersPageViewModel
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Description = page.Description,
            Version = page.Version,
            Published = page.Published,
            Order = page.Order,
            PreviousVersionId = previousVersionId,
        };
    }
}
