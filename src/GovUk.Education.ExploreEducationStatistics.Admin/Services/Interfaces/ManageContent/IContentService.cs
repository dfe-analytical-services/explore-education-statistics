#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IContentService
{
    public Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid releaseVersionId) where T : ContentBlock;

    Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
        Guid releaseVersionId,
        Dictionary<Guid, int> newSectionOrder);

    Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
        Guid releaseVersionId,
        ContentSectionAddRequest? request);

    Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
        Guid releaseVersionId,
        Guid contentSectionId,
        string newHeading);

    Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
        Guid releaseVersionId,
        Guid contentSectionId);

    Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
        Guid releaseVersionId,
        Guid contentSectionId,
        Dictionary<Guid, int> newBlocksOrder);

    Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        ContentBlockAddRequest request);

    Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId);

    Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        Guid contentBlockId,
        ContentBlockUpdateRequest request);

    Task<Either<ActionResult, DataBlockViewModel>> AttachDataBlock(
        Guid releaseVersionId,
        Guid contentSectionId,
        DataBlockAttachRequest request);
}
