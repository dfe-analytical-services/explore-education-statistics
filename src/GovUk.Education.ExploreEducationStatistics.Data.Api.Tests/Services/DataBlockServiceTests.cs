#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

public class DataBlockServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetDataBlockTableResult()
    {
        var subjectId = Guid.NewGuid();

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(releaseVersion)
                .WithSubjectId(subjectId)
                .Generate())
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.DataBlockParents.AddRangeAsync(dataBlockParent);
            await contentDbContext.SaveChangesAsync();
        }

        var tableBuilderResults = new TableBuilderResultViewModel
        {
            Results = new List<ObservationViewModel>
            {
                new()
            }
        };

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var (service, tableBuilderService) = BuildServiceAndDependencies(contentDbContext);

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            dataBlockVersion.ReleaseVersionId,
                            It.Is<FullTableQuery>(q => q.SubjectId == subjectId),
                            default
                        )
                )
                .ReturnsAsync(tableBuilderResults);

            var result = (await service.GetDataBlockTableResult(
                releaseVersionId: dataBlockVersion.ReleaseVersionId,
                dataBlockVersionId: dataBlockVersion.Id)).AssertRight();

            VerifyAllMocks(tableBuilderService);

            Assert.Equal(tableBuilderResults, result);
        }
    }

    [Fact]
    public async Task GetDataBlockTableResult_NotDataBlockType()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        var htmlBlock = new HtmlBlock
        {
            ReleaseVersion = new ReleaseVersion()
        };

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ContentBlocks.AddRangeAsync(htmlBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var (service, _) = BuildServiceAndDependencies(contentDbContext);

            var result = await service.GetDataBlockTableResult(
                releaseVersionId: htmlBlock.ReleaseVersionId,
                dataBlockVersionId: htmlBlock.Id);

            result.AssertNotFound();
        }
    }

    private static (
        DataBlockService service,
        Mock<ITableBuilderService> tableBuilderService)
        BuildServiceAndDependencies(ContentDbContext contentDbContext)
    {
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);
        var userService = AlwaysTrueUserService();

        var controller = new DataBlockService(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            tableBuilderService.Object,
            userService.Object
        );

        return (controller, tableBuilderService);
    }
}
