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
        public Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid releaseId) where T : ContentBlock;

        Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder);

        Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, ContentSectionAddRequest? request);

        Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid releaseId, Guid contentSectionId, string newHeading);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
            Guid releaseId, Guid contentSectionId);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder);

        Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(
            Guid releaseId, Guid contentSectionId,
            ContentBlockAddRequest request);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);

        Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request);

        Task<Either<ActionResult, IContentBlockViewModel>> AttachDataBlock(
            Guid releaseId, Guid contentSectionId, ContentBlockAttachRequest request);
    }
}
