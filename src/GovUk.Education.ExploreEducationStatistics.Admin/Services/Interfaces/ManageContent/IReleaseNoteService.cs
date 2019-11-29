using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IReleaseNoteService
    {
        Task<Either<ValidationResult, List<Update>>> AddReleaseNoteAsync(Guid releaseId,
            CreateReleaseNoteRequest request);
    }
}