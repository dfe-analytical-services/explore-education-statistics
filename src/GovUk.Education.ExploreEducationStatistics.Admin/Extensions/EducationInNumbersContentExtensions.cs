using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class EducationInNumbersContentExtensions
{
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
                EinContentSectionId = newSectionId,
                Body = htmlBlock.Body,
            },
            EinTileGroupBlock groupBlock => new EinTileGroupBlock
            {
                Id = Guid.NewGuid(),
                Order = groupBlock.Order,
                EinContentSectionId = newSectionId,
                Title = groupBlock.Title,
                Tiles = groupBlock.Tiles
                    .Select(tile => tile.Clone(groupBlock.Id))
                    .OrderBy(tile => tile.Order)
                    .ToList(),
            },
            _ => throw new Exception($"{nameof(EinContentBlock)} type {block.GetType()} not found")
        };
    }

    private static EinTile Clone(this EinTile tile, Guid groupBlockId)
    {
        return tile switch
        {
            EinFreeTextStatTile statTile => new EinFreeTextStatTile
            {
                Id = Guid.NewGuid(),
                Order = statTile.Order,
                EinParentBlockId = groupBlockId,
                Title = statTile.Title,
                Statistic = statTile.Statistic,
                Trend = statTile.Trend,
                LinkUrl = statTile.LinkUrl,
                LinkText = statTile.LinkText,
            },
            _ => throw new Exception($"{nameof(EinTile)} type {tile.GetType()} not found")
        };
    }
}
