#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyContentService
    {
        Task<Either<ActionResult, ManageMethodologyContentViewModel>> GetContent(Guid methodologyVersionId);

        Task<Either<ActionResult, List<T>>> GetContentBlocks<T>(Guid methodologyVersionId) where T : ContentBlock;

        Task<Either<ActionResult, List<ContentSectionViewModel>>> ReorderContentSections(
            Guid methodologyVersionId, Dictionary<Guid, int> newSectionOrder);

        Task<Either<ActionResult, ContentSectionViewModel>> AddContentSection(
            Guid methodologyVersionId, ContentSectionAddRequest request,
            MethodologyContentService.ContentListType contentType);

        Task<Either<ActionResult, ContentSectionViewModel>> UpdateContentSectionHeading(
            Guid methodologyVersionId, Guid contentSectionId, string newHeading);

        Task<Either<ActionResult, List<ContentSectionViewModel>>> RemoveContentSection(
            Guid methodologyVersionId, Guid contentSectionId);

        Task<Either<ActionResult, ContentSectionViewModel>> GetContentSection(
            Guid methodologyVersionId, Guid contentSectionId);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> ReorderContentBlocks(
            Guid methodologyVersionId, Guid contentSectionId, Dictionary<Guid, int> newBlocksOrder);

        Task<Either<ActionResult, IContentBlockViewModel>> AddContentBlock(
            Guid methodologyVersionId, Guid contentSectionId,
            ContentBlockAddRequest request);

        Task<Either<ActionResult, List<IContentBlockViewModel>>> RemoveContentBlock(
            Guid methodologyVersionId, Guid contentSectionId, Guid contentBlockId);

        Task<Either<ActionResult, IContentBlockViewModel>> UpdateTextBasedContentBlock(
            Guid methodologyVersionId, Guid contentSectionId, Guid contentBlockId, ContentBlockUpdateRequest request);
    }
}
