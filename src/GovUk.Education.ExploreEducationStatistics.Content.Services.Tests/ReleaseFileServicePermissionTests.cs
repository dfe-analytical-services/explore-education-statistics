#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ReleaseFileServicePermissionTests
    {
        private static readonly Release Release = new()
        {
            Id = Guid.NewGuid()
        };

        private static readonly ReleaseFile ReleaseFile = new()
        {
            Release = Release,
            File = new File(),
        };

        [Fact]
        public async Task StreamFile()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(ReleaseFile.Release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var persistenceHelper =
                            MockPersistenceHelper<ContentDbContext, ReleaseFile>(ReleaseFile);

                        var service = BuildReleaseFileService(
                            userService: userService.Object,
                            persistenceHelper: persistenceHelper.Object
                        );
                        return service.StreamFile(ReleaseFile.ReleaseId, ReleaseFile.FileId);
                    }
                );
        }

        [Fact]
        public async Task ZipFilesToStream()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(Release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseFileService(userService: userService.Object);
                        return service.ZipFilesToStream(
                            releaseId: Release.Id,
                            outputStream: Stream.Null,
                            fileIds: ListOf(Guid.NewGuid())
                        );
                    }
                );
        }

        private ReleaseFileService BuildReleaseFileService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? Mock.Of<IBlobStorageService>(),
                dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(),
                userService ?? Mock.Of<IUserService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockPersistenceHelper<ContentDbContext, Release>(Release.Id, Release);
        }
    }
}