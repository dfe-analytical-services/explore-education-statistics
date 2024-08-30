#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleasePublishingStatusService : IReleasePublishingStatusService
    {
        private readonly IMapper _mapper;
        private readonly IPublisherTableStorageServiceOld _publisherTableStorageServiceOld;
        private readonly IUserService _userService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;

        public ReleasePublishingStatusService(
            IMapper mapper,
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IPublisherTableStorageServiceOld publisherTableStorageServiceOld)
        {
            _mapper = mapper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _publisherTableStorageServiceOld = publisherTableStorageServiceOld;
        }

        public async Task<Either<ActionResult, ReleasePublishingStatusViewModel>> GetReleaseStatusAsync(
            Guid releaseVersionId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(async _ =>
                {
                    var query = new TableQuery<ReleasePublishingStatusOld>()
                        .Where(TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatusOld.PartitionKey),
                            QueryComparisons.Equal, releaseVersionId.ToString()));

                    var result = await _publisherTableStorageServiceOld.ExecuteQuery(PublisherReleaseStatusTableName, query);
                    var first = result.OrderByDescending(status => status.Created).FirstOrDefault();
                    return _mapper.Map<ReleasePublishingStatusViewModel>(first);
                });
        }
    }
}
