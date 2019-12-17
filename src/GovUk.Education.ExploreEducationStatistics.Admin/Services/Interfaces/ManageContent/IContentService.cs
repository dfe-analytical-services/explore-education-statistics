using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IContentService
    {
        Task<Either<ValidationResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid releaseId);

        Task<Either<ValidationResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid releaseId, Dictionary<Guid, int> newSectionOrder);
        
        Task<Either<ValidationResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId, AddContentSectionRequest? request);

        Task<Either<ValidationResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
            Guid releaseId, Guid contentSectionId, string newHeading);

        Task<Either<ValidationResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(
            Guid releaseId, Guid contentSectionId);

        Task<Either<ValidationResult, ContentSectionViewModel>> GetContentSectionAsync(
            Guid releaseId, Guid contentSectionId);
        
        Task<Either<ValidationResult, List<IContentBlock>>> ReorderContentBlocksAsync(
            Guid releaseId, Guid contentSectionId, Dictionary<Guid,int> newBlocksOrder);
        
        Task<Either<ValidationResult, IContentBlock>> AddContentBlockAsync(
            Guid releaseId, Guid contentSectionId,
            AddContentBlockRequest request);
        
        Task<Either<ValidationResult, List<IContentBlock>>> RemoveContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);
        
        Task<Either<ValidationResult, IContentBlock>> UpdateTextBasedContentBlockAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, UpdateTextBasedContentBlockRequest request);
        
        Task<Either<ValidationResult, List<T>>> GetUnattachedContentBlocksAsync<T>(Guid releaseId)
            where T : IContentBlock;
        
        Task<Either<ValidationResult, IContentBlock>> AttachContentBlockAsync(
            Guid releaseId, Guid contentSectionId, AttachContentBlockRequest request);

        Task<Either<ValidationResult, List<CommentViewModel>>> GetCommentsAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);
        
        Task<Either<ValidationResult, CommentViewModel>> AddCommentAsync(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, AddCommentRequest comment);
    }
}