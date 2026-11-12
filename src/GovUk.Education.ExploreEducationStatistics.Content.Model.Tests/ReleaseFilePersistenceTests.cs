using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public class ReleaseFilePersistenceTests
{
    private readonly DataFixture _fixture = new();

    public class PairedIndicatorsTests : ReleaseFilePersistenceTests
    {
        [Fact]
        public async Task PairedIndicatorsAreStoredPerReleaseVersionForTheSameDataFile()
        {
            var (releaseVersion1, releaseVersion2, releaseVersion3) = _fixture.DefaultReleaseVersion().GenerateTuple3();

            var file = _fixture.DefaultFile().WithType(FileType.Data).Generate();

            List<PairedIndicatorEntry> pairedIndicators1 =
            [
                new(ParentIndicatorId: Guid.NewGuid(), ChildIndicatorId: Guid.NewGuid()),
                new(ParentIndicatorId: Guid.NewGuid(), ChildIndicatorId: Guid.NewGuid()),
            ];

            List<PairedIndicatorEntry> pairedIndicators2 =
            [
                new(ParentIndicatorId: Guid.NewGuid(), ChildIndicatorId: Guid.NewGuid()),
            ];

            var releaseFile1 = _fixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion1)
                .WithFile(file)
                .WithPairedIndicators(pairedIndicators1)
                .Generate();

            var releaseFile2 = _fixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion2)
                .WithFile(file)
                .WithPairedIndicators(pairedIndicators2)
                .Generate();

            // No paired indicators configured
            var releaseFile3 = _fixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion3)
                .WithFile(file)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseFiles.AddRange(releaseFile1, releaseFile2, releaseFile3);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var savedReleaseFiles = await context.ReleaseFiles.Where(rf => rf.FileId == file.Id).ToListAsync();

                Assert.Equal(3, savedReleaseFiles.Count);

                savedReleaseFiles
                    .Single(rf => rf.ReleaseVersionId == releaseVersion1.Id)
                    .PairedIndicators.AssertDeepEqualTo(pairedIndicators1);

                savedReleaseFiles
                    .Single(rf => rf.ReleaseVersionId == releaseVersion2.Id)
                    .PairedIndicators.AssertDeepEqualTo(pairedIndicators2);

                Assert.Null(savedReleaseFiles.Single(rf => rf.ReleaseVersionId == releaseVersion3.Id).PairedIndicators);
            }
        }
    }
}
