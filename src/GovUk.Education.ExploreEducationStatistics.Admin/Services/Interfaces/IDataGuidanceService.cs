using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataGuidanceService
    {
        public Task<Either<ActionResult, DataGuidanceViewModel>> Get(Guid releaseId);

        public Task<Either<ActionResult, DataGuidanceViewModel>> Update(Guid releaseId,
            DataGuidanceUpdateViewModel request);

        public Task<Either<ActionResult, Unit>> Validate(Guid releaseId);
    }
}