#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IPublicationService
{
    Task<Either<ActionResult, PublicationCacheViewModel>> Get(string publicationSlug);

    Task<IList<PublicationTreeThemeViewModel>> GetPublicationTree();

    Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        ReleaseType? releaseType = null,
        Guid? themeId = null,
        string? search = null,
        PublicationsSortBy? sort = null,
        SortOrder? order = null,
        int page = 1,
        int pageSize = 10);

    public enum PublicationsSortBy
    {
        /// <summary>
        /// The published date of the latest published release version by time series associated with the publication.
        /// </summary>
        Published,

        /// <summary>
        /// The relevance of the publication to the search term. This option is only applicable when a search term is provided.
        /// </summary>
        Relevance,

        /// <summary>
        /// The title of the publication.
        /// </summary>
        Title
    }
}
