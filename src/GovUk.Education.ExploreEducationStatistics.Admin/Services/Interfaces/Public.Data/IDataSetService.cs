#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetService
{
    Task<Either<ActionResult, PaginatedListViewModel<DataSetViewModel>>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId,
        CancellationToken cancellationToken = default);
}
