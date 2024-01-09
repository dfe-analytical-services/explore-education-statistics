using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record PublicationsListGetRequest(
    ReleaseType? ReleaseType,
    Guid? ThemeId,
    [MinLength(3)] string? Search,
    PublicationsSortBy? Sort,
    SortOrder? Order,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, int.MaxValue)] int PageSize = 10);
