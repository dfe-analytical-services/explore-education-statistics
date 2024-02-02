#nullable enable
using System;
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
        Guid? themeId = null,
        Guid? publicationId = null,
        Guid? releaseId = null,
        bool? latest = true,
        string? searchTerm = null,
        DataSetsListRequestOrderBy? orderBy = null,
        SortOrder? sort = null,
        int page = 1,
        int pageSize = 10);
}
