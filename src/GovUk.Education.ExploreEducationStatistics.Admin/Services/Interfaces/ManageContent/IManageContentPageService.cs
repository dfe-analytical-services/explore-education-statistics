using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IManageContentPageService
    {
        Task<Either<ValidationResult, ManageContentPageViewModel>> GetManageContentPageViewModelAsync(Guid releaseId);
    }
}