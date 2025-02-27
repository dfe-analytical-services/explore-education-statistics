using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Migrations;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Migrations;

// TODO EES-5660 - remove this migration after it has been run against each Public API-enabled environment.
public class EES5660_MigrateDraftDataSetVersionFolderNamesTests(TestApplicationFactory testApp) 
    : IntegrationTestFixture(testApp)
{
    public static readonly TheoryData<(DataSetVersionStatus, DataSetVersionType)>
        PrivateDataSetVersionStatusAndTypes = new(DataSetVersionAuthExtensions
            .PrivateStatuses
            .Cartesian(EnumUtil.GetEnums<DataSetVersionType>()));
    
    public static readonly TheoryData<(DataSetVersionStatus, DataSetVersionType)>
        PublicDataSetVersionStatusAndTypes = new(DataSetVersionAuthExtensions
            .PublicStatuses
            .Cartesian(EnumUtil.GetEnums<DataSetVersionType>()));
    
    [Theory]
    [MemberData(nameof(PrivateDataSetVersionStatusAndTypes))]
    public async Task Success_NonMigratedDraft((DataSetVersionStatus, DataSetVersionType) statusAndType)
    {
        var (status, type) = statusAndType;

        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithId(Guid.NewGuid())
            .WithDataSet(DataFixture
                .DefaultDataSet()
                .WithId(Guid.NewGuid()))
            .WithStatus(status)
            .WithVersionNumber(
                major: type == DataSetVersionType.Major ? 2 : 1,
                minor: type == DataSetVersionType.Major ? 0 : 1);
        
        await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

        var pathResolver = TestApp.Services.GetRequiredService<IDataSetVersionPathResolver>();

        var versionedFolder = pathResolver.DirectoryPath(dataSetVersion, dataSetVersion.SemVersion());
        Directory.CreateDirectory(versionedFolder);
        File.Create(Path.Combine(versionedFolder, "test.txt")).Dispose();
        
        GetMigration().Apply();

        // Assert that the original folder that used the draft's version in its name no longer exists.
        Assert.False(Directory.Exists(versionedFolder));

        // Assert that the original folder has moved to use the new special "draft" folder.
        var expectedDraftFolder = pathResolver.DirectoryPath(dataSetVersion);
        Assert.True(Directory.Exists(expectedDraftFolder));
        Assert.True(File.Exists(Path.Combine(expectedDraftFolder, "test.txt")));
    }

    [Theory]
    [MemberData(nameof(PrivateDataSetVersionStatusAndTypes))]
    public async Task Success_AlreadyMigratedDraft((DataSetVersionStatus, DataSetVersionType) statusAndType)
    {
        var (status, type) = statusAndType;
        
        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithId(Guid.NewGuid())
            .WithDataSet(DataFixture
                .DefaultDataSet()
                .WithId(Guid.NewGuid()))
            .WithStatus(status)
            .WithVersionNumber(
                major: type == DataSetVersionType.Major ? 2 : 1,
                minor: type == DataSetVersionType.Major ? 0 : 1);
        
        await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

        var pathResolver = TestApp.Services.GetRequiredService<IDataSetVersionPathResolver>();

        var draftFolder = pathResolver.DirectoryPath(dataSetVersion);
        Directory.CreateDirectory(draftFolder);
        File.Create(Path.Combine(draftFolder, "test.txt"));
        
        GetMigration().Apply();

        // Assert that the existing draft folder is left untouched.
        Assert.True(Directory.Exists(draftFolder));
        Assert.True(File.Exists(Path.Combine(draftFolder, "test.txt")));
    }
    
    [Fact]
    public async Task Failure_DraftFolderAndVersionedFolderExist()
    {
        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(DataFixture.DefaultDataSet())
            .WithStatus(DataSetVersionStatus.Draft)
            .WithVersionNumber(major: 2, minor: 0);
     
        await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

        var pathResolver = TestApp.Services.GetRequiredService<IDataSetVersionPathResolver>();

        var versionedFolder = pathResolver.DirectoryPath(dataSetVersion, dataSetVersion.SemVersion());
        Directory.CreateDirectory(versionedFolder);
        File.Create(Path.Combine(versionedFolder, "versioned.txt"));
        
        var draftFolder = pathResolver.DirectoryPath(dataSetVersion);
        Directory.CreateDirectory(draftFolder);
        File.Create(Path.Combine(draftFolder, "draft.txt"));

        var exception = Assert.Throws<Exception>(GetMigration().Apply);

        Assert.Equal("The following DataSetVersions have both a versioned " +
                     "and a draft folder: " + dataSetVersion.Id, exception.Message);

        // Assert that the versioned folder still exists.
        Assert.True(Directory.Exists(versionedFolder));
        Assert.True(File.Exists(Path.Combine(versionedFolder, "versioned.txt")));

        // Assert that the draft folder still exists.
        Assert.True(Directory.Exists(draftFolder));
        Assert.True(File.Exists(Path.Combine(draftFolder, "draft.txt")));
    }
        
    [Theory]
    [MemberData(nameof(PublicDataSetVersionStatusAndTypes))]
    public async Task Success_PublicVersionsNotMigrated((DataSetVersionStatus, DataSetVersionType) statusAndType)
    {
        var (status, type) = statusAndType;
        
        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithId(Guid.NewGuid())
            .WithDataSet(DataFixture
                .DefaultDataSet()
                .WithId(Guid.NewGuid()))
            .WithStatus(status)
            .WithVersionNumber(
                major: type == DataSetVersionType.Major ? 2 : 1,
                minor: type == DataSetVersionType.Major ? 0 : 1);
        
        await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

        var pathResolver = TestApp.Services.GetRequiredService<IDataSetVersionPathResolver>();

        var versionedFolder = pathResolver.DirectoryPath(dataSetVersion, dataSetVersion.SemVersion());
        Directory.CreateDirectory(versionedFolder);
        File.Create(Path.Combine(versionedFolder, "test.txt"));
        
        GetMigration().Apply();

        // Assert that the original folder has been left unaffected.
        Assert.True(Directory.Exists(versionedFolder));
        Assert.True(File.Exists(Path.Combine(versionedFolder, "test.txt")));
        
        // Assert that no draft folder has been created.
        dataSetVersion.Status = DataSetVersionStatus.Draft;
        var draftFolder = pathResolver.DirectoryPath(dataSetVersion);
        Assert.False(Directory.Exists(draftFolder));
    }

    private EES5660_MigrateDraftDataSetVersionFolderNames GetMigration()
    {
        return TestApp
            .Services
            .GetServices<ICustomMigration>()
            .Where(migration => migration.GetType() == typeof(EES5660_MigrateDraftDataSetVersionFolderNames))
            .Cast<EES5660_MigrateDraftDataSetVersionFolderNames>()
            .Single();
    }
}
