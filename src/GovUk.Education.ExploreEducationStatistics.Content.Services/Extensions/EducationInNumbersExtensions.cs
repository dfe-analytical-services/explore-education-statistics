using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersContentViewModels;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;

public static class EducationInNumbersExtensions
{
    public static EinNavItemViewModel ToNavItemViewModel(
        this EducationInNumbersPage page)
    {
            return new EinNavItemViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Published = page.Published!.Value, // we only display published Ein pages publicly
                Order = page.Order,
            };
    }

    public static EinPageViewModel ToPageViewModel(
        this EducationInNumbersPage page)
    {
            return new EinPageViewModel
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

    private static EinContentSectionViewModel ToViewModel(
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

    private static EinContentBlockViewModel ToViewModel(this EinContentBlock block)
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
