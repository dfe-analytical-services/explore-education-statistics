#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IDataBlockService
{
    Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(Guid releaseId, Guid dataBlockId);
}
