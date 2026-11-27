using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

public class ReleaseDataFileRepositoryTests
{
    [Theory]
    [InlineData(FileType.Data, DataStorageVersion.StatsDB, DataStorageVersion.StatsDB, false)]
    [InlineData(FileType.Data, DataStorageVersion.ParquetV1, DataStorageVersion.ParquetV1, true)]
    [InlineData(FileType.Metadata, DataStorageVersion.ParquetV1, null, false)]
    public async Task Create_SetsDataStorageVersionAndHasParquetOnDataFilesOnly(
        FileType type,
        DataStorageVersion dataStorageVersion,
        DataStorageVersion? expectedDataStorageVersion,
        bool expectedHasParquet
    )
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        Guid fileId;
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var releaseDataFileRepository = new ReleaseDataFileRepository(contentDbContext);
            var file = await releaseDataFileRepository.Create(
                releaseVersionId: Guid.NewGuid(),
                subjectId: Guid.NewGuid(),
                filename: "data.csv",
                contentLength: 1024,
                type: type,
                createdById: Guid.NewGuid(),
                dataStorageVersion: dataStorageVersion
            );

            fileId = file.Id;
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var file = await contentDbContext.Files.SingleAsync(f => f.Id == fileId);
            Assert.Equal(expectedDataStorageVersion, file.DataStorageVersion);
            Assert.Equal(expectedHasParquet, file.HasParquet);
        }
    }
}
