using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class DataSetFileServiceTests
{
    [Fact]
    public async Task GetSitemapSummaries()
    {
        var dataSetFileId = Guid.NewGuid();
        var dataSetCreatedDate = new DateTime(2024, 05, 04, 10, 24, 13);

        var file = new File() { DataSetFileId = dataSetFileId, Created = dataSetCreatedDate };
        
        var releaseFile = new ReleaseFile
        {
            File = file,
            ReleaseVersion = new ReleaseVersion()
            {
                Id = Guid.NewGuid()
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddAsync(file);
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Attach(file);
            contentDbContext.Attach(releaseFile);

            var service = SetupDataSetFileService(contentDbContext);

            var result = (await service.GetSitemapSummaries()).AssertRight();

            Assert.Equal(dataSetFileId.ToString(), result.Single().Id);
            Assert.Equal(dataSetCreatedDate, result.Single().LastModified);
        }
    }

    private static DataSetFileService SetupDataSetFileService(
        ContentDbContext contentDbContext,
        IReleaseVersionRepository? releaseVersionRepository = null) =>
        new(
            contentDbContext,
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict)
        );
}
