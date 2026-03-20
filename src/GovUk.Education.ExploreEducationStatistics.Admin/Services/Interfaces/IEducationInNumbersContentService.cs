#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersContentService
{
    Task<Either<ActionResult, EinContentViewModel>> GetPageContent(Guid pageId, CancellationToken cancellationToken);

    Task<Either<ActionResult, EinContentSectionViewModel>> AddSection(
        Guid pageId,
        int order,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinContentSectionViewModel>> UpdateSectionHeading(
        Guid pageId,
        Guid sectionId,
        string heading,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, List<EinContentSectionViewModel>>> ReorderSections(
        Guid pageId,
        List<Guid> newSectionOrder,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, List<EinContentSectionViewModel>>> DeleteSection(
        Guid pageId,
        Guid sectionId,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinContentBlockViewModel>> AddBlock(
        Guid pageId,
        Guid sectionId,
        EinBlockType type,
        int? order,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinContentBlockViewModel>> UpdateHtmlBlock(
        Guid pageId,
        Guid sectionId,
        Guid htmlBlockId,
        EinHtmlBlockUpdateRequest request,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinContentBlockViewModel>> UpdateTileGroupBlock(
        Guid pageId,
        Guid sectionId,
        Guid tileGroupBlockId,
        EinTileGroupBlockUpdateRequest request,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, List<EinContentBlockViewModel>>> ReorderBlocks(
        Guid pageId,
        Guid sectionId,
        List<Guid> newBlockOrder,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, Unit>> DeleteBlock(
        Guid pageId,
        Guid sectionId,
        Guid blockId,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinTileViewModel>> AddTile(
        Guid pageId,
        Guid parentBlockId,
        EinTileType type,
        int? order,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinTileViewModel>> UpdateFreeTextStatTile(
        Guid pageId,
        Guid tileId,
        EinFreeTextStatTileUpdateRequest request,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, EinTileViewModel>> UpdateApiQueryStatTile(
        Guid pageId,
        Guid tileId,
        EinApiQueryStatTileUpdateRequest request,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, List<EinTileViewModel>>> ReorderTiles(
        Guid pageId,
        Guid parentBlockId,
        List<Guid> newTileOrder,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, Unit>> DeleteTile(
        Guid pageId,
        Guid blockId,
        Guid tileId,
        CancellationToken cancellationToken
    );
}
