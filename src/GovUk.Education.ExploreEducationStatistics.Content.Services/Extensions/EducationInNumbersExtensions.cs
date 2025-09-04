using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;

public static class EducationInNumbersExtensions
{
    public static EducationInNumbersViewModels.EinNavItemViewModel ToNavItemViewModel(
        this EducationInNumbersPage page)
    {
            return new EducationInNumbersViewModels.EinNavItemViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Published = page.Published!.Value, // we only display published Ein pages publicly
                Order = page.Order,
            };
    }

    public static EducationInNumbersViewModels.EinPageViewModel ToPageViewModel(
        this EducationInNumbersPage page)
    {
            return new EducationInNumbersViewModels.EinPageViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Description = page.Description,
                Published = page.Published!.Value, // we only display published Ein pages publicly
                Content = page.Content
                    .Select(section => section.ToViewModel())
                    .OrderBy(section => section.Order)
                    .ToList(),
            };
    }

    public static EinContentSectionViewModel ToViewModel( // @MarkFix duplicated
        this EinContentSection section)
    {
            return new EinContentSectionViewModel
            {
                Id = section.Id,
                Order = section.Order,
                Heading = section.Heading,
                Caption = section.Caption,
                Content = section.Content
                    .Select(block => block.ToViewModel())
                    .OrderBy(block => block.Order)
                    .ToList(),
            };
    }

    public static EinContentBlockViewModel ToViewModel(this EinContentBlock block) // @MarkFix duplicated
    {
        return block switch
        {
            EinHtmlBlock htmlBlock => new EinHtmlBlockViewModel
            {
                Id = htmlBlock.Id,
                Order = htmlBlock.Order,
                Type = EinBlockType.HtmlBlock,
                Body = htmlBlock.Body,
            },
            _ => throw new Exception($"{nameof(EinContentBlock)} type {block.GetType()} not found")
        };
    }
}
