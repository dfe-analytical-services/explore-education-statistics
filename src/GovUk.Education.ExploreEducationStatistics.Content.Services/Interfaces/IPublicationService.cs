#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IPublicationService
{
    Task<Either<ActionResult, PublicationCacheViewModel>> GetById(Guid publicationId);

    Task<Either<ActionResult, PublicationCacheViewModel>> GetBySlug(string publicationSlug);

    Task<IList<PublicationTreeThemeViewModel>> GetPublicationTree();

    Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        ReleaseType? releaseType = null,
        Guid? themeId = null,
        string? search = null,
        PublicationsSortBy? sort = null,
        SortOrder? order = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Guid>? publicationIds = null);
}
