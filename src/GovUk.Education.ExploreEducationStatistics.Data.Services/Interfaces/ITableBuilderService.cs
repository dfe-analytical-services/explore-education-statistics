#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ITableBuilderService
    {
        Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            FullTableQuery query,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            Guid releaseVersionId,
            FullTableQuery query,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, Unit>> QueryToCsvStream(
            FullTableQuery query,
            Stream stream,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, Unit>> QueryToCsvStream(
            Guid releaseVersionId,
            FullTableQuery query,
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}
