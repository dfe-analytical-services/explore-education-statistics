using System;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class EducationInNumbersExtensions
{
    public static EducationInNumbersSummaryViewModel ToViewModel(
        this EducationInNumbersPage page)
    {
            return new EducationInNumbersSummaryViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Description = page.Description,
                Version = page.Version,
                Published = page.Published,
                Order = page.Order,
            };
    }

    public static EducationInNumbersSummaryWithPrevVersionViewModel ToViewModel(
        this EducationInNumbersPage page,
        Guid? previousVersionId)
    {
        return new EducationInNumbersSummaryWithPrevVersionViewModel
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
