#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Requests;

public record DataSetListRequest(
    Guid? ThemeId,
    Guid? PublicationId,
    Guid? ReleaseId,
    [MinLength(3)] string? SearchTerm,
    IDataSetService.DataSetServiceOrderBy? OrderBy,
    SortOrder? SortOrder,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, int.MaxValue)] int PageSize = 10);
