#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IDataSetService
{
    Task<Either<ActionResult, PaginatedListViewModel<DataSetSearchResultViewModel>>> ListDataSets(
        Guid? themeId = null,
        Guid? publicationId = null,
        Guid? releaseId = null,
        string? searchTerm = null,
        DataSetServiceOrderBy? orderBy = null,
        SortOrder? sortOrder = null,
        int page = 1,
        int pageSize = 10);

    public enum DataSetServiceOrderBy
    {
        /// <summary>
        /// The natural order of the data set, as defined by analysts. All results must be for the same release version
        /// to ensure a stable sort order. This option is only applicable in combination with the release version filter.
        /// </summary>
        Natural,

        /// <summary>
        /// The published date of the release version that the data set is associated with.
        /// </summary>
        Published,

        /// <summary>
        /// The relevance of the data set to the search term. This option is only applicable when a search term is provided.
        /// </summary>
        Relevance,

        /// <summary>
        /// The title of the data set.
        /// </summary>
        Title
    }
}
