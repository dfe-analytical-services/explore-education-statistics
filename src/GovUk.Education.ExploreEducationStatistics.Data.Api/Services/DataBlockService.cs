#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class DataBlockService : IDataBlockService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly ITableBuilderService _tableBuilderService;
    private readonly IUserService _userService;

    public DataBlockService(ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        ITableBuilderService tableBuilderService,
        IUserService userService)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _tableBuilderService = tableBuilderService;
        _userService = userService;
    }

    public async Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(
        Guid releaseVersionId,
        Guid dataBlockVersionId)
    {
        return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(() => CheckDataBlockVersionExists(releaseVersionId: releaseVersionId,
                dataBlockVersionId: dataBlockVersionId))
            .OnSuccess(dataBlock => _tableBuilderService.Query(releaseVersionId, dataBlock.Query));
    }

    public async Task<Either<ActionResult, Dictionary<string, List<LocationAttributeViewModel>>>> GetLocationsForDataBlock(
        Guid releaseVersionId,
        Guid dataBlockVersionId,
        long boundaryLevelId)
    {
        return await _persistenceHelper.CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(() => CheckDataBlockVersionExists(releaseVersionId: releaseVersionId,
                dataBlockVersionId: dataBlockVersionId))
            .OnSuccess(dataBlock => _tableBuilderService.QueryForBoundaryLevel(releaseVersionId, dataBlock.Query, boundaryLevelId));
    }

    private async Task<Either<ActionResult, DataBlockVersion>> CheckDataBlockVersionExists(
        Guid releaseVersionId,
        Guid dataBlockVersionId)
    {
        return await _contentDbContext
            .DataBlockVersions
            .Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId
                                       && dataBlockVersion.Id == dataBlockVersionId)
            .SingleOrDefaultAsync()
            .OrNotFound();
    }
}
