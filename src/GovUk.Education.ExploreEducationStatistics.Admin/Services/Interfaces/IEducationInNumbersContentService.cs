#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersContentService
{
    Task<Either<ActionResult, EinContentViewModel>> GetPageContent(Guid pageId);

    Task<Either<ActionResult, EinContentSectionViewModel>> AddSection(Guid pageId, int order);

    Task<Either<ActionResult, EinContentSectionViewModel>> UpdateSectionHeading(
        Guid pageId,
        Guid sectionId,
        string heading
    );

    Task<Either<ActionResult, List<EinContentSectionViewModel>>> ReorderSections(
        Guid pageId,
        List<Guid> newSectionOrder
    );

    Task<Either<ActionResult, List<EinContentSectionViewModel>>> DeleteSection(Guid pageId, Guid sectionId);

    Task<Either<ActionResult, EinContentBlockViewModel>> AddBlock(
        Guid pageId,
        Guid sectionId,
        EinBlockType type,
        int? order
    );

    Task<Either<ActionResult, EinContentBlockViewModel>> UpdateHtmlBlock(
        Guid pageId,
        Guid sectionId,
        Guid htmlBlockId,
        EinHtmlBlockUpdateRequest request
    );

    Task<Either<ActionResult, EinContentBlockViewModel>> UpdateTileGroupBlock(
        Guid pageId,
        Guid sectionId,
        Guid tileGroupBlockId,
        EinTileGroupBlockUpdateRequest request
    );

    Task<Either<ActionResult, List<EinContentBlockViewModel>>> ReorderBlocks(
        Guid pageId,
        Guid sectionId,
        List<Guid> newBlockOrder
    );

    Task<Either<ActionResult, Unit>> DeleteBlock(Guid pageId, Guid sectionId, Guid blockId);

    Task<Either<ActionResult, EinTileViewModel>> AddTile(Guid pageId, Guid parentBlockId, EinTileType type, int? order);

    Task<Either<ActionResult, EinTileViewModel>> UpdateFreeTextStatTile(
        Guid pageId,
        Guid tileId,
        EinFreeTextStatTileUpdateRequest request
    );

    Task<Either<ActionResult, List<EinTileViewModel>>> ReorderTiles(
        Guid pageId,
        Guid parentBlockId,
        List<Guid> newTileOrder
    );

    Task<Either<ActionResult, Unit>> DeleteTile(Guid pageId, Guid blockId, Guid tileId);
}
