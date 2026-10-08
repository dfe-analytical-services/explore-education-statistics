#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetFileStoragePermissionTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetTemporaryFileDownloadToken()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(async userService =>
            {
                await using var contentDbContext = InMemoryApplicationDbContext();
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();

                var service = BuildService(contentDbContext: contentDbContext, userService: userService.Object);

                return await service.GetTemporaryFileDownloadToken(
                    releaseVersionId: releaseVersion.Id,
                    dataSetUploadId: Guid.NewGuid(),
                    fileType: FileType.Data,
                    cancellationToken: default
                );
            });
    }

    private static DataSetFileStorage BuildService(ContentDbContext contentDbContext, IUserService userService)
    {
        return new DataSetFileStorage(
            contentDbContext,
            Mock.Of<IPrivateBlobStorageService>(Strict),
            Mock.Of<IReleaseVersionRepository>(Strict),
            Mock.Of<IReleaseDataFileRepository>(Strict),
            Mock.Of<IDataSetUploadRepository>(Strict),
            Mock.Of<IDataImportService>(Strict),
            userService,
            Mock.Of<IDataSetVersionService>(Strict),
            Mock.Of<IDataSetService>(Strict),
            TimeProvider.System,
            Mock.Of<ILogger<DataSetFileStorage>>(Strict)
        );
    }
}
