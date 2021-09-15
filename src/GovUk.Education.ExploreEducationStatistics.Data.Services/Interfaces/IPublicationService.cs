#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IPublicationService
    {
        Task<Either<ActionResult, List<SubjectViewModel>>> ListLatestReleaseSubjects(Guid publicationId);

        Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListLatestReleaseFeaturedTables(Guid publicationId);
    }
}