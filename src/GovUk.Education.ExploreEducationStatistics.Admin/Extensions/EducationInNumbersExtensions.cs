using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class EducationInNumbersExtensions
{
    public static EinSummaryViewModel ToSummaryViewModel(this EducationInNumbersPage page)
    {
        return new EinSummaryViewModel
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

    public static EinSummaryWithPrevVersionViewModel ToViewModel(
        this EducationInNumbersPage page,
        Guid? previousVersionId
    )
    {
        return new EinSummaryWithPrevVersionViewModel
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
