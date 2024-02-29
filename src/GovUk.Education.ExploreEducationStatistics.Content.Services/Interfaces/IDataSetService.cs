#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IDataSetService
{
    Task<Either<ActionResult, PaginatedListViewModel<DataSetListViewModel>>> ListDataSets(
        Guid? themeId,
        Guid? publicationId,
        Guid? releaseId,
        bool? latestOnly,
        string? searchTerm,
        DataSetsListRequestOrderBy? orderBy,
        SortOrder? sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, DataSetDetailsViewModel>> GetDataSet(
        Guid releaseId,
        Guid fileId);
}
