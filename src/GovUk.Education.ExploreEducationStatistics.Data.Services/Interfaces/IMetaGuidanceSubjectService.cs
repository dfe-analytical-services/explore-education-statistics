#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IMetaGuidanceSubjectService
    {
        Task<Either<ActionResult, List<MetaGuidanceSubjectViewModel>>> GetSubjects(
            Guid releaseId,
            IEnumerable<Guid>? subjectIds = null);

        Task<TimePeriodLabels> GetTimePeriods(Guid subjectId);

        Task<List<string>> GetGeographicLevels(Guid subjectId);

        Task<Either<ActionResult, bool>> Validate(Guid releaseId);
    }
}