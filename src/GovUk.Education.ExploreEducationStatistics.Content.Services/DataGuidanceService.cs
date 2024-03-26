#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class DataGuidanceService : IDataGuidanceService
{
    private readonly IDataGuidanceDataSetService _dataGuidanceDataSetService;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly IReleaseCacheService _releaseCacheService;

    public DataGuidanceService(IDataGuidanceDataSetService dataGuidanceDataSetService,
        IPublicationCacheService publicationCacheService,
        IReleaseCacheService releaseCacheService)
    {
        _dataGuidanceDataSetService = dataGuidanceDataSetService;
        _publicationCacheService = publicationCacheService;
        _releaseCacheService = releaseCacheService;
    }

    public async Task<Either<ActionResult, DataGuidanceViewModel>> GetDataGuidance(string publicationSlug,
        string? releaseSlug = null)
    {
        return await _publicationCacheService.GetPublication(publicationSlug)
            .OnSuccessCombineWith(_ => _releaseCacheService.GetRelease(publicationSlug, releaseSlug))
            .OnSuccess(publicationAndRelease =>
            {
                var (publication, release) = publicationAndRelease;
                return _dataGuidanceDataSetService.ListDataSets(release.Id)
                    .OnSuccess(dataSets => new DataGuidanceViewModel(
                        release,
                        publication,
                        dataSets
                    ));
            });
    }
}
