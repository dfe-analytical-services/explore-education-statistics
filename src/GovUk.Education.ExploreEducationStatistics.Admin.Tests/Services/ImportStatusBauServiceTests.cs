#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ImportStatusBauServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetIncompleteImports_NoResults()
    {
        await using (var contentDbContext = InMemoryApplicationDbContext())
        {
            var importStatusBauService = BuildImportStatusBauService(contentDbContext: contentDbContext);

            var result = await importStatusBauService.GetAllIncompleteImports();
            Assert.True(result.IsRight);
            Assert.Empty(result.Right);
        }
    }

    [Fact]
    public async Task GetIncompleteImports()
    {
        Publication publication = _dataFixture.DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        var (releaseFile1, releaseFile2, releaseFile3) = _dataFixture.DefaultReleaseFile()
            .WithFile(() => _dataFixture.DefaultFile(FileType.Data))
            .WithReleaseVersion(releaseVersion)
            .GenerateTuple3();

        var import1 = new DataImport
        {
            File = releaseFile1.File,
            TotalRows = 100,
            StagePercentageComplete = 99,
            Status = FAILED,
            SubjectId = Guid.NewGuid(),
            Created = DateTime.UtcNow.AddHours(-1)
        };

        var import2 = new DataImport
        {
            File = releaseFile2.File,
            TotalRows = 200,
            StagePercentageComplete = 54,
            Status = STAGE_1,
            SubjectId = Guid.NewGuid(),
            Created = DateTime.UtcNow
        };

        var import3 = new DataImport
        {
            File = releaseFile3.File,
            TotalRows = 300,
            StagePercentageComplete = 76,
            Status = STAGE_3,
            SubjectId = Guid.NewGuid(),
            Created = DateTime.UtcNow.AddDays(-1)
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            contentDbContext.ReleaseFiles.AddRange(releaseFile1, releaseFile2, releaseFile3);
            contentDbContext.DataImports.AddRange(import1, import2, import3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var importStatusBauService = BuildImportStatusBauService(contentDbContext: contentDbContext);

            var result = await importStatusBauService.GetAllIncompleteImports();

            Assert.True(result.IsRight);

            var imports = result.Right;
            Assert.Equal(3, imports.Count);

            // Assert they are in descending timestamp order - import2, import1, import3

            Assert.Equal(import2.File.Id, imports[0].FileId);
            Assert.Null(imports[0].SubjectTitle);
            Assert.Equal(import2.SubjectId, imports[0].SubjectId);
            Assert.Equal(releaseVersion.Release.Publication.Id, imports[0].PublicationId);
            Assert.Equal(releaseVersion.Release.Publication.Title, imports[0].PublicationTitle);
            Assert.Equal(releaseVersion.Id, imports[0].ReleaseId);
            Assert.Equal(releaseVersion.Release.Title, imports[0].ReleaseTitle);
            Assert.Equal(import2.File.Filename, imports[0].DataFileName);
            Assert.Equal(import2.TotalRows, imports[0].TotalRows);
            Assert.Equal(import2.Status, imports[0].Status);
            Assert.Equal(import2.StagePercentageComplete, imports[0].StagePercentageComplete);
            Assert.Equal(import2.PercentageComplete(), imports[0].PercentageComplete);

            Assert.Equal(import1.File.Id, imports[1].FileId);
            Assert.Null(imports[1].SubjectTitle);
            Assert.Equal(import1.SubjectId, imports[1].SubjectId);
            Assert.Equal(releaseVersion.Release.Publication.Id, imports[1].PublicationId);
            Assert.Equal(releaseVersion.Release.Publication.Title, imports[1].PublicationTitle);
            Assert.Equal(releaseVersion.Id, imports[1].ReleaseId);
            Assert.Equal(releaseVersion.Release.Title, imports[1].ReleaseTitle);
            Assert.Equal(import1.File.Filename, imports[1].DataFileName);
            Assert.Equal(import1.TotalRows, imports[1].TotalRows);
            Assert.Equal(import1.Status, imports[1].Status);
            Assert.Equal(import1.StagePercentageComplete, imports[1].StagePercentageComplete);
            Assert.Equal(import1.PercentageComplete(), imports[1].PercentageComplete);

            Assert.Equal(import3.File.Id, imports[2].FileId);
            Assert.Null(imports[2].SubjectTitle);
            Assert.Equal(import3.SubjectId, imports[2].SubjectId);
            Assert.Equal(releaseVersion.Release.Publication.Id, imports[2].PublicationId);
            Assert.Equal(releaseVersion.Release.Publication.Title, imports[2].PublicationTitle);
            Assert.Equal(releaseVersion.Id, imports[2].ReleaseId);
            Assert.Equal(releaseVersion.Release.Title, imports[2].ReleaseTitle);
            Assert.Equal(import3.File.Filename, imports[2].DataFileName);
            Assert.Equal(import3.TotalRows, imports[2].TotalRows);
            Assert.Equal(import3.StagePercentageComplete, imports[2].StagePercentageComplete);
            Assert.Equal(import3.PercentageComplete(), imports[2].PercentageComplete);
        }
    }

    internal static ImportStatusBauService BuildImportStatusBauService(
        IUserService? userService = null,
        ContentDbContext? contentDbContext = null)
    {
        return new ImportStatusBauService(
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            contentDbContext ?? new Mock<ContentDbContext>().Object
        );
    }
}
