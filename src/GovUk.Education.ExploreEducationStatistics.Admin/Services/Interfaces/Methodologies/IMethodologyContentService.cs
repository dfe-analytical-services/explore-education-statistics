﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyContentService
    {
        Task<Either<ActionResult, ManageMethodologyContentViewModel>> GetContentAsync(Guid methodologyId);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid releaseId,
            MethodologyContentService.ContentListType contentType);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder);

        Task<Either<ActionResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, AddContentSectionRequest request,
            MethodologyContentService.ContentListType contentType);

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
    }
}