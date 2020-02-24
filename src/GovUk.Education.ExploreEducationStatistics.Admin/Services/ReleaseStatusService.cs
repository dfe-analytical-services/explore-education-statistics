using System;
using System.Linq;
using System.Threading.Tasks;
using WindowsAzure.Table.Extensions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseStatusService : IReleaseStatusService
    {
        private const string TableName = "ReleaseStatus";

        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;
        private readonly IUserService _userService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public ReleaseStatusService(IMapper mapper, IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper, ITableStorageService tableStorageService)
        {
            _mapper = mapper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _tableStorageService = tableStorageService;
        }

        public async Task<Either<ActionResult, ReleaseStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var table = await GetTableAsync();
                    var query = table.CreateQuery<ReleaseStatus>()
                        .Where(releaseStatus => releaseStatus.PartitionKey.Equals(releaseId.ToString()));
                    var result = await query.FirstOrDefaultAsync();
                    return _mapper.Map<ReleaseStatusViewModel>(result);
                });
        }

        private async Task<CloudTable> GetTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}