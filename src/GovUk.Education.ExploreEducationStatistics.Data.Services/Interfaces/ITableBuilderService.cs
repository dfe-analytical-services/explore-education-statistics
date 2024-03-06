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
            ObservationQueryContext queryContext,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            Guid releaseVersionId,
            ObservationQueryContext queryContext,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, Unit>> QueryToCsvStream(
            ObservationQueryContext queryContext,
            Stream stream,
            CancellationToken cancellationToken = default);

        Task<Either<ActionResult, Unit>> QueryToCsvStream(
            Guid releaseVersionId,
            ObservationQueryContext queryContext,
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}
