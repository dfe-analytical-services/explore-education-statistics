#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// Checks that the rows added, updated and deleted by <see cref="DataSetFileDualWriteInterceptor"/> are saved in an
/// order which satisfies the foreign keys of the DataSetFile tables, which the EF Core in-memory provider doesn't
/// enforce.
/// </summary>
public class DataSetFileDualWriteInterceptorSqliteTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ReplaceThenDeleteOriginal_ForeignKeysSatisfied()
    {
        var role = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "BAU User",
            NormalizedName = "BAU USER",
        };

        // Users.CreatedById is a required self-referencing foreign key, so the user has to be created by itself
        var createdById = Guid.NewGuid();

        User createdBy = _dataFixture
            .DefaultUser()
            .WithId(createdById)
            .WithCreatedById(createdById)
            .WithRoleId(role.Id);

        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(
                _dataFixture
                    .DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication().WithTheme(_dataFixture.DefaultTheme()))
            )
            .WithCreated(createdById: createdBy.Id);

        var originalDataFile = _dataFixture.DefaultFile(FileType.Data).WithCreatedByUser(createdBy).Generate();
        var originalMetaFile = _dataFixture
            .DefaultFile(FileType.Metadata)
            .WithSubjectId(originalDataFile.SubjectId!.Value)
            .WithRootPath(originalDataFile.RootPath)
            .Generate();

        var originalDataReleaseFile = new ReleaseFile
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = releaseVersion.Id,
            File = originalDataFile,
        };

        var originalMetaReleaseFile = new ReleaseFile
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = releaseVersion.Id,
            File = originalMetaFile,
        };

        using var sqliteFixture = new SqliteContentDbContextFixture(enforceForeignKeys: true);

        await using (var context = sqliteFixture.CreateContext())
        {
            context.Set<IdentityRole>().Add(role);
            context.Users.Add(createdBy);
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.AddRange(originalDataReleaseFile, originalMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        var replacementDataFile = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileId(originalDataFile.DataSetFileId!.Value)
            .Generate();
        replacementDataFile.DataSetFileVersion = 1;

        var replacementDataReleaseFile = new ReleaseFile
        {
            Id = Guid.NewGuid(),
            ReleaseVersionId = releaseVersion.Id,
            File = replacementDataFile,
        };

        // As per ReleaseDataFileRepository.Create, the replacement is added and linked to the original in one save
        await using (var context = sqliteFixture.CreateContext())
        {
            var originalFile = await context.Files.SingleAsync(f => f.Id == originalDataFile.Id);

            replacementDataFile.Replacing = originalFile;
            await context.ReleaseFiles.AddAsync(replacementDataReleaseFile);
            context.Update(originalFile);
            originalFile.ReplacedBy = replacementDataFile;

            await context.SaveChangesAsync();
        }

        await using (var context = sqliteFixture.CreateContext())
        {
            var original = await context.DataSetFileVersions.SingleAsync(v => v.Id == originalDataFile.Id);
            var replacement = await context.DataSetFileVersions.SingleAsync(v => v.Id == replacementDataFile.Id);

            Assert.Equal(replacement.Id, original.ReplacedById);
            Assert.Equal(original.Id, replacement.ReplacingId);
        }

        // Deleting the original whilst the replacement is tracked has EF null the replacement's ReplacingId, which has
        // to be saved before the original's version is deleted
        await using (var context = sqliteFixture.CreateContext())
        {
            var files = await context.Files.Where(f => f.Type == FileType.Data).ToListAsync();
            var originalFile = files.Single(f => f.Id == originalDataFile.Id);

            originalFile.ReplacedById = null;
            context.ReleaseFiles.RemoveRange(
                context.ReleaseFiles.Where(rf => rf.FileId == originalDataFile.Id || rf.FileId == originalMetaFile.Id)
            );
            context.Files.RemoveRange(originalFile, await context.Files.SingleAsync(f => f.Id == originalMetaFile.Id));

            await context.SaveChangesAsync();
        }

        await using (var context = sqliteFixture.CreateContext())
        {
            var dataSetFile = await context.DataSetFiles.Include(dsf => dsf.Versions).SingleAsync();
            var version = Assert.Single(dataSetFile.Versions);

            Assert.Equal(replacementDataFile.Id, version.Id);
            Assert.Null(version.ReplacingId);
            Assert.Null(version.ReplacedById);
            Assert.Null(version.MetaFileId);

            var link = Assert.Single(await context.ReleaseVersionDataSetFileVersions.ToListAsync());
            Assert.Equal(replacementDataReleaseFile.Id, link.Id);
        }
    }
}
