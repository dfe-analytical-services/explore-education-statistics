using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ReleaseFileServicePermissionTests
    {
        private readonly DataFixture _dataFixture = new();

        private static readonly ReleaseVersion ReleaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        private static readonly ReleaseFile ReleaseFile = new()
        {
            ReleaseVersion = ReleaseVersion,
            File = new File(),
        };

        [Fact]
        public async Task StreamFile()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(ReleaseFile.ReleaseVersion, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var persistenceHelper =
                            MockPersistenceHelper<ContentDbContext, ReleaseFile>(ReleaseFile);

                        var service = BuildService(
                            userService: userService.Object,
                            persistenceHelper: persistenceHelper.Object
                        );
                        return service.StreamFile(releaseVersionId: ReleaseFile.ReleaseVersionId,
                            fileId: ReleaseFile.FileId);
                    }
                );
        }

        [Fact]
        public async Task ZipFilesToStream()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id,
                    ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    async userService =>
                    {
                        await using var contextDbContext = InMemoryContentDbContext();
                        contextDbContext.ReleaseVersions.Add(releaseVersion);
                        await contextDbContext.SaveChangesAsync();

                        var service = BuildService(
                            contentDbContext: contextDbContext,
                            userService: userService.Object);

                        return await service.ZipFilesToStream(
                            releaseVersionId: releaseVersion.Id,
                            outputStream: Stream.Null,
                            fileIds: [Guid.NewGuid()]
                        );
                    }
                );
        }

        private ReleaseFileService BuildService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IPublicBlobStorageService? publicBlobStorageService = null,
            IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
            IUserService? userService = null,
            IAnalyticsManager? analyticsManager = null,
            ILogger<ReleaseFileService>? logger = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(),
                dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(),
                userService ?? Mock.Of<IUserService>(),
                analyticsManager ?? Mock.Of<IAnalyticsManager>(),
                logger ?? Mock.Of<ILogger<ReleaseFileService>>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockPersistenceHelper<ContentDbContext, ReleaseVersion>(ReleaseVersion.Id, ReleaseVersion);
        }
    }
}
