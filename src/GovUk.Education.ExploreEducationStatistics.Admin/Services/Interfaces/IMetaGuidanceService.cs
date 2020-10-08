using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IMetaGuidanceService
    {
        public Task<Either<ActionResult, MetaGuidanceViewModel>> Get(Guid releaseId);

        public Task<Either<ActionResult, MetaGuidanceViewModel>> Update(Guid releaseId,
            MetaGuidanceUpdateViewModel request);
    }
}