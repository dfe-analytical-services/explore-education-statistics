#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class MethodologyCacheService : IMethodologyCacheService
{
    private readonly IMethodologyService _methodologyService;
    private readonly ILogger<MethodologyCacheService> _logger;

    public MethodologyCacheService(
        IMethodologyService methodologyService,
        ILogger<MethodologyCacheService> logger)
    {
        _methodologyService = methodologyService;
        _logger = logger;
    }

    [BlobCache(typeof(AllMethodologiesCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetSummariesTree()
    {
        return _methodologyService.GetSummariesTree();
    }

    [BlobCache(typeof(AllMethodologiesCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> UpdateSummariesTree()
    {
        _logger.LogInformation("Updating cached Methodology Tree");
        return _methodologyService.GetSummariesTree();
    }

    public Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetSummariesByPublication(
        Guid publicationId)
    {
        return GetSummariesTree()
            .OnSuccess(methodologiesByTheme =>
            {
                var matchingPublication = methodologiesByTheme
                    .SelectMany(theme => theme.Topics)
                    .SelectMany(topic => topic.Publications)
                    .SingleOrDefault(publication => publication.Id == publicationId);
                return matchingPublication?.Methodologies ?? new List<MethodologyVersionSummaryViewModel>();
            });
    }
}
