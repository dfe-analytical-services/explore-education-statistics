#nullable enable
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersContentService(
    ContentDbContext contentDbContext,
    IPublicDataSetRepository publicDataSetRepository,
    IPublicDataApiClient publicDataApiClient
) : IEducationInNumbersContentService
{
    public async Task<Either<ActionResult, EinContentViewModel>> GetPageContent(
        Guid pageVersionId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
            .Include(pageVersion => pageVersion.Content)
                .ThenInclude(section => section.Content)
                    .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
                        .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .Where(pageVersion => pageVersion.Id == pageVersionId)
            .FirstOrNotFoundAsync(cancellationToken)
            .OnSuccess(EinContentViewModel.FromModel);
    }

    public async Task<Either<ActionResult, EinContentSectionViewModel>> AddSection(
        Guid pageVersionId,
        int order,
        CancellationToken cancellationToken
    )
    {
        var sectionList = await contentDbContext
            .EinContentSections.Where(section => section.EinPageVersionId == pageVersionId)
            .ToListAsync(cancellationToken);

        var newSection = new EinContentSection
        {
            Id = Guid.NewGuid(),
            EinPageVersionId = pageVersionId,
            Order = order,
            Heading = "New section",
            Content = [],
        };

        sectionList.Where(section => section.Order >= order).ForEach(section => section.Order++);

        contentDbContext.EinContentSections.Add(newSection);
        contentDbContext.EinContentSections.UpdateRange(sectionList);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return EinContentSectionViewModel.FromModel(newSection);
    }

    public async Task<Either<ActionResult, EinContentSectionViewModel>> UpdateSectionHeading(
        Guid pageVersionId,
        Guid sectionId,
        string heading,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentSections.Include(s => s.Content)
                .ThenInclude(b => (b as EinTileGroupBlock)!.Tiles)
                    .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .FirstOrNotFoundAsync(
                section => section.EinPageVersionId == pageVersionId && section.Id == sectionId,
                cancellationToken
            )
            .OnSuccess(async section =>
            {
                section.Heading = heading;
                contentDbContext.EinContentSections.Update(section);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return EinContentSectionViewModel.FromModel(section);
            });
    }

    public async Task<Either<ActionResult, List<EinContentSectionViewModel>>> ReorderSections(
        Guid pageVersionId,
        List<Guid> newSectionOrder,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinPageVersions.Include(p => p.Content)
                .ThenInclude(s => s.Content)
                    .ThenInclude(b => (b as EinTileGroupBlock)!.Tiles)
                        .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .SingleOrNotFoundAsync(p => p.Id == pageVersionId, cancellationToken)
            .OnSuccess(async pageVersion =>
            {
                var sectionList = pageVersion.Content;

                if (
                    !ComparerUtils.SequencesAreEqualIgnoringOrder(
                        sectionList.Select(section => section.Id),
                        newSectionOrder
                    )
                )
                {
                    return new Either<ActionResult, List<EinContentSectionViewModel>>(
                        ValidationUtils.ValidationActionResult(
                            ValidationErrorMessages.EinProvidedSectionIdsDifferFromActualSectionIds
                        )
                    );
                }

                newSectionOrder.ForEach(
                    (sectionId, order) =>
                    {
                        var matchingSection = sectionList.Single(section => section.Id == sectionId);
                        matchingSection.Order = order;
                    }
                );

                contentDbContext.EinContentSections.UpdateRange(sectionList);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return sectionList
                    .Select(EinContentSectionViewModel.FromModel)
                    .OrderBy(section => section.Order)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, List<EinContentSectionViewModel>>> DeleteSection(
        Guid pageVersionId,
        Guid sectionId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinPageVersions.Include(p => p.Content)
                .ThenInclude(section => section.Content)
                    .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
                        .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .SingleOrNotFoundAsync(p => p.Id == pageVersionId, cancellationToken)
            .OnSuccess(async pageVersion =>
            {
                var pageVersionSections = pageVersion.Content;

                var sectionToDelete = pageVersionSections.SingleOrDefault(section => section.Id == sectionId);

                if (sectionToDelete == null)
                {
                    return new Either<ActionResult, List<EinContentSectionViewModel>>(new NotFoundResult());
                }

                pageVersionSections.Remove(sectionToDelete);

                pageVersionSections // fix order of remaining sections
                    .Where(section => section.Order > sectionToDelete.Order)
                    .ForEach(section => section.Order--);

                contentDbContext.EinContentSections.UpdateRange(pageVersionSections);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return pageVersionSections
                    .Select(EinContentSectionViewModel.FromModel)
                    .OrderBy(section => section.Order)
                    .ToList();
            });
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> AddBlock(
        Guid pageVersionId,
        Guid sectionId,
        EinBlockType type,
        int? order,
        CancellationToken cancellationToken
    )
    {
        var blockList = await contentDbContext
            .EinContentBlocks.Where(block => block.EinContentSectionId == sectionId)
            .ToListAsync(cancellationToken);

        EinContentBlock newBlock = type switch
        {
            EinBlockType.HtmlBlock => new EinHtmlBlock
            {
                Id = Guid.NewGuid(),
                EinContentSectionId = sectionId,
                Order = order ?? blockList.Count,
                Body = "",
            },
            EinBlockType.TileGroupBlock => new EinTileGroupBlock
            {
                Id = Guid.NewGuid(),
                Title = null,
                EinContentSectionId = sectionId,
                Order = order ?? blockList.Count,
                Tiles = [],
            },
            _ => throw new Exception($"{nameof(EinContentBlock)} type {type} not found"),
        };

        blockList // fix order of preexisting blocks
            .Where(block => block.Order >= newBlock.Order)
            .ForEach(block => block.Order++);

        contentDbContext.EinContentBlocks.UpdateRange(blockList);
        contentDbContext.EinContentBlocks.Add(newBlock);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return EinContentBlockViewModel.FromModel(newBlock);
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> UpdateHtmlBlock(
        Guid pageVersionId,
        Guid sectionId,
        Guid htmlBlockId,
        EinHtmlBlockUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentBlocks.OfType<EinHtmlBlock>()
            .SingleOrNotFoundAsync(
                htmlBlock =>
                    htmlBlock.Id == htmlBlockId
                    && htmlBlock.EinContentSectionId == sectionId
                    && htmlBlock.EinContentSection.EinPageVersionId == pageVersionId,
                cancellationToken
            )
            .OnSuccess(async htmlBlockToUpdate =>
            {
                htmlBlockToUpdate.Body = request.Body;
                contentDbContext.EinContentBlocks.Update(htmlBlockToUpdate);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return EinContentBlockViewModel.FromModel(htmlBlockToUpdate);
            });
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> UpdateTileGroupBlock(
        Guid pageVersionId,
        Guid sectionId,
        Guid tileGroupBlockId,
        EinTileGroupBlockUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentBlocks.OfType<EinTileGroupBlock>()
            .Include(groupBlock => groupBlock.Tiles)
                .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .SingleOrNotFoundAsync(
                tileGroupBlock =>
                    tileGroupBlock.Id == tileGroupBlockId
                    && tileGroupBlock.EinContentSectionId == sectionId
                    && tileGroupBlock.EinContentSection.EinPageVersionId == pageVersionId,
                cancellationToken
            )
            .OnSuccess(async tileGroupBlockToUpdate =>
            {
                tileGroupBlockToUpdate.Title = request.Title;
                contentDbContext.EinContentBlocks.Update(tileGroupBlockToUpdate);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return EinContentBlockViewModel.FromModel(tileGroupBlockToUpdate);
            });
    }

    public async Task<Either<ActionResult, List<EinContentBlockViewModel>>> ReorderBlocks(
        Guid pageVersionId,
        Guid sectionId,
        List<Guid> newBlockOrder,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentSections.Include(p => p.Content)
                .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
                    .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .SingleOrNotFoundAsync(s => s.Id == sectionId && s.EinPageVersionId == pageVersionId, cancellationToken)
            .OnSuccess(async section =>
            {
                var blockList = section.Content;

                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(blockList.Select(block => block.Id), newBlockOrder))
                {
                    return new Either<ActionResult, List<EinContentBlockViewModel>>(
                        ValidationUtils.ValidationActionResult(
                            ValidationErrorMessages.EinProvidedBlockIdsDifferFromActualBlockIds
                        )
                    );
                }

                newBlockOrder.ForEach(
                    (blockId, order) =>
                    {
                        var matchingBlock = blockList.Single(block => block.Id == blockId);
                        matchingBlock.Order = order;
                    }
                );

                contentDbContext.EinContentBlocks.UpdateRange(blockList);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return blockList.Select(EinContentBlockViewModel.FromModel).OrderBy(block => block.Order).ToList();
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteBlock(
        Guid pageVersionId,
        Guid sectionId,
        Guid blockId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentSections.Include(section => section.Content)
                .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .SingleOrNotFoundAsync(s => s.Id == sectionId && s.EinPageVersionId == pageVersionId, cancellationToken)
            .OnSuccess(async section =>
            {
                var blockList = section.Content;

                var blockToDelete = blockList.SingleOrDefault(block => block.Id == blockId);

                if (blockToDelete == null)
                {
                    return new Either<ActionResult, Unit>(new NotFoundResult());
                }

                blockList.Remove(blockToDelete);

                blockList // fix order of remaining blocks
                    .Where(block => block.Order > blockToDelete.Order)
                    .ForEach(block => block.Order--);

                await contentDbContext.SaveChangesAsync(cancellationToken);

                return Unit.Instance;
            });
    }

    public async Task<Either<ActionResult, EinTileViewModel>> AddTile(
        Guid pageVersionId,
        Guid parentBlockId,
        EinTileType type,
        int? order,
        CancellationToken cancellationToken
    )
    {
        var tileList = await contentDbContext
            .EinTiles.Where(tile =>
                tile.EinParentBlockId == parentBlockId
                && tile.EinParentBlock.EinContentSection.EinPageVersionId == pageVersionId
            )
            .ToListAsync(cancellationToken);

        EinTile newTile = type switch
        {
            EinTileType.FreeTextStatTile => new EinFreeTextStatTile
            {
                EinParentBlockId = parentBlockId,
                Order = order ?? tileList.Count,
            },
            EinTileType.ApiQueryStatTile => new EinApiQueryStatTile
            {
                EinParentBlockId = parentBlockId,
                Order = order ?? tileList.Count,
            },
            _ => throw new Exception($"{nameof(EinTile)} type {type} not found"),
        };

        tileList // fix order of preexisting tiles
            .Where(tile => tile.Order >= newTile.Order)
            .ForEach(tile => tile.Order++);

        contentDbContext.EinTiles.UpdateRange(tileList);
        contentDbContext.EinTiles.Add(newTile);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return EinTileViewModel.FromModel(newTile);
    }

    public async Task<Either<ActionResult, EinTileViewModel>> UpdateFreeTextStatTile(
        Guid pageVersionId,
        Guid tileId,
        EinFreeTextStatTileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinTiles.OfType<EinFreeTextStatTile>()
            .SingleOrNotFoundAsync(
                tile => tile.Id == tileId && tile.EinParentBlock.EinContentSection.EinPageVersionId == pageVersionId,
                cancellationToken
            )
            .OnSuccess(async tileToUpdate =>
            {
                tileToUpdate.Title = request.Title;
                tileToUpdate.Statistic = request.Statistic;
                tileToUpdate.Trend = request.Trend;
                tileToUpdate.LinkUrl = request.LinkUrl;
                tileToUpdate.LinkText = request.LinkText;

                contentDbContext.EinTiles.Update(tileToUpdate);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return EinTileViewModel.FromModel(tileToUpdate);
            });
    }

    public async Task<Either<ActionResult, EinTileViewModel>> UpdateApiQueryStatTile(
        Guid pageVersionId,
        Guid tileId,
        EinApiQueryStatTileUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        // Get tile to update
        return await contentDbContext
            .EinTiles.OfType<EinApiQueryStatTile>()
            .SingleOrNotFoundAsync(
                tile => tile.Id == tileId && tile.EinParentBlock.EinContentSection.EinPageVersionId == pageVersionId,
                cancellationToken
            )
            .OnSuccess(async tileToUpdate =>
            {
                // Get indicator PublicId
                var indicatorPublicId = FetchSingleIndicator(request.Query);
                if (indicatorPublicId == null)
                {
                    return new Either<ActionResult, EinTileViewModel>(
                        new BadRequestObjectResult("Request query must contain exactly one indicator")
                    );
                }

                // Get data from publicDataDbContext where possible
                var apiDataSet = await publicDataSetRepository.GetDataSet(request.DataSetId, cancellationToken);
                if (apiDataSet.LatestLiveVersion == null)
                {
                    return new BadRequestObjectResult("API data set has no live version");
                }

                var apiDataSetLatestVersion = apiDataSet.LatestLiveVersion;

                var latestVersion =
                    $"{apiDataSetLatestVersion.VersionMajor}.{apiDataSetLatestVersion.VersionMinor}.{apiDataSetLatestVersion.VersionPatch}";
                if (latestVersion != request.Version) // we always expect the full api data set version to be provided in the request
                {
                    return new BadRequestObjectResult(
                        $"Version provided isn't the latest version. Latest: {latestVersion} Provided: {request.Version}"
                    );
                }

                var releaseInfo = await contentDbContext
                    .ReleaseFiles.Where(rf => rf.Id == apiDataSetLatestVersion.Release.ReleaseFileId)
                    .Select(rf => new
                    {
                        rf.ReleaseVersion.ReleaseId,
                        ReleaseSlug = rf.ReleaseVersion.Release.Slug,
                        PublicationSlug = rf.ReleaseVersion.Release.Publication.Slug,
                    })
                    .SingleAsync(cancellationToken);

                var indicatorMeta = await publicDataSetRepository.GetIndicatorMeta(
                    apiDataSetLatestVersion.Id,
                    indicatorPublicId,
                    cancellationToken
                );
                if (indicatorMeta == null)
                {
                    return new BadRequestObjectResult(
                        $"Could not find indicator meta for {indicatorPublicId} for API data set {apiDataSetLatestVersion.Id}"
                    );
                }

                var indicatorUnit = indicatorMeta.Unit ?? IndicatorUnit.None;
                var indicatorDecimalPlaces = indicatorMeta.DecimalPlaces;

                // Make the actual PAPI query
                return await publicDataApiClient
                    .QueryDataSetPost(request.DataSetId, request.Version, request.Query, cancellationToken)
                    .OnSuccess(async queryResults =>
                    {
                        if (queryResults.Warnings.Count > 0)
                        {
                            return new Either<ActionResult, EinTileViewModel>(
                                new BadRequestObjectResult(
                                    $"PAPI query returned warnings: {queryResults.Warnings.Select(w => w.Message).JoinToString(',')}"
                                )
                            );
                        }

                        if (queryResults.Paging.TotalPages > 1)
                        {
                            return new BadRequestObjectResult("Results need to all fit on the first page");
                        }

                        if (queryResults.Results.Count == 0)
                        {
                            return new BadRequestObjectResult("PAPI query returned no results");
                        }

                        var latestResults = FetchLatestYearNationalResults(queryResults);

                        if (latestResults.Count != 1)
                        {
                            return new BadRequestObjectResult(
                                $"Should only be one result with NAT and latest year. Found {latestResults.Count} results"
                            );
                        }

                        var theStat = latestResults[0].Values[indicatorPublicId];

                        tileToUpdate.Title = request.Title;
                        tileToUpdate.DataSetId = request.DataSetId;
                        tileToUpdate.Version = request.Version;
                        tileToUpdate.DataSetVersionId = apiDataSetLatestVersion.Id; // we check this matches the latest version above
                        tileToUpdate.LatestDataSetVersionId = apiDataSetLatestVersion.Id;
                        tileToUpdate.Query = request.Query;
                        tileToUpdate.Statistic = theStat;
                        tileToUpdate.IndicatorUnit = indicatorUnit;
                        tileToUpdate.DecimalPlaces = indicatorDecimalPlaces;
                        tileToUpdate.QueryResult = JsonSerializer.Serialize(queryResults.Results);
                        tileToUpdate.ReleaseId = releaseInfo.ReleaseId;

                        await contentDbContext.SaveChangesAsync(cancellationToken);

                        if (tileToUpdate.ReleaseId != null)
                        {
                            // need to refetch so FromModel can get Publication.Slug and Release.Slug
                            tileToUpdate = await contentDbContext
                                .EinTiles.OfType<EinApiQueryStatTile>()
                                .Include(apiTile => apiTile.Release!.Publication)
                                .SingleAsync(apiTile => apiTile.Id == tileToUpdate.Id, cancellationToken);
                        }

                        return EinTileViewModel.FromModel(tileToUpdate);
                    });
            });
    }

    public async Task<Either<ActionResult, List<EinTileViewModel>>> ReorderTiles(
        Guid pageVersionId,
        Guid parentBlockId,
        List<Guid> newTileOrder,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentBlocks.OfType<EinTileGroupBlock>()
            .Include(parentBlock => parentBlock.Tiles)
                .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .SingleOrNotFoundAsync(
                parentBlock =>
                    parentBlock.Id == parentBlockId && parentBlock.EinContentSection.EinPageVersionId == pageVersionId,
                cancellationToken
            )
            .OnSuccess(async parentBlock =>
            {
                var tileList = parentBlock.Tiles;

                if (!ComparerUtils.SequencesAreEqualIgnoringOrder(tileList.Select(tile => tile.Id), newTileOrder))
                {
                    return new Either<ActionResult, List<EinTileViewModel>>(
                        ValidationUtils.ValidationActionResult(
                            ValidationErrorMessages.EinProvidedTileIdsDifferFromActualTileIds
                        )
                    );
                }

                newTileOrder.ForEach(
                    (tileId, order) =>
                    {
                        var matching = tileList.Single(tile => tile.Id == tileId);
                        matching.Order = order;
                    }
                );

                contentDbContext.EinTiles.UpdateRange(tileList);
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return tileList.Select(EinTileViewModel.FromModel).OrderBy(tile => tile.Order).ToList();
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteTile(
        Guid pageVersionId,
        Guid blockId,
        Guid tileId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinContentBlocks.OfType<EinTileGroupBlock>()
            .Include(block => block.Tiles)
                .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .SingleOrNotFoundAsync(
                block => block.Id == blockId && block.EinContentSection.EinPageVersionId == pageVersionId,
                cancellationToken
            )
            .OnSuccess(async block =>
            {
                var tileList = block.Tiles;

                var tileToDelete = tileList.SingleOrDefault(tile => tile.Id == tileId);

                if (tileToDelete == null)
                {
                    return new Either<ActionResult, Unit>(new NotFoundResult());
                }

                tileList.Remove(tileToDelete);

                tileList // fix order of remaining tiles
                    .Where(tile => tile.Order > tileToDelete.Order)
                    .ForEach(tile => tile.Order--);

                await contentDbContext.SaveChangesAsync(cancellationToken);

                return Unit.Instance;
            });
    }

    private static string? FetchSingleIndicator(string queryString)
    {
        using var doc = JsonDocument.Parse(queryString);

        if (
            !doc.RootElement.TryGetProperty("indicators", out var indicators)
            || indicators.ValueKind != JsonValueKind.Array
            || indicators.GetArrayLength() != 1
            || indicators[0].ValueKind != JsonValueKind.String
        )
        {
            return null;
        }
        return indicators[0].GetString();
    }

    private static List<DataSetQueryResultViewModel> FetchLatestYearNationalResults(
        DataSetQueryPaginatedResultsViewModel queryResults
    )
    {
        var latestTimePeriod = queryResults
            .Results.OrderBy(result => result.TimePeriod.Period[..4])
            .ThenBy(result => result.TimePeriod.Code)
            .ToList()
            .Select(result => result.TimePeriod)
            .Last();

        return queryResults
            .Results.Where(result => result.GeographicLevel == GeographicLevel.Country)
            .Where(result =>
                result.TimePeriod.Period == latestTimePeriod.Period && result.TimePeriod.Code == latestTimePeriod.Code
            )
            .ToList();
    }
}
