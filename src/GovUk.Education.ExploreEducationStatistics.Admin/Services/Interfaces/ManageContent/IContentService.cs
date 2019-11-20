using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IContentService
    {
        Task<Either<ValidationResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(Guid releaseId);

        Task<Either<ValidationResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(Guid releaseId, Dictionary<Guid, int> newSectionOrder);
        
        Task<Either<ValidationResult, ContentSectionViewModel>> AddContentSectionAsync(Guid releaseId);

        Task<Either<ValidationResult, ContentSectionViewModel>> UpdateContentSectionHeadingAsync(
            Guid releaseId, Guid contentSectionId, string newHeading);

        Task<Either<ValidationResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(Guid releaseId,
            Guid contentSectionId);
    }
}