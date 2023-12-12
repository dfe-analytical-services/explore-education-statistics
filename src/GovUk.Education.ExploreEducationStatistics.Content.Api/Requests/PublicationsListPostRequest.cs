#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;

public record PublicationsListPostRequest(
    ReleaseType? ReleaseType,
    Guid? ThemeId,
    [MinLength(3)] string? Search,
    IPublicationService.PublicationsSortBy? Sort,
    SortOrder? Order,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, int.MaxValue)] int PageSize = 10,
    IEnumerable<Guid>? PublicationIds = null);
