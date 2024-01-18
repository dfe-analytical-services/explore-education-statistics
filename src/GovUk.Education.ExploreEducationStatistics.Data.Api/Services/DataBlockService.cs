#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
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
        Guid releaseId,
        Guid dataBlockVersionId)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanViewRelease)
            .OnSuccess(() => CheckDataBlockVersionExists(releaseId, dataBlockVersionId))
            .OnSuccess(dataBlock => _tableBuilderService.Query(releaseId, dataBlock.Query));
    }

    private async Task<Either<ActionResult, DataBlockVersion>> CheckDataBlockVersionExists(
        Guid releaseId,
        Guid dataBlockVersionId)
    {
        return await _contentDbContext
            .DataBlockVersions
            .Where(dataBlockVersion => dataBlockVersion.ReleaseId == releaseId
                                       && dataBlockVersion.Id == dataBlockVersionId)
            .SingleOrDefaultAsync()
            .OrNotFound();
    }
}
