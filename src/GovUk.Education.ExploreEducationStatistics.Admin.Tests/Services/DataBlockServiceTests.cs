#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataBlockServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Get()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(
                        ListOf<IChart>(
                            new LineChart
                            {
                                Title = "Test chart",
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            Name = "test release file",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            DataBlockVersion = dataBlockVersion,
        };

        var dataBlockVersionLink = new DataBlockVersionLink
        {
            Id = dataBlockVersion.Id,
            DataBlockVersionId = dataBlockVersion.Id,
            DataBlockVersion = dataBlockVersion,
            ReleaseVersion = releaseVersion,
            Order = 5,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.Add(releaseFile);
            context.DataBlockVersions.Add(dataBlockVersion);
            context.DataBlockVersionLinks.Add(dataBlockVersionLink);
            context.FeaturedTables.Add(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Get(dataBlockVersion.Id);

            var retrievedResult = result.AssertRight();

            Assert.Equal(dataBlockVersion.Heading, retrievedResult.Heading);
            Assert.Equal(dataBlockVersion.Name, retrievedResult.Name);
            Assert.Equal(dataBlockVersion.Source, retrievedResult.Source);
            Assert.Equal(dataBlockVersionLink.Order, retrievedResult.Order);

            Assert.Equal(featuredTable.Name, retrievedResult.HighlightName);
            Assert.Equal(featuredTable.Description, retrievedResult.HighlightDescription);

            Assert.Equal(subjectId, retrievedResult.DataSetId);
            Assert.Equal("test release file", retrievedResult.DataSetName);

            dataBlockVersion.Query.AssertDeepEqualTo(retrievedResult.Query);
            dataBlockVersion.Table.AssertDeepEqualTo(retrievedResult.Table);
            dataBlockVersion.Charts.AssertDeepEqualTo(retrievedResult.Charts);
        }
    }

    [Fact]
    public async Task Get_NoFeaturedTable()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.Add(releaseFile);
            context.DataBlockVersions.Add(dataBlockVersion);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Get(dataBlockVersion.Id);

            var retrievedResult = result.AssertRight();

            Assert.Equal(dataBlockVersion.Name, retrievedResult.Name);

            Assert.Null(retrievedResult.HighlightName);
            Assert.Null(retrievedResult.HighlightDescription);
        }
    }

    [Fact]
    public async Task Get_ReleaseContentBlockFileWithoutNameReturnsEmptyString()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    // Set the name to null
                    .WithName(null)
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion, releaseFile);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Get(dataBlockVersion.Id);

            var retrievedResult = result.AssertRight();

            Assert.Equal(dataBlockVersion.Heading, retrievedResult.Heading);

            Assert.Equal(subjectId, retrievedResult.DataSetId);
            Assert.Equal(string.Empty, retrievedResult.DataSetName);
        }
    }

    [Fact]
    public async Task Get_ChartWithoutTitleReturnsHeading()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(
                        ListOf<IChart>(
                            new LineChart
                            {
                                // No title
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            Name = "test release file",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion, releaseFile);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Get(dataBlockVersion.Id);

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlockVersion.Heading, viewModel.Heading);
            dataBlockVersion.Charts.AssertDeepEqualTo(viewModel.Charts);

            Assert.Single(viewModel.Charts);
            Assert.Equal(dataBlockVersion.Heading, viewModel.Charts[0].Title);
        }
    }

    [Fact]
    public async Task Get_ContentBlockWithReleaseFileReturnsDataSetName()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .Generate()
            )
            .Generate();

        var releaseFile = new ReleaseFile
        {
            Name = "test file name",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion, releaseFile);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Get(dataBlockVersion.Id);

            var retrievedResult = result.AssertRight();

            Assert.Equal("test file name", retrievedResult.DataSetName);
            Assert.Equal(subjectId, retrievedResult.DataSetId);
        }
    }

    [Fact]
    public async Task Get_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();

        await using var context = InMemoryContentDbContext(contextId);
        var service = BuildDataBlockService(context);
        var result = await service.Get(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task Get_WrongRelease()
    {
        var dataBlockVersion = new DataBlockVersion { Name = "Test name" };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlockVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Get(dataBlockVersion.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task List()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlockVersion1 = new DataBlockVersion
        {
            Id = Guid.NewGuid(),
            Heading = "Test heading 1",
            Name = "Test name 1",
            Source = "Test source 1",
            Created = new DateTime(2000, 1, 1),
            Query = new FullTableQuery { Filters = [Guid.NewGuid()], Indicators = [Guid.NewGuid()] },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    Rows = [new(Guid.NewGuid().ToString(), TableHeaderType.Indicator)],
                    Columns = [new(Guid.NewGuid().ToString(), TableHeaderType.Filter)],
                },
            },
            Charts =
            [
                new LineChart
                {
                    Title = "Test chart 1",
                    Height = 400,
                    Width = 500,
                },
            ],
            ReleaseVersion = releaseVersion,
        };
        var featuredTable1 = new FeaturedTable
        {
            Name = "Test highlight name 1",
            Description = "Test highlight description 1",
            DataBlockVersion = dataBlockVersion1,
            ReleaseVersion = releaseVersion,
        };

        var dataBlockVersion2 = new DataBlockVersion
        {
            Id = Guid.NewGuid(),
            Heading = "Test heading 2",
            Name = "Test name 2",
            Source = "Test source 2",
            Created = new DateTime(2001, 2, 2),
            Query = new FullTableQuery { Filters = [Guid.NewGuid()], Indicators = [Guid.NewGuid()] },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    Rows = [new(Guid.NewGuid().ToString(), TableHeaderType.Indicator)],
                    Columns = [new(Guid.NewGuid().ToString(), TableHeaderType.Filter)],
                },
            },
            Charts = [],
            ReleaseVersion = releaseVersion,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Test highlight name 2",
            Description = "Test highlight description 2",
            DataBlockVersion = dataBlockVersion2,
            ReleaseVersion = releaseVersion,
        };

        // dataBlock1 is "in content" via a DataBlockVersionLink placed in a content section. dataBlock2 is not, and
        // so has no link at all - a DataBlockVersionLink only exists for as long as its version is placed.
        var dataBlockVersionLink1 = new DataBlockVersionLink
        {
            Id = dataBlockVersion1.Id,
            DataBlockVersionId = dataBlockVersion1.Id,
            DataBlockVersion = dataBlockVersion1,
            ReleaseVersion = releaseVersion,
            Order = 5,
            ContentSectionId = Guid.NewGuid(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion1, dataBlockVersion2);
            await context.DataBlockVersionLinks.AddAsync(dataBlockVersionLink1);
            await context.FeaturedTables.AddRangeAsync(featuredTable1, featuredTable2);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.List(releaseVersion.Id);

            var listResult = result.AssertRight();

            Assert.Equal(2, listResult.Count);

            Assert.Equal(dataBlockVersion1.Heading, listResult[0].Heading);
            Assert.Equal(dataBlockVersion1.Name, listResult[0].Name);
            Assert.Equal(dataBlockVersion1.Created, listResult[0].Created);
            Assert.Equal(featuredTable1.Name, listResult[0].HighlightName);
            Assert.Equal(featuredTable1.Description, listResult[0].HighlightDescription);
            Assert.Equal(dataBlockVersion1.Source, listResult[0].Source);
            Assert.Equal(1, listResult[0].ChartsCount);
            Assert.True(listResult[0].InContent);

            Assert.Equal(dataBlockVersion2.Heading, listResult[1].Heading);
            Assert.Equal(dataBlockVersion2.Name, listResult[1].Name);
            Assert.Equal(dataBlockVersion2.Created, listResult[1].Created);
            Assert.Equal(featuredTable2.Name, listResult[1].HighlightName);
            Assert.Equal(featuredTable2.Description, listResult[1].HighlightDescription);
            Assert.Equal(dataBlockVersion2.Source, listResult[1].Source);
            Assert.Equal(0, listResult[1].ChartsCount);
            Assert.False(listResult[1].InContent);
        }
    }

    [Fact]
    public async Task List_KeyStatisticInContent()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlockVersion = new DataBlockVersion { ReleaseVersion = releaseVersion };

        var keyStatistic = new KeyStatisticDataBlock
        {
            ReleaseVersion = releaseVersion,
            DataBlockVersion = dataBlockVersion,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion);
            await context.KeyStatisticsDataBlock.AddAsync(keyStatistic);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.List(releaseVersion.Id);

            var listResult = result.AssertRight();

            var responseDataBlock = Assert.Single(listResult);

            Assert.Equal(dataBlockVersion.Id, responseDataBlock.Id);
            Assert.True(responseDataBlock.InContent);
        }
    }

    [Fact]
    public async Task List_FiltersUnrelated()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var relatedDataBlockVersion = new DataBlockVersion
        {
            Id = Guid.NewGuid(),
            Heading = "Test heading 1",
            Name = "Test name 1",
            Source = "Test source 1",
            Created = new DateTime(2000, 1, 1),
            Query = new FullTableQuery { Filters = [Guid.NewGuid()], Indicators = [Guid.NewGuid()] },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    Rows = [new(Guid.NewGuid().ToString(), TableHeaderType.Indicator)],
                    Columns = [new(Guid.NewGuid().ToString(), TableHeaderType.Filter)],
                },
            },
            Charts =
            [
                new LineChart
                {
                    Title = "Test chart 1",
                    Height = 400,
                    Width = 500,
                },
            ],
            ReleaseVersion = releaseVersion,
        };
        var featuredTable1 = new FeaturedTable
        {
            Name = "Test highlight name 1",
            Description = "Test highlight description 1",
            DataBlockVersion = relatedDataBlockVersion,
            ReleaseVersion = releaseVersion,
        };
        var unrelatedDataBlockVersion = new DataBlockVersion
        {
            Name = "Test name 2",
            // This Data Block is attached to a different Release
            ReleaseVersion = new ReleaseVersion(),
        };

        var relatedDataBlockVersionLink = new DataBlockVersionLink
        {
            Id = relatedDataBlockVersion.Id,
            DataBlockVersionId = relatedDataBlockVersion.Id,
            DataBlockVersion = relatedDataBlockVersion,
            ReleaseVersion = releaseVersion,
            Order = 5,
            ContentSectionId = Guid.NewGuid(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(relatedDataBlockVersion, unrelatedDataBlockVersion);
            await context.DataBlockVersionLinks.AddAsync(relatedDataBlockVersionLink);
            await context.FeaturedTables.AddAsync(featuredTable1);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.List(releaseVersion.Id);

            var viewModel = Assert.Single(result.AssertRight());

            Assert.Equal(relatedDataBlockVersion.Heading, viewModel.Heading);
            Assert.Equal(relatedDataBlockVersion.Name, viewModel.Name);
            Assert.Equal(relatedDataBlockVersion.Created, viewModel.Created);
            Assert.Equal(featuredTable1.Name, viewModel.HighlightName);
            Assert.Equal(featuredTable1.Description, viewModel.HighlightDescription);
            Assert.Equal(relatedDataBlockVersion.Source, viewModel.Source);
            Assert.Equal(1, viewModel.ChartsCount);
            Assert.True(viewModel.InContent);
        }
    }

    [Fact]
    public async Task GetDeletePlan()
    {
        var fileId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithCharts(
                        ListOf<IChart>(
                            new InfographicChart
                            {
                                Title = "Test chart",
                                FileId = fileId.ToString(),
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var dataBlockVersionLink = new DataBlockVersionLink
        {
            Id = dataBlockVersion.Id,
            DataBlockVersionId = dataBlockVersion.Id,
            DataBlockVersion = dataBlockVersion,
            ReleaseVersion = releaseVersion,
        };

        releaseVersion.Content = _fixture
            .DefaultContentSection()
            .WithContentBlocks(ListOf<ContentBlock>(dataBlockVersionLink))
            .GenerateList(1);

        var file = new File { Id = fileId, Filename = "test-infographic.jpg" };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlockVersion);
            await context.AddAsync(file);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.GetDeletePlan(releaseVersion.Id, dataBlockVersion.Id);

            var deletePlan = result.AssertRight();

            Assert.Equal(releaseVersion.Id, deletePlan.ReleaseId);

            var dependentBlocks = deletePlan.DependentDataBlocks;

            Assert.Single(dependentBlocks);

            Assert.Equal(dataBlockVersion.Id, dependentBlocks[0].Id);
            Assert.Equal(dataBlockVersion.Name, dependentBlocks[0].Name);
            Assert.Equal(releaseVersion.Content[0].Heading, dependentBlocks[0].ContentSectionHeading);
            Assert.False(dependentBlocks[0].IsKeyStatistic);
            Assert.Null(dependentBlocks[0].FeaturedTable);

            Assert.Single(dependentBlocks[0].InfographicFilesInfo);

            Assert.Equal(file.Id, dependentBlocks[0].InfographicFilesInfo[0].Id);
            Assert.Equal(file.Filename, dependentBlocks[0].InfographicFilesInfo[0].Filename);
        }
    }

    [Fact]
    public async Task GetDeletePlan_DependentDataBlockIsKeyStatistic()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var keyStatistic = new KeyStatisticDataBlock { DataBlockVersion = dataBlockVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlockVersion);
            await context.KeyStatisticsDataBlock.AddAsync(keyStatistic);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.GetDeletePlan(releaseVersion.Id, dataBlockVersion.Id);

            var deletePlan = result.AssertRight();

            Assert.Equal(releaseVersion.Id, deletePlan.ReleaseId);

            var dependentBlocks = deletePlan.DependentDataBlocks;

            Assert.Single(dependentBlocks);

            Assert.Equal(dataBlockVersion.Id, dependentBlocks[0].Id);
            Assert.Equal(dataBlockVersion.Name, dependentBlocks[0].Name);
            Assert.Null(dependentBlocks[0].ContentSectionHeading);
            Assert.True(dependentBlocks[0].IsKeyStatistic);
        }
    }

    [Fact]
    public async Task GetDeletePlan_DependentDataBlockIncludesFeaturedTableDetails()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            DataBlockVersion = dataBlockVersion,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlockVersion);
            await context.FeaturedTables.AddAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.GetDeletePlan(releaseVersion.Id, dataBlockVersion.Id);

            var deletePlan = result.AssertRight();

            Assert.Equal(releaseVersion.Id, deletePlan.ReleaseId);

            var dependentBlocks = deletePlan.DependentDataBlocks;

            Assert.Single(dependentBlocks);

            Assert.Equal(dataBlockVersion.Id, dependentBlocks[0].Id);
            Assert.Equal(dataBlockVersion.Name, dependentBlocks[0].Name);
            Assert.NotNull(dependentBlocks[0].FeaturedTable);
            Assert.Equal(featuredTable.Name, dependentBlocks[0].FeaturedTable!.Name);
            Assert.Equal(featuredTable.Description, dependentBlocks[0].FeaturedTable!.Description);
        }
    }

    [Fact]
    public async Task GetDeletePlan_NotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.GetDeletePlan(releaseVersion.Id, Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetDeletePlan_WrongRelease()
    {
        var fileId = Guid.NewGuid();

        var dataBlockVersion = new DataBlockVersion
        {
            Name = "Test name",
            Charts =
            [
                new InfographicChart
                {
                    Title = "Test chart",
                    FileId = fileId.ToString(),
                    Height = 400,
                    Width = 500,
                },
            ],
            ReleaseVersion = new ReleaseVersion(),
        };
        var file = new File { Id = fileId, Filename = "test-infographic.jpg" };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlockVersion);
            await context.AddAsync(file);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.GetDeletePlan(Guid.NewGuid(), dataBlockVersion.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete()
    {
        var fileId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestDraftVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithCharts(
                        ListOf<IChart>(
                            new InfographicChart
                            {
                                Title = "Test chart",
                                FileId = fileId.ToString(),
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .WithReleaseVersion(releaseVersion)
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestDraftVersion!;

        releaseVersion.KeyStatistics = [new KeyStatisticDataBlock { DataBlockVersionId = dataBlockVersion.Id }];

        releaseVersion.FeaturedTables = ListOf(new FeaturedTable { DataBlockVersionId = dataBlockVersion.Id });

        var file = new File { Id = fileId, Filename = "test-infographic.jpg" };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlock);
            await context.AddAsync(file);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlock.LatestDraftVersion!, releaseVersion));
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            Assert.NotEmpty(context.DataBlockVersionLinks.ToList());
            Assert.NotEmpty(context.DataBlockVersions.ToList());
            Assert.NotEmpty(context.DataBlocks.ToList());
            Assert.NotEmpty(context.FeaturedTables.ToList());
            Assert.NotEmpty(context.KeyStatistics.ToList());
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(s => s.Delete(releaseVersion.Id, new List<Guid> { fileId }, false))
                .ReturnsAsync(Unit.Instance);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);

            var dataBlockVersionCacheKey = new DataBlockVersionTableResultCacheKey(dataBlockVersion);

            cacheKeyService
                .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                .ReturnsAsync(new Either<ActionResult, DataBlockVersionTableResultCacheKey>(dataBlockVersionCacheKey));

            var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

            privateCacheService.Setup(s => s.DeleteItemAsync(dataBlockVersionCacheKey)).Returns(Task.CompletedTask);

            var service = BuildDataBlockService(
                context,
                releaseFileService: releaseFileService.Object,
                cacheKeyService: cacheKeyService.Object,
                privateCacheService: privateCacheService.Object
            );

            var result = await service.Delete(releaseVersion.Id, dataBlockVersion.Id);

            VerifyAllMocks(releaseFileService, cacheKeyService, privateCacheService);

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            Assert.Empty(context.DataBlockVersionLinks.ToList());
            Assert.Empty(context.DataBlockVersions.ToList());
            Assert.Empty(context.DataBlocks.ToList());
            Assert.Empty(context.FeaturedTables.ToList());
            Assert.Empty(context.KeyStatistics.ToList());
        }
    }

    [Fact]
    public async Task Delete_WithVersionAlreadyPublished()
    {
        var fileId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestDraftVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithCharts(
                        ListOf<IChart>(
                            new InfographicChart
                            {
                                Title = "Test chart",
                                FileId = fileId.ToString(),
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .WithReleaseVersion(releaseVersion)
                    .Generate()
            )
            // In this test, the DataBlock also has an already-published DataBlockVersion which cannot be
            // deleted, and thus the parent will also not be deleted.
            .WithLatestPublishedVersion(_fixture.DefaultDataBlockVersion().Generate())
            .Generate();

        var draftDataBlockVersion = dataBlock.LatestDraftVersion!;
        var publishedDataBlockVersion = dataBlock.LatestPublishedVersion!;

        releaseVersion.KeyStatistics =
        [
            new KeyStatisticDataBlock { DataBlockVersionId = draftDataBlockVersion.Id },
            new KeyStatisticDataBlock { DataBlockVersionId = publishedDataBlockVersion.Id },
        ];

        releaseVersion.FeaturedTables = ListOf(
            new FeaturedTable { DataBlockVersionId = draftDataBlockVersion.Id },
            new FeaturedTable { DataBlockVersionId = publishedDataBlockVersion.Id }
        );

        var file = new File { Id = fileId, Filename = "test-infographic.jpg" };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlock);
            await context.AddAsync(file);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(draftDataBlockVersion, releaseVersion));
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(publishedDataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            Assert.NotEmpty(context.DataBlockVersionLinks.ToList());
            Assert.NotEmpty(context.DataBlockVersions.ToList());
            Assert.NotEmpty(context.DataBlocks.ToList());
            Assert.NotEmpty(context.FeaturedTables.ToList());
            Assert.NotEmpty(context.KeyStatistics.ToList());
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(s => s.Delete(releaseVersion.Id, new List<Guid> { fileId }, false))
                .ReturnsAsync(Unit.Instance);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);

            var dataBlockVersionCacheKey = new DataBlockVersionTableResultCacheKey(draftDataBlockVersion);

            cacheKeyService
                .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, draftDataBlockVersion.Id))
                .ReturnsAsync(new Either<ActionResult, DataBlockVersionTableResultCacheKey>(dataBlockVersionCacheKey));

            var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

            privateCacheService.Setup(s => s.DeleteItemAsync(dataBlockVersionCacheKey)).Returns(Task.CompletedTask);

            var service = BuildDataBlockService(
                context,
                releaseFileService: releaseFileService.Object,
                cacheKeyService: cacheKeyService.Object,
                privateCacheService: privateCacheService.Object
            );

            var result = await service.Delete(releaseVersion.Id, draftDataBlockVersion.Id);

            VerifyAllMocks(releaseFileService, cacheKeyService, privateCacheService);

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var remainingDataBlockVersionLink = Assert.Single(context.DataBlockVersionLinks.ToList());
            Assert.Equal(publishedDataBlockVersion.Id, remainingDataBlockVersionLink.Id);

            var remainingDataBlockVersion = Assert.Single(context.DataBlockVersions.ToList());
            Assert.Equal(publishedDataBlockVersion.Id, remainingDataBlockVersion.Id);

            var remainingDataBlock = Assert.Single(context.DataBlocks.ToList());

            // The already-published DataBlockVersion will remain unchanged until at such a point in time where this
            // Release Amendment is published, at which point it will be updated to null to indicate that this
            // Data Block is no longer publicly visible.
            Assert.Equal(publishedDataBlockVersion.Id, remainingDataBlock.LatestPublishedVersionId);

            // The latest draft DataBlockVersion will be set to null, as there is no longer a draft version as part
            // of this Release Amendment.
            Assert.Null(remainingDataBlock.LatestDraftVersionId);

            var remainingFeaturedTable = Assert.Single(context.FeaturedTables.ToList());
            Assert.Equal(publishedDataBlockVersion.Id, remainingFeaturedTable.DataBlockVersionId);

            var remainingKeyStatistic = Assert.IsType<KeyStatisticDataBlock>(
                Assert.Single(context.KeyStatistics.ToList())
            );
            Assert.Equal(publishedDataBlockVersion.Id, remainingKeyStatistic.DataBlockVersionId);
        }
    }

    [Fact]
    public async Task Delete_NotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Delete(releaseVersion.Id, Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_ReleaseNotFound()
    {
        var dataBlockVersion = new DataBlockVersion();

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddAsync(dataBlockVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Delete(Guid.NewGuid(), dataBlockVersion.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Create()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile = new ReleaseFile
        {
            Name = "test release file",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.Add(releaseFile);
            await context.SaveChangesAsync();
        }

        var createRequest = new DataBlockCreateRequest
        {
            Heading = "Test heading",
            Name = "Test name",
            Source = "Test source",
            Query = new FullTableQueryRequest
            {
                SubjectId = subjectId,
                Filters = new List<Guid> { Guid.NewGuid() },
                Indicators = new List<Guid> { Guid.NewGuid() },
            },
            Table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    Rows = [new(Guid.NewGuid().ToString(), TableHeaderType.Indicator)],
                    Columns = [new(Guid.NewGuid().ToString(), TableHeaderType.Filter)],
                },
            },
            Charts =
            [
                new LineChart
                {
                    Title = "Test chart",
                    Height = 600,
                    Width = 700,
                },
            ],
        };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Create(releaseVersion.Id, createRequest);

            var viewModel = result.AssertRight();

            Assert.Equal(createRequest.Heading, viewModel.Heading);
            Assert.Equal(createRequest.Name, viewModel.Name);
            Assert.Equal(createRequest.Source, viewModel.Source);

            createRequest.Query.AsFullTableQuery().AssertDeepEqualTo(viewModel.Query);
            Assert.Equal(createRequest.Table, viewModel.Table);
            Assert.Equal(createRequest.Charts, viewModel.Charts);

            Assert.Single(viewModel.Charts);
            Assert.NotEqual(createRequest.Heading, viewModel.Charts[0].Title);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlocks = context.DataBlocks.ToList();
            var dataBlockVersions = context.DataBlockVersions.ToList();

            // Validate that we have a new "DataBlock" to keep track of the various DataBlockVersions.
            // Assert as well that it does not currently have a LatestPublishedVersion as this is a new
            // DataBlock but instead has a LatestDraftVersion.
            var dataBlock = Assert.Single(dataBlocks);
            Assert.Null(dataBlock.LatestPublishedVersionId);
            Assert.NotEqual(Guid.Empty, dataBlock.LatestDraftVersionId);

            // Validate that we have a single "version 0" DataBlockVersion for this new DataBlock. Assert that it
            // is attached to its parent correctly and that it is recognised as the LatestDraftVersion.
            var dataBlockVersion = Assert.Single(dataBlockVersions);
            Assert.Equal(0, dataBlockVersion.Version);
            Assert.Equal(dataBlock.Id, dataBlockVersion.DataBlockId);
            Assert.Equal(dataBlock.LatestDraftVersionId, dataBlockVersion.Id);

            // A newly created DataBlock is not yet placed in a content section, so it has no DataBlockVersionLink.
            Assert.Empty(context.DataBlockVersionLinks.ToList());

            // Assert that the new DataBlock is connected correctly to its owning Release.
            Assert.Equal(releaseVersion.Id, dataBlockVersion.ReleaseVersionId);

            // Assert that the DataBlockVersion has a Created date, but no Updated or Published dates at this time.
            dataBlockVersion.Created.AssertUtcNow();
            Assert.Null(dataBlockVersion.Updated);
            Assert.Null(dataBlockVersion.Published);

            Assert.Equal(createRequest.Heading, dataBlockVersion.Heading);
            Assert.Equal(createRequest.Name, dataBlockVersion.Name);
            Assert.Equal(createRequest.Source, dataBlockVersion.Source);

            createRequest.Query.AsFullTableQuery().AssertDeepEqualTo(dataBlockVersion.Query);
            createRequest.Table.AssertDeepEqualTo(dataBlockVersion.Table);
            createRequest.Charts.AssertDeepEqualTo(dataBlockVersion.Charts);

            var savedRelease = await context.ReleaseVersions.FirstOrDefaultAsync(rv => rv.Id == releaseVersion.Id);

            Assert.NotNull(savedRelease);

            // No ContentBlock is created either, as the only ContentBlock a data block owns is its
            // DataBlockVersionLink.
            Assert.Empty(context.ContentBlocks.Where(block => block.ReleaseVersionId == releaseVersion.Id).ToList());
        }
    }

    [Fact]
    public async Task Create_BlankChartTitleUsesHeading()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFile = new ReleaseFile
        {
            Name = "test release file",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.Add(releaseFile);
            await context.SaveChangesAsync();
        }

        var createRequest = new DataBlockCreateRequest
        {
            Heading = "Test heading",
            Name = "Test name",
            Query = new FullTableQueryRequest { SubjectId = subjectId },
            Charts =
            [
                new LineChart
                {
                    // No title
                    Height = 600,
                    Width = 700,
                },
            ],
        };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Create(releaseVersion.Id, createRequest);

            var viewModel = result.AssertRight();

            Assert.Equal(createRequest.Heading, viewModel.Heading);
            Assert.Equal(createRequest.Charts, viewModel.Charts);

            Assert.Single(viewModel.Charts);
            Assert.Equal(createRequest.Heading, viewModel.Charts[0].Title);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlocks = context.DataBlockVersions.ToList();

            Assert.Single(dataBlocks);

            var dataBlock = dataBlocks[0];

            Assert.Equal(createRequest.Heading, dataBlock.Heading);
            createRequest.Charts.AssertDeepEqualTo(dataBlock.Charts);

            Assert.Single(dataBlock.Charts);
            Assert.Equal(createRequest.Heading, dataBlock.Charts[0].Title);
        }
    }

    [Fact]
    public async Task Create_ReleaseNotFound()
    {
        var contextId = Guid.NewGuid().ToString();

        var createRequest = new DataBlockCreateRequest { Heading = "Heading 1", Name = "Name 1" };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildDataBlockService(context);
            var result = await service.Create(Guid.NewGuid(), createRequest);

            result.AssertNotFound();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlocks = context.DataBlockVersionLinks.ToList();

            Assert.Empty(dataBlocks);
        }
    }

    [Fact]
    public async Task Update()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(
                        ListOf<IChart>(
                            new LineChart
                            {
                                Title = "Old chart",
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            Name = "test file",
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var dataBlockVersionLink = new DataBlockVersionLink
        {
            Id = dataBlockVersion.Id,
            DataBlockVersionId = dataBlockVersion.Id,
            DataBlockVersion = dataBlockVersion,
            ReleaseVersion = releaseVersion,
            Order = 5,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion, releaseFile);
            await context.DataBlockVersionLinks.AddAsync(dataBlockVersionLink);
            await context.SaveChangesAsync();
        }

        var updateRequest = new DataBlockUpdateRequest
        {
            Heading = "New heading",
            Name = "New name",
            Source = "New source",
            Query = new FullTableQueryRequest { SubjectId = subjectId },
            Charts =
            [
                new LineChart
                {
                    Title = "New chart",
                    Height = 600,
                    Width = 700,
                },
            ],
        };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);

            var dataBlockVersionCacheKey = new DataBlockVersionTableResultCacheKey(dataBlockVersion);

            cacheKeyService
                .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                .ReturnsAsync(new Either<ActionResult, DataBlockVersionTableResultCacheKey>(dataBlockVersionCacheKey));

            var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

            privateCacheService.Setup(s => s.DeleteItemAsync(dataBlockVersionCacheKey)).Returns(Task.CompletedTask);

            var service = BuildDataBlockService(
                context,
                cacheKeyService: cacheKeyService.Object,
                privateCacheService: privateCacheService.Object
            );

            var result = await service.Update(dataBlockVersion.Id, updateRequest);

            VerifyAllMocks(cacheKeyService, privateCacheService);

            var updateResult = result.AssertRight();

            Assert.Equal(dataBlockVersion.Id, updateResult.Id);
            Assert.Equal(updateRequest.Heading, updateResult.Heading);
            Assert.Equal(updateRequest.Name, updateResult.Name);
            Assert.Equal(updateRequest.Source, updateResult.Source);
            Assert.Equal(dataBlockVersionLink.Order, updateResult.Order);
            Assert.Equal(subjectId, updateResult.DataSetId);
            Assert.Equal("test file", updateResult.DataSetName);

            updateRequest.Query.AsFullTableQuery().AssertDeepEqualTo(updateResult.Query);
            Assert.Equal(updateRequest.Table, updateResult.Table);
            Assert.Equal(updateRequest.Charts, updateResult.Charts);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var updatedDataBlock = await context.DataBlockVersions.FindAsync(dataBlockVersion.Id);

            Assert.NotNull(updatedDataBlock);
            Assert.Equal(updateRequest.Heading, updatedDataBlock.Heading);
            Assert.Equal(updateRequest.Name, updatedDataBlock.Name);
            Assert.Equal(updateRequest.Source, updatedDataBlock.Source);

            updateRequest.Query.AsFullTableQuery().AssertDeepEqualTo(updatedDataBlock.Query);
            updateRequest.Table.AssertDeepEqualTo(updatedDataBlock.Table);
            updateRequest.Charts.AssertDeepEqualTo(updatedDataBlock.Charts);
        }
    }

    [Fact]
    public async Task Update_HeadingUpdateAlsoChangesChartTitle()
    {
        var subjectId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(
                        ListOf<IChart>(
                            new LineChart
                            {
                                // No title
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion, releaseFile);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        var updateRequest = new DataBlockUpdateRequest
        {
            Heading = "New heading",
            Name = "New name",
            Query = new FullTableQueryRequest { SubjectId = subjectId },
            Charts =
            [
                new LineChart
                {
                    // No title
                    Height = 600,
                    Width = 700,
                },
            ],
        };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);

            var dataBlockVersionCacheKey = new DataBlockVersionTableResultCacheKey(dataBlockVersion);

            cacheKeyService
                .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                .ReturnsAsync(new Either<ActionResult, DataBlockVersionTableResultCacheKey>(dataBlockVersionCacheKey));

            var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

            privateCacheService.Setup(s => s.DeleteItemAsync(dataBlockVersionCacheKey)).Returns(Task.CompletedTask);

            var service = BuildDataBlockService(
                context,
                cacheKeyService: cacheKeyService.Object,
                privateCacheService: privateCacheService.Object
            );

            var result = await service.Update(dataBlockVersion.Id, updateRequest);

            VerifyAllMocks(cacheKeyService, privateCacheService);

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlockVersion.Id, viewModel.Id);
            Assert.Equal(updateRequest.Heading, viewModel.Heading);
            Assert.Equal(updateRequest.Charts, viewModel.Charts);

            Assert.Single(viewModel.Charts);
            Assert.Equal(updateRequest.Heading, viewModel.Charts[0].Title);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var updatedDataBlock = await context.DataBlockVersions.FindAsync(dataBlockVersion.Id);

            Assert.Equal(updateRequest.Heading, updatedDataBlock!.Heading);
            updateRequest.Charts.AssertDeepEqualTo(updatedDataBlock.Charts);

            Assert.Single(updatedDataBlock.Charts);
            Assert.Equal(updateRequest.Heading, updatedDataBlock.Charts[0].Title);
        }
    }

    [Fact]
    public async Task Update_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();

        await using var context = InMemoryContentDbContext(contextId);

        var service = BuildDataBlockService(context);
        var result = await service.Update(
            Guid.NewGuid(),
            new DataBlockUpdateRequest { Heading = "Heading 1", Name = "Name 1" }
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task Update_RemoveOldInfographic()
    {
        var subjectId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlock = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(
                _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(
                        ListOf<IChart>(
                            new InfographicChart
                            {
                                Title = "Old chart",
                                FileId = fileId.ToString(),
                                Height = 400,
                                Width = 500,
                            }
                        )
                    )
                    .Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlock.LatestPublishedVersion!;

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                Filename = "test filename",
                Type = FileType.Data,
            },
        };

        var file = new File { Id = fileId, Filename = "test-infographic.jpg" };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.AddRangeAsync(dataBlockVersion, file, releaseFile);
            context.DataBlockVersionLinks.Add(BuildDataBlockVersionLink(dataBlockVersion, releaseVersion));
            await context.SaveChangesAsync();
        }

        var updateRequest = new DataBlockUpdateRequest
        {
            Heading = "Test heading",
            Name = "Test name",
            Query = new FullTableQueryRequest { SubjectId = subjectId },
            Charts =
            [
                new LineChart
                {
                    Title = "New chart",
                    Height = 600,
                    Width = 700,
                },
            ],
        };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService.Setup(s => s.Delete(releaseVersion.Id, fileId, false)).ReturnsAsync(Unit.Instance);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);

            var dataBlockVersionCacheKey = new DataBlockVersionTableResultCacheKey(dataBlockVersion);

            cacheKeyService
                .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                .ReturnsAsync(new Either<ActionResult, DataBlockVersionTableResultCacheKey>(dataBlockVersionCacheKey));

            var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

            privateCacheService.Setup(s => s.DeleteItemAsync(dataBlockVersionCacheKey)).Returns(Task.CompletedTask);

            var service = BuildDataBlockService(
                context,
                releaseFileService: releaseFileService.Object,
                cacheKeyService: cacheKeyService.Object,
                privateCacheService: privateCacheService.Object
            );

            var result = await service.Update(dataBlockVersion.Id, updateRequest);

            VerifyAllMocks(releaseFileService, cacheKeyService, privateCacheService);

            var updateResult = result.AssertRight();

            Assert.Equal(updateRequest.Charts, updateResult.Charts);
        }
    }

    [Fact]
    public async Task GetUnattachedDataBlocks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var dataBlocks = _fixture
            .DefaultDataBlock()
            .WithLatestPublishedVersion(() =>
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .GenerateList(4);

        var unattachedDataBlockVersion1 = dataBlocks[0].LatestPublishedVersion!;
        var unattachedDataBlockVersion2 = dataBlocks[1].LatestPublishedVersion!;
        var attachedDataBlockVersion1 = dataBlocks[2].LatestPublishedVersion!;
        var attachedDataBlockVersion2 = dataBlocks[3].LatestPublishedVersion!;

        var keyStat = new KeyStatisticDataBlock
        {
            ReleaseVersion = releaseVersion,
            // This Data Block is "attached" because it's used with a Key Stat.
            DataBlockVersion = attachedDataBlockVersion1,
        };

        // Only a DataBlockVersion that is placed in a content section has a DataBlockVersionLink, so just the
        // content-attached one gets a link here. The key-stat one is excluded via its key stat instead, and the two
        // unattached ones have no link at all.
        var contentLink = BuildDataBlockVersionLink(attachedDataBlockVersion2, releaseVersion);

        releaseVersion.Content = _fixture
            .DefaultContentSection()
            .WithContentBlocks(
                ListOf<ContentBlock>(
                    // This Data Block is "attached" because it's used within Release Content.
                    contentLink,
                    new HtmlBlock()
                )
            )
            .GenerateList(1);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Add an unrelated Data Block link for a different Release.
            await contentDbContext.ContentBlocks.AddRangeAsync(
                new DataBlockVersionLink { ContentSection = new(), ReleaseVersion = new ReleaseVersion() }
            );
            await contentDbContext.DataBlocks.AddRangeAsync(dataBlocks);
            await contentDbContext.KeyStatisticsDataBlock.AddRangeAsync(keyStat);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildDataBlockService(contentDbContext: contentDbContext);
            var result = await service.GetUnattachedDataBlocks(releaseVersion.Id);

            var unattachedDataBlocks = result.AssertRight();

            Assert.Equal(2, unattachedDataBlocks.Count);

            Assert.Equal(unattachedDataBlockVersion1.Id, unattachedDataBlocks[0].Id);
            Assert.Equal(unattachedDataBlockVersion1.Name, unattachedDataBlocks[0].Name);
            Assert.Equal(unattachedDataBlockVersion2.Id, unattachedDataBlocks[1].Id);
            Assert.Equal(unattachedDataBlockVersion2.Name, unattachedDataBlocks[1].Name);
        }
    }

    [Fact]
    public async Task GetUnattachedDataBlocks_NoRelease()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = BuildDataBlockService(contentDbContext: contentDbContext);
        var result = await service.GetUnattachedDataBlocks(Guid.NewGuid());

        result.AssertNotFound();
    }

    private static DataBlockVersionLink BuildDataBlockVersionLink(
        DataBlockVersion dataBlockVersion,
        ReleaseVersion releaseVersion
    ) =>
        new()
        {
            Id = dataBlockVersion.Id,
            DataBlockVersionId = dataBlockVersion.Id,
            DataBlockVersion = dataBlockVersion,
            ReleaseVersion = releaseVersion,
        };

    private static DataBlockService BuildDataBlockService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IReleaseFileService? releaseFileService = null,
        IUserService? userService = null,
        IPrivateBlobCacheService? privateCacheService = null,
        ICacheKeyService? cacheKeyService = null
    )
    {
        var service = new DataBlockService(
            contentDbContext,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
            userService ?? AlwaysTrueUserService().Object,
            AdminMapper(),
            privateCacheService ?? Mock.Of<IPrivateBlobCacheService>(Strict),
            cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict)
        );

        return service;
    }
}
