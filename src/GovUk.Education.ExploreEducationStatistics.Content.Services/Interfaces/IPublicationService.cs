using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IPublicationService
{
    Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetSummary(Guid publicationId);

    Task<Either<ActionResult, PublicationCacheViewModel>> Get(string publicationSlug);

    Task<IList<PublicationTreeThemeViewModel>> GetPublicationTree();

    Task<IList<PublicationInfoViewModel>> ListPublicationInfos(
        Guid? themeId = null,
        CancellationToken cancellationToken = default
    );
}
