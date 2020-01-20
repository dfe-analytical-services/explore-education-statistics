using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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

        public async Task<Either<ActionResult, IEnumerable<ReleaseStatusViewModel>>> GetReleaseStatusesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var query = new TableQuery<ReleaseStatus>().Where(
                        TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey), QueryComparisons.Equal,
                            releaseId.ToString()));

                    var results = new List<ReleaseStatus>();
                    var table = await GetTableAsync();
                    TableContinuationToken token = null;
                    do
                    {
                        var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                        results.AddRange(queryResult.Results);
                        token = queryResult.ContinuationToken;
                    } while (token != null);

                    return _mapper.Map<IEnumerable<ReleaseStatusViewModel>>(results);
                });
        }

        private async Task<CloudTable> GetTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}