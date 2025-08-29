using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class EducationInNumbersContentExtensions
{
    public static EinContentSectionViewModel ToViewModel(
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

    public static EinContentBlockViewModel ToViewModel(this EinContentBlock block)
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
            _ => throw new Exception("Ein block type not found")
        };
    }

    public static EinContentSection Clone(this EinContentSection section, Guid newPageId)
    {
        var newSectionId = Guid.NewGuid();

        return new EinContentSection
        {
            Id = newSectionId,
            Order = section.Order,
            Heading = section.Heading,
            Caption = section.Caption,
            EducationInNumbersPageId = newPageId,
            Content = section.Content
                .Select(block => block.Clone(newSectionId))
                .OrderBy(block => block.Order)
                .ToList(),
        };
    }

    private static EinContentBlock Clone(this EinContentBlock block, Guid newSectionId)
    {
        return block switch
        {
            EinHtmlBlock htmlBlock => new EinHtmlBlock
            {
                Id = Guid.NewGuid(),
                Order = htmlBlock.Order,
                Body = htmlBlock.Body,
                EinContentSectionId = newSectionId,
            },
            _ => throw new Exception("Ein block type not found")
        };
    }
}
