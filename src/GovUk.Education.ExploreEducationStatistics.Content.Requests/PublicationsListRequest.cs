#nullable enable
<<<<<<<< HEAD:src/GovUk.Education.ExploreEducationStatistics.Content.Requests/PublicationsListGetRequest.cs
using System;
========
>>>>>>>> EES-4705-removing-content-api-and-content-services-dependencies:src/GovUk.Education.ExploreEducationStatistics.Content.Requests/PublicationsListRequest.cs
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record PublicationsListGetRequest(
    ReleaseType? ReleaseType,
    Guid? ThemeId,
    [MinLength(3)] string? Search,
    IPublicationService.PublicationsSortBy? Sort,
    SortOrder? Order,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, int.MaxValue)] int PageSize = 10);
