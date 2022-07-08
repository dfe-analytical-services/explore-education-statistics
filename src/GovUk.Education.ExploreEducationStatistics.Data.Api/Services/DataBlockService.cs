#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
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

    public async Task<Either<ActionResult, TableBuilderResultViewModel>> GetDataBlockTableResult(Guid releaseId,
        Guid dataBlockId)
    {
        return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanViewRelease)
            .OnSuccess(() => CheckDataBlockExists(releaseId, dataBlockId))
            .OnSuccess(dataBlock => _tableBuilderService.Query(releaseId, dataBlock.Query));
    }

    private async Task<Either<ActionResult, DataBlock>> CheckDataBlockExists(Guid releaseId, Guid dataBlockId)
    {
        var dataBlock = await _contentDbContext.ReleaseContentBlocks
            .Include(block => block.ContentBlock)
            .Where(block => block.ReleaseId == releaseId && block.ContentBlockId == dataBlockId)
            .Select(block => block.ContentBlock)
            .OfType<DataBlock>()
            .SingleOrDefaultAsync();

        return dataBlock ?? new Either<ActionResult, DataBlock>(new NotFoundResult());
    }
}
