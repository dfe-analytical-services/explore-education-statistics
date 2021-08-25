#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IContentService
    {
        Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSections(
            Guid releaseId);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder);

        Task<Either<ActionResult, ContentSectionViewModel>> AddContentSection(
            Guid releaseId, ContentSectionAddRequest? request);

        Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid contentSectionId, string newHeading);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId, Guid contentSectionId);

        Task<Either<ActionResult, ContentSectionViewModel>> GetContentSection(
            Guid contentSectionId);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder);

        Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(
            Guid contentSectionId,
            ContentBlockAddRequest request);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);

        Task<Either<ActionResult, IContentBlockViewModel>> UpdateContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request);

        Task<Either<ActionResult, DataBlockViewModel>> UpdateDataBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, DataBlockUpdateRequest request);

        Task<Either<ActionResult, List<DataBlock>>> GetUnattachedDataBlocks(Guid releaseId);

        Task<Either<ActionResult, IContentBlockViewModel>> AttachDataBlock(
            Guid contentSectionId, DataBlockAttachRequest request);
    }
}
