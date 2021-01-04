using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ImportServiceTests
    {
        [Fact]
        public async void CancelFileImport()
        {
            var userService = new Mock<IUserService>(MockBehavior.Strict);
            var queueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            var cancelRequest = new ReleaseFileImportInfo
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file.csv"
            };

            userService
                .Setup(s => s.MatchesPolicy(cancelRequest, SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(true);

            queueService
                .Setup(s => s.AddMessageAsync("imports-cancelling",
                    It.Is<CancelImportMessage>(m => m.ReleaseId == cancelRequest.ReleaseId
                                                    && m.DataFileName == cancelRequest.DataFileName))).
                Returns(Task.CompletedTask);

            var service = BuildImportService(queueService: queueService.Object, userService: userService.Object);

            var result = await service.CancelImport(cancelRequest);
            Assert.True(result.IsRight);

            MockUtils.VerifyAllMocks(userService, queueService);
        }

        [Fact]
        public async void CancelFileImportButNotAllowed()
        {
            var userService = new Mock<IUserService>();
            var queueService = new Mock<IStorageQueueService>(MockBehavior.Strict);

            var cancelRequest = new ReleaseFileImportInfo
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file.csv"
            };

            userService
                .Setup(s => s.MatchesPolicy(cancelRequest, SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(false);

            var service = BuildImportService(queueService: queueService.Object, userService: userService.Object);

            var result = await service.CancelImport(cancelRequest);
            Assert.True(result.IsLeft);
            Assert.IsType<ForbidResult>(result.Left);

            MockUtils.VerifyAllMocks(userService, queueService);
        }

        private static ImportService BuildImportService(
            ContentDbContext context = null,
            IMapper mapper = null,
            ILogger<ImportService> logger = null,
            ITableStorageService tableStorageService = null,
            IGuidGenerator guidGenerator = null,
            IStorageQueueService queueService = null,
            IUserService userService = null)
        {
            return new ImportService(
                context ?? DbUtils.InMemoryApplicationDbContext(),
                mapper ?? new Mock<IMapper>().Object,
                logger ?? new Mock<ILogger<ImportService>>().Object,
                queueService ?? new Mock<IStorageQueueService>().Object,
                tableStorageService ?? new Mock<ITableStorageService>().Object,
                guidGenerator ?? new Mock<IGuidGenerator>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object);
        }
    }
}