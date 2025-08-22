using System;
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
                Type = EinBlockType.EinHtmlBlock,
                Body = htmlBlock.Body,
            },
            _ => throw new Exception("Ein block type not found")
        };
    }
}
