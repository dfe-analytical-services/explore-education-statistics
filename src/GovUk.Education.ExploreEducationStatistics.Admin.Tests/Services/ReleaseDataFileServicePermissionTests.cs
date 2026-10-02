#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseDataFileServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new DataFixture()
        .DefaultReleaseVersion()
        .WithRelease(new DataFixture().DefaultRelease());

    [Fact]
    public async Task Delete()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.Delete(releaseVersionId: _releaseVersion.Id, fileId: Guid.NewGuid());
            });
    }

    [Fact]
    public async Task Delete_MultipleFiles()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.Delete(releaseVersionId: _releaseVersion.Id, fileIds: new List<Guid> { Guid.NewGuid() });
            });
    }

    [Fact]
    public async Task DeleteAll()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = _releaseVersion,
            File = new File { Filename = "ancillary.pdf", Type = Ancillary },
        };

        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseFiles.Add(releaseFile);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.DeleteAll(_releaseVersion.Id);
            });
    }

    [Fact]
    public async Task GetInfo()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = _releaseVersion,
            File = new File { Id = Guid.NewGuid(), Type = FileType.Data },
        };

        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseFiles.Add(releaseFile);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(releaseFile.ReleaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.GetInfo(releaseVersionId: releaseFile.ReleaseVersion.Id, fileId: releaseFile.File.Id);
            });
    }

    [Fact]
    public async Task GetAccoutrementsSummary()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = _releaseVersion,
            File = new File { Id = Guid.NewGuid(), Type = FileType.Data },
        };

        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseFiles.Add(releaseFile);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.GetAccoutrementsSummary(
                    releaseVersionId: releaseFile.ReleaseVersionId,
                    fileId: releaseFile.FileId
                );
            });
    }

    [Fact]
    public async Task ListAll()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.ListAll(_releaseVersion.Id);
            });
    }

    [Fact]
    public async Task ListDataSetUploads()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.ListDataSetUploads(_releaseVersion.Id, cancellationToken: CancellationToken.None);
            });
    }

    [Fact]
    public async Task DeleteDataSetUpload()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.DeleteDataSetUpload(
                    releaseVersionId: _releaseVersion.Id,
                    dataSetUploadId: Guid.NewGuid(),
                    cancellationToken: CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task ReorderDataFiles()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.ReorderDataFiles(_releaseVersion.Id, new List<Guid>());
            });
    }

    [Fact]
    public async Task Upload()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.Upload(
                    releaseVersionId: _releaseVersion.Id,
                    dataFile: new Mock<IManagedStreamFile>().Object,
                    metaFile: new Mock<IManagedStreamFile>().Object,
                    dataSetTitle: "",
                    cancellationToken: CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task UploadAsZip()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.UploadFromZip(
                    releaseVersionId: _releaseVersion.Id,
                    zipFile: new Mock<IManagedStreamZipFile>().Object,
                    dataSetTitle: "",
                    cancellationToken: CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task ValidateAndUploadBulkZip()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.UploadFromBulkZip(
                    releaseVersionId: _releaseVersion.Id,
                    zipFile: new Mock<IManagedStreamZipFile>().Object,
                    cancellationToken: CancellationToken.None
                );
            });
    }

    [Fact]
    public async Task SaveDataSetsFromTemporaryBlobStorage()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = SetupReleaseDataFileService(
                    contentDbContext: contentDbContext,
                    userService: userService.Object
                );
                return service.SaveDataSetsFromTemporaryBlobStorage(
                    releaseVersionId: _releaseVersion.Id,
                    dataSetUploadIds: [],
                    cancellationToken: CancellationToken.None
                );
            });
    }

    private ReleaseDataFileService SetupReleaseDataFileService(
        ContentDbContext? contentDbContext = null,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IDataSetValidator? dataSetValidator = null,
        IFileRepository? fileRepository = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IReleaseFileService? releaseFileService = null,
        IDataImportService? dataImportService = null,
        IUserService? userService = null,
        IDataSetFileStorage? dataSetFileStorage = null,
        IDataSetUploadRepository? dataSetUploadRepository = null,
        IDataBlockService? dataBlockService = null,
        IFootnoteRepository? footnoteRepository = null,
        IDataSetScreenerService? dataSetScreenerService = null,
        IReplacementPlanService? replacementPlanService = null,
        IMapper? mapper = null
    )
    {
        contentDbContext ??= Mock.Of<ContentDbContext>();

        return new ReleaseDataFileService(
            contentDbContext,
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(MockBehavior.Strict),
            dataSetValidator ?? Mock.Of<IDataSetValidator>(MockBehavior.Strict),
            fileRepository ?? new FileRepository(contentDbContext),
            releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
            releaseFileService ?? Mock.Of<IReleaseFileService>(MockBehavior.Strict),
            dataImportService ?? Mock.Of<IDataImportService>(MockBehavior.Strict),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
            dataSetFileStorage ?? Mock.Of<IDataSetFileStorage>(MockBehavior.Strict),
            dataSetUploadRepository ?? Mock.Of<IDataSetUploadRepository>(MockBehavior.Strict),
            dataBlockService ?? Mock.Of<IDataBlockService>(MockBehavior.Strict),
            footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
            dataSetScreenerService ?? Mock.Of<IDataSetScreenerService>(MockBehavior.Strict),
            replacementPlanService ?? Mock.Of<IReplacementPlanService>(MockBehavior.Strict),
            mapper ?? Mock.Of<IMapper>(MockBehavior.Strict)
        );
    }
}
