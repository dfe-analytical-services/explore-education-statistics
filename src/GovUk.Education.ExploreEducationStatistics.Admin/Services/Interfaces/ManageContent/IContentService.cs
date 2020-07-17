﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IContentService
    {
        Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid releaseId);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder);

        Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, AddContentSectionRequest? request);

        Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
            Guid releaseId, Guid contentSectionId, string newHeading);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(
            Guid releaseId, Guid contentSectionId);

        Task<Either<ActionResult, ContentSectionViewModel>> GetContentSectionAsync(
            Guid releaseId, Guid contentSectionId);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocksAsync(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder);

        Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlockAsync(
            Guid releaseId, Guid contentSectionId,
            AddContentBlockRequest request);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);

        Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request);

        Task<Either<ActionResult, DataBlockViewModel>> UpdateDataBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateDataBlockRequest request);

        Task<Either<ActionResult, List<T>>> GetUnattachedContentBlocksAsync<T>(Guid releaseId)
            where T : ContentBlock;

        Task<Either<ActionResult, IContentBlockViewModel>> AttachDataBlock(
            Guid releaseId, Guid contentSectionId, AttachContentBlockRequest request);

        Task<Either<ActionResult, List<CommentViewModel>>> GetCommentsAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);

        Task<Either<ActionResult, CommentViewModel>> AddCommentAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, AddOrUpdateCommentRequest request);

        Task<Either<ActionResult, CommentViewModel>> UpdateCommentAsync(Guid commentId, 
            AddOrUpdateCommentRequest request);

        Task<Either<ActionResult, bool>> DeleteCommentAsync(Guid commentId);
    }
}