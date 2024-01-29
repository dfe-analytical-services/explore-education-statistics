using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record PublicationsListPostRequest(
    ReleaseType? ReleaseType = null,
    Guid? ThemeId = null,
    [MinLength(3)] string? Search = null,
    PublicationsSortBy? Sort = null,
    SortOrder? Order = null,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, int.MaxValue)] int PageSize = 10,
    IEnumerable<Guid>? PublicationIds = null);
