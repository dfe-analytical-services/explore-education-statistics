#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task Get()
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
                    .WithCharts(ListOf<IChart>(new LineChart
                    {
                        Title = "Test chart",
                        Height = 400,
                        Width = 500,
                    }))
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var releaseFile = new ReleaseFile
            {
                Name = "test release file",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var featuredTable = new FeaturedTable
            {
                Name = "Featured table name",
                Description = "Featured table description",
                DataBlock = dataBlockVersion.ContentBlock,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.ReleaseFiles.Add(releaseFile);
                context.DataBlockVersions.Add(dataBlockVersion);
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
                Assert.Equal(dataBlockVersion.Order, retrievedResult.Order);

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

            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.ReleaseFiles.Add(releaseFile);
                context.DataBlockVersions.Add(dataBlockVersion);
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

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    // Set the name to null
                    .WithName(null)
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlockVersion, releaseFile);
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

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(ListOf<IChart>(new LineChart
                    {
                        // No title
                        Height = 400,
                        Width = 500,
                    }))
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var releaseFile = new ReleaseFile
            {
                Name = "test release file",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlockVersion, releaseFile);
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

            var releaseFile = new ReleaseFile
            {
                Name = "test file name",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlockVersion, releaseFile);
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
            var dataBlock = new DataBlock
            {
                Name = "Test name",
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(dataBlock);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(dataBlock.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task List()
        {
            var releaseVersion = new ReleaseVersion();

            var dataBlock1 = new DataBlock
            {
                Heading = "Test heading 1",
                Name = "Test name 1",
                Source = "Test source 1",
                Order = 5,
                Created = new DateTime(2000, 1, 1),
                ContentSectionId = Guid.NewGuid(),
                Query = new FullTableQuery
                {
                    Filters = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                    Indicators = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader>
                        {
                            new TableHeader(Guid.NewGuid().ToString(), TableHeaderType.Indicator)
                        },
                        Columns = new List<TableHeader>
                        {
                            new TableHeader(Guid.NewGuid().ToString(), TableHeaderType.Filter)
                        }
                    }
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Title = "Test chart 1",
                        Height = 400,
                        Width = 500,
                    }
                },
                ReleaseVersion = releaseVersion
            };
            var featuredTable1 = new FeaturedTable
            {
                Name = "Test highlight name 1",
                Description = "Test highlight description 1",
                DataBlock = dataBlock1,
                ReleaseVersion = releaseVersion,
            };

            var dataBlock2 = new DataBlock
            {
                Heading = "Test heading 2",
                Name = "Test name 2",
                Source = "Test source 2",
                Order = 7,
                Created = new DateTime(2001, 2, 2),
                Query = new FullTableQuery
                {
                    Filters = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                    Indicators = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader>
                        {
                            new TableHeader(Guid.NewGuid().ToString(), TableHeaderType.Indicator)
                        },
                        Columns = new List<TableHeader>
                        {
                            new TableHeader(Guid.NewGuid().ToString(), TableHeaderType.Filter)
                        }
                    }
                },
                Charts = new List<IChart>(),
                ReleaseVersion = releaseVersion
            };
            var featuredTable2 = new FeaturedTable
            {
                Name = "Test highlight name 2",
                Description = "Test highlight description 2",
                DataBlock = dataBlock2,
                ReleaseVersion = releaseVersion
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlock1, dataBlock2);
                await context.FeaturedTables.AddRangeAsync(featuredTable1, featuredTable2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(releaseVersion.Id);

                var listResult = result.AssertRight();

                Assert.Equal(2, listResult.Count);

                Assert.Equal(dataBlock1.Heading, listResult[0].Heading);
                Assert.Equal(dataBlock1.Name, listResult[0].Name);
                Assert.Equal(dataBlock1.Created, listResult[0].Created);
                Assert.Equal(featuredTable1.Name, listResult[0].HighlightName);
                Assert.Equal(featuredTable1.Description, listResult[0].HighlightDescription);
                Assert.Equal(dataBlock1.Source, listResult[0].Source);
                Assert.Equal(1, listResult[0].ChartsCount);
                Assert.True(listResult[0].InContent);

                Assert.Equal(dataBlock2.Heading, listResult[1].Heading);
                Assert.Equal(dataBlock2.Name, listResult[1].Name);
                Assert.Equal(dataBlock2.Created, listResult[1].Created);
                Assert.Equal(featuredTable2.Name, listResult[1].HighlightName);
                Assert.Equal(featuredTable2.Description, listResult[1].HighlightDescription);
                Assert.Equal(dataBlock2.Source, listResult[1].Source);
                Assert.Equal(0, listResult[1].ChartsCount);
                Assert.False(listResult[1].InContent);
            }
        }

        [Fact]
        public async Task List_KeyStatisticInContent()
        {
            var releaseVersion = new ReleaseVersion();

            var dataBlock = new DataBlock
            {
                ContentSectionId = null,
                ReleaseVersion = releaseVersion
            };

            var keyStatistic = new KeyStatisticDataBlock
            {
                ReleaseVersion = releaseVersion,
                DataBlock = dataBlock,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlock);
                await context.KeyStatisticsDataBlock.AddAsync(keyStatistic);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(releaseVersion.Id);

                var listResult = result.AssertRight();

                var responseDataBlock = Assert.Single(listResult);

                Assert.Equal(dataBlock.Id, responseDataBlock.Id);
                Assert.True(responseDataBlock.InContent);
            }
        }

        [Fact]
        public async Task List_FiltersUnrelated()
        {
            var releaseVersion = new ReleaseVersion();

            var relatedDataBlock = new DataBlock
            {
                Heading = "Test heading 1",
                Name = "Test name 1",
                Source = "Test source 1",
                Order = 5,
                Created = new DateTime(2000, 1, 1),
                ContentSectionId = Guid.NewGuid(),
                Query = new FullTableQuery
                {
                    Filters = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                    Indicators = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader>
                        {
                            new TableHeader(Guid.NewGuid().ToString(), TableHeaderType.Indicator)
                        },
                        Columns = new List<TableHeader>
                        {
                            new TableHeader(Guid.NewGuid().ToString(), TableHeaderType.Filter)
                        }
                    }
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Title = "Test chart 1",
                        Height = 400,
                        Width = 500,
                    }
                },
                ReleaseVersion = releaseVersion
            };
            var featuredTable1 = new FeaturedTable
            {
                Name = "Test highlight name 1",
                Description = "Test highlight description 1",
                DataBlock = relatedDataBlock,
                ReleaseVersion = releaseVersion,
            };
            var unrelatedDataBlock = new DataBlock
            {
                Name = "Test name 2",
                // This Data Block is attached to a different Release
                ReleaseVersion = new ReleaseVersion()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(relatedDataBlock, unrelatedDataBlock);
                await context.FeaturedTables.AddAsync(featuredTable1);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(releaseVersion.Id);

                var viewModel = Assert.Single(result.AssertRight());

                Assert.Equal(relatedDataBlock.Heading, viewModel.Heading);
                Assert.Equal(relatedDataBlock.Name, viewModel.Name);
                Assert.Equal(relatedDataBlock.Created, viewModel.Created);
                Assert.Equal(featuredTable1.Name, viewModel.HighlightName);
                Assert.Equal(featuredTable1.Description, viewModel.HighlightDescription);
                Assert.Equal(relatedDataBlock.Source, viewModel.Source);
                Assert.Equal(1, viewModel.ChartsCount);
                Assert.True(viewModel.InContent);
            }
        }

        [Fact]
        public async Task GetDeletePlan()
        {
            var fileId = Guid.NewGuid();

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithCharts(ListOf<IChart>(new InfographicChart
                    {
                        Title = "Test chart",
                        FileId = fileId.ToString(),
                        Height = 400,
                        Width = 500,
                    }))
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            releaseVersion.Content = _fixture
                .DefaultContentSection()
                .WithContentBlocks(ListOf<ContentBlock>(dataBlockVersion.ContentBlock))
                .GenerateList(1);

            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

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
                Assert.Equal(dataBlockVersion.ContentSection!.Heading, dependentBlocks[0].ContentSectionHeading);
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
            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var keyStatistic = new KeyStatisticDataBlock
            {
                DataBlock = dataBlockVersion.ContentBlock,
            };

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
                Assert.Null(dataBlockVersion.ContentSection);
                Assert.True(dependentBlocks[0].IsKeyStatistic);
            }
        }

        [Fact]
        public async Task GetDeletePlan_DependentDataBlockIncludesFeaturedTableDetails()
        {
            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var featuredTable = new FeaturedTable
            {
                Name = "Featured table name",
                Description = "Featured table description",
                DataBlock = dataBlockVersion.ContentBlock,
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
                Assert.Null(dataBlockVersion.ContentSection);
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

            var dataBlock = new DataBlock
            {
                Name = "Test name",
                Charts = new List<IChart>
                {
                    new InfographicChart
                    {
                        Title = "Test chart",
                        FileId = fileId.ToString(),
                        Height = 400,
                        Width = 500,
                    }
                },
                ContentSection = new ContentSection
                {
                    Heading = "Test heading"
                },
                ReleaseVersion = new ReleaseVersion()
            };
            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(dataBlock);
                await context.AddAsync(file);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.GetDeletePlan(Guid.NewGuid(), dataBlock.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Delete()
        {
            var fileId = Guid.NewGuid();

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestDraftVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithCharts(ListOf<IChart>(
                        new InfographicChart
                        {
                            Title = "Test chart",
                            FileId = fileId.ToString(),
                            Height = 400,
                            Width = 500,
                        }))
                    .WithReleaseVersion(releaseVersion)
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestDraftVersion!;

            releaseVersion.KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticDataBlock
                {
                    DataBlockId = dataBlockVersion.Id
                },
            };

            releaseVersion.FeaturedTables = ListOf(new FeaturedTable
            {
                DataBlockId = dataBlockVersion.Id
            });

            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(dataBlockParent);
                await context.AddAsync(file);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.NotEmpty(context.DataBlocks.ToList());
                Assert.NotEmpty(context.DataBlockVersions.ToList());
                Assert.NotEmpty(context.DataBlockParents.ToList());
                Assert.NotEmpty(context.FeaturedTables.ToList());
                Assert.NotEmpty(context.KeyStatistics.ToList());
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>(Strict);

                releaseFileService
                    .Setup(
                        s =>
                            s.Delete(releaseVersion.Id, new List<Guid> {fileId}, false)
                    )
                    .ReturnsAsync(Unit.Instance);

                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

                privateCacheService
                    .Setup(s => s.DeleteItemAsync(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context,
                    releaseFileService: releaseFileService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    privateCacheService: privateCacheService.Object);

                var result = await service.Delete(releaseVersion.Id, dataBlockVersion.Id);

                VerifyAllMocks(releaseFileService, cacheKeyService, privateCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.Empty(context.DataBlocks.ToList());
                Assert.Empty(context.DataBlockVersions.ToList());
                Assert.Empty(context.DataBlockParents.ToList());
                Assert.Empty(context.FeaturedTables.ToList());
                Assert.Empty(context.KeyStatistics.ToList());
            }
        }

        [Fact]
        public async Task Delete_WithVersionAlreadyPublished()
        {
            var fileId = Guid.NewGuid();

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestDraftVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithCharts(ListOf<IChart>(
                        new InfographicChart
                        {
                            Title = "Test chart",
                            FileId = fileId.ToString(),
                            Height = 400,
                            Width = 500,
                        }))
                    .WithReleaseVersion(releaseVersion)
                    .Generate())
                // In this test, the DataBlockParent also has an already-published DataBlockVersion which cannot be
                // deleted, and thus the parent will also not be deleted.
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .Generate())
                .Generate();

            var draftDataBlockVersion = dataBlockParent.LatestDraftVersion!;
            var publishedDataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            releaseVersion.KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticDataBlock
                {
                    DataBlockId = draftDataBlockVersion.Id
                },
                new KeyStatisticDataBlock
                {
                    DataBlockId = publishedDataBlockVersion.Id
                }
            };

            releaseVersion.FeaturedTables = ListOf(
                new FeaturedTable
                {
                    DataBlockId = draftDataBlockVersion.Id
                },
                new FeaturedTable
                {
                    DataBlockId = publishedDataBlockVersion.Id
                }
            );

            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(dataBlockParent);
                await context.AddAsync(file);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.NotEmpty(context.DataBlocks.ToList());
                Assert.NotEmpty(context.DataBlockVersions.ToList());
                Assert.NotEmpty(context.DataBlockParents.ToList());
                Assert.NotEmpty(context.FeaturedTables.ToList());
                Assert.NotEmpty(context.KeyStatistics.ToList());
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>(Strict);

                releaseFileService
                    .Setup(
                        s =>
                            s.Delete(releaseVersion.Id, new List<Guid> { fileId }, false)
                    )
                    .ReturnsAsync(Unit.Instance);

                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(draftDataBlockVersion);

                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, draftDataBlockVersion.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

                privateCacheService
                    .Setup(s => s.DeleteItemAsync(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context,
                    releaseFileService: releaseFileService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    privateCacheService: privateCacheService.Object);

                var result = await service.Delete(releaseVersion.Id, draftDataBlockVersion.Id);

                VerifyAllMocks(releaseFileService, cacheKeyService, privateCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingDataBlock = Assert.Single(context.DataBlocks.ToList());
                Assert.Equal(publishedDataBlockVersion.Id, remainingDataBlock.Id);

                var remainingDataBlockVersion = Assert.Single(context.DataBlockVersions.ToList());
                Assert.Equal(publishedDataBlockVersion.Id, remainingDataBlockVersion.Id);

                var remainingDataBlockParent = Assert.Single(context.DataBlockParents.ToList());

                // The already-published DataBlockVersion will remain unchanged until at such a point in time where this
                // Release Amendment is published, at which point it will be updated to null to indicate that this
                // Data Block is no longer publicly visible.
                Assert.Equal(publishedDataBlockVersion.Id, remainingDataBlockParent.LatestPublishedVersionId);

                // The latest draft DataBlockVersion will be set to null, as there is no longer a draft version as part
                // of this Release Amendment.
                Assert.Null(remainingDataBlockParent.LatestDraftVersionId);

                var remainingFeaturedTable = Assert.Single(context.FeaturedTables.ToList());
                Assert.Equal(publishedDataBlockVersion.Id, remainingFeaturedTable.DataBlockId);

                var remainingKeyStatistic = Assert.IsType<KeyStatisticDataBlock>(Assert.Single(context.KeyStatistics.ToList()));
                Assert.Equal(publishedDataBlockVersion.Id, remainingKeyStatistic.DataBlockId);
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
            var dataBlock = new DataBlock();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(dataBlock);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Delete(Guid.NewGuid(), dataBlock.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Create()
        {
            var subjectId = Guid.NewGuid();

            var releaseVersion = new ReleaseVersion();

            var releaseFile = new ReleaseFile
            {
                Name = "test release file",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
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
                    Filters = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                    Indicators = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader>
                        {
                            new (Guid.NewGuid().ToString(), TableHeaderType.Indicator)
                        },
                        Columns = new List<TableHeader>
                        {
                            new (Guid.NewGuid().ToString(), TableHeaderType.Filter)
                        }
                    }
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Title = "Test chart",
                        Height = 600,
                        Width = 700,
                    }
                },
            };

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Create(releaseVersion.Id, createRequest);

                var viewModel = result.AssertRight();

                Assert.Equal(createRequest.Heading, viewModel.Heading);
                Assert.Equal(createRequest.Name, viewModel.Name);
                Assert.Equal(createRequest.Source, viewModel.Source);

                createRequest.Query.AsFullTableQuery()
                    .AssertDeepEqualTo(viewModel.Query);
                Assert.Equal(createRequest.Table, viewModel.Table);
                Assert.Equal(createRequest.Charts, viewModel.Charts);

                Assert.Single(viewModel.Charts);
                Assert.NotEqual(createRequest.Heading, viewModel.Charts[0].Title);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var dataBlockParents = context.DataBlockParents.ToList();
                var dataBlockVersions = context.DataBlockVersions.ToList();
                var dataBlocks = context.DataBlocks.ToList();

                // Validate that we have a new "DataBlockParent" to keep track of the various DataBlockVersions.
                // Assert as well that it does not currently have a LatestPublishedVersion as this is a new
                // DataBlock but instead has a LatestDraftVersion.
                var dataBlockParent = Assert.Single(dataBlockParents);
                Assert.Null(dataBlockParent.LatestPublishedVersionId);
                Assert.NotEqual(Guid.Empty, dataBlockParent.LatestDraftVersionId);

                // Validate that we have a single "version 0" DataBlockVersion for this new DataBlock. Assert that it
                // is attached to its parent correctly, that is recognised as the LatestDraftVersion, and that it is
                // attached to the underlying ContentBlock successfully.
                var dataBlockVersion = Assert.Single(dataBlockVersions);
                var dataBlock = Assert.Single(dataBlocks);
                Assert.Equal(0, dataBlockVersion.Version);
                Assert.Equal(dataBlock.Id, dataBlockVersion.ContentBlockId);
                Assert.Equal(dataBlockParent.Id, dataBlockVersion.DataBlockParentId);
                Assert.Equal(dataBlockParent.LatestDraftVersionId, dataBlockVersion.Id);

                // Assert that the new DataBlock is connected correctly to its owning Release.
                Assert.Equal(releaseVersion.Id, dataBlockVersion.ReleaseVersionId);
                Assert.Equal(releaseVersion.Id, dataBlock.ReleaseVersionId);

                // Assert that the DataBlockVersion has a Created date, but no Updated or Published dates at this time.
                dataBlockVersion.Created.AssertUtcNow();
                Assert.Null(dataBlockVersion.Updated);
                Assert.Null(dataBlockVersion.Published);

                dataBlock.Created.AssertUtcNow();
                Assert.Null(dataBlock.Updated);

                Assert.Equal(createRequest.Heading, dataBlock.Heading);
                Assert.Equal(createRequest.Name, dataBlock.Name);
                Assert.Equal(createRequest.Source, dataBlock.Source);

                createRequest.Query.AsFullTableQuery()
                    .AssertDeepEqualTo(dataBlock.Query);
                createRequest.Table.AssertDeepEqualTo(dataBlock.Table);
                createRequest.Charts.AssertDeepEqualTo(dataBlock.Charts);

                var savedRelease = await context
                    .ReleaseVersions
                    .FirstOrDefaultAsync(rv => rv.Id == releaseVersion.Id);

                var savedDataBlocks = context
                    .ContentBlocks
                    .Where(block => block.ReleaseVersionId == releaseVersion.Id)
                    .ToList();

                Assert.NotNull(savedRelease);
                Assert.Single(savedDataBlocks);
                Assert.Equal(dataBlock, savedDataBlocks[0]);
            }
        }

        [Fact]
        public async Task Create_BlankChartTitleUsesHeading()
        {
            var subjectId = Guid.NewGuid();

            var releaseVersion = new ReleaseVersion();

            var releaseFile = new ReleaseFile
            {
                Name = "test release file",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
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
                Query = new FullTableQueryRequest
                {
                    SubjectId = subjectId
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        // No title
                        Height = 600,
                        Width = 700,
                    }
                },
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
                var dataBlocks = context.DataBlocks.ToList();

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

            var createRequest = new DataBlockCreateRequest
            {
                Heading = "Heading 1",
                Name = "Name 1",
            };

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Create(Guid.NewGuid(), createRequest);

                result.AssertNotFound();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var dataBlocks = context.DataBlocks.ToList();

                Assert.Empty(dataBlocks);
            }
        }

        [Fact]
        public async Task Update()
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
                    .WithCharts(ListOf<IChart>(
                        new LineChart
                        {
                            Title = "Old chart",
                            Height = 400,
                            Width = 500,
                        }))
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var releaseFile = new ReleaseFile
            {
                Name = "test file",
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlockVersion, releaseFile);
                await context.SaveChangesAsync();
            }

            var updateRequest = new DataBlockUpdateRequest
            {
                Heading = "New heading",
                Name = "New name",
                Source = "New source",
                Query = new FullTableQueryRequest
                {
                    SubjectId = subjectId
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Title = "New chart",
                        Height = 600,
                        Width = 700,
                    }
                }
            };

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

                privateCacheService
                    .Setup(s => s.DeleteItemAsync(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context,
                    cacheKeyService: cacheKeyService.Object,
                    privateCacheService: privateCacheService.Object);

                var result = await service.Update(dataBlockVersion.Id, updateRequest);

                VerifyAllMocks(cacheKeyService, privateCacheService);

                var updateResult = result.AssertRight();

                Assert.Equal(dataBlockVersion.Id, updateResult.Id);
                Assert.Equal(updateRequest.Heading, updateResult.Heading);
                Assert.Equal(updateRequest.Name, updateResult.Name);
                Assert.Equal(updateRequest.Source, updateResult.Source);
                Assert.Equal(dataBlockVersion.Order, updateResult.Order);
                Assert.Equal(subjectId, updateResult.DataSetId);
                Assert.Equal("test file", updateResult.DataSetName);

                updateRequest.Query.AsFullTableQuery()
                    .AssertDeepEqualTo(updateResult.Query);
                Assert.Equal(updateRequest.Table, updateResult.Table);
                Assert.Equal(updateRequest.Charts, updateResult.Charts);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataBlock = await context.DataBlocks.FindAsync(dataBlockVersion.Id);

                Assert.NotNull(updatedDataBlock);
                Assert.Equal(updateRequest.Heading, updatedDataBlock.Heading);
                Assert.Equal(updateRequest.Name, updatedDataBlock.Name);
                Assert.Equal(updateRequest.Source, updatedDataBlock.Source);

                updateRequest.Query.AsFullTableQuery()
                    .AssertDeepEqualTo(updatedDataBlock.Query);
                updateRequest.Table.AssertDeepEqualTo(updatedDataBlock.Table);
                updateRequest.Charts.AssertDeepEqualTo(updatedDataBlock.Charts);
            }
        }

        [Fact]
        public async Task Update_HeadingUpdateAlsoChangesChartTitle()
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
                    .WithCharts(ListOf<IChart>(
                        new LineChart
                        {
                            // No title
                            Height = 400,
                            Width = 500,
                        }))
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlockVersion, releaseFile);
                await context.SaveChangesAsync();
            }

            var updateRequest = new DataBlockUpdateRequest
            {
                Heading = "New heading",
                Name = "New name",
                Query = new FullTableQueryRequest
                {
                    SubjectId = subjectId
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        // No title
                        Height = 600,
                        Width = 700,
                    }
                },
            };

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

                privateCacheService
                    .Setup(s => s.DeleteItemAsync(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context,
                    cacheKeyService: cacheKeyService.Object,
                    privateCacheService: privateCacheService.Object);

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
                var updatedDataBlock = await context.DataBlocks.FindAsync(dataBlockVersion.Id);

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
            var result = await service.Update(Guid.NewGuid(),
                new DataBlockUpdateRequest
                {
                    Heading = "Heading 1",
                    Name = "Name 1",
                });

            result.AssertNotFound();
        }

        [Fact]
        public async Task Update_RemoveOldInfographic()
        {
            var subjectId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithSubjectId(subjectId)
                    .WithCharts(ListOf<IChart>(
                        new InfographicChart
                        {
                            Title = "Old chart",
                            FileId = fileId.ToString(),
                            Height = 400,
                            Width = 500,
                        }))
                    .Generate())
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var releaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subjectId,
                    Filename = "test filename",
                    Type = FileType.Data
                }
            };

            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(dataBlockVersion, file, releaseFile);
                await context.SaveChangesAsync();
            }

            var updateRequest = new DataBlockUpdateRequest
            {
                Heading = "Test heading",
                Name = "Test name",
                Query = new FullTableQueryRequest
                {
                    SubjectId = subjectId
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Title = "New chart",
                        Height = 600,
                        Width = 700,
                    }
                },
            };

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>(Strict);

                releaseFileService
                    .Setup(s => s.Delete(releaseVersion.Id, fileId, false))
                    .ReturnsAsync(Unit.Instance);

                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(releaseVersion.Id, dataBlockVersion.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var privateCacheService = new Mock<IPrivateBlobCacheService>(Strict);

                privateCacheService
                    .Setup(s => s.DeleteItemAsync(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context,
                    releaseFileService: releaseFileService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    privateCacheService: privateCacheService.Object);

                var result = await service.Update(dataBlockVersion.Id, updateRequest);

                VerifyAllMocks(releaseFileService, cacheKeyService, privateCacheService);

                var updateResult = result.AssertRight();

                Assert.Equal(updateRequest.Charts, updateResult.Charts);
            }
        }

        [Fact]
        public async Task GetUnattachedDataBlocks()
        {
            var releaseVersion = _fixture
                .DefaultReleaseVersion()
                .Generate();

            var dataBlockParents = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(() => _fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .Generate())
                .GenerateList(4);

            var unattachedDataBlockVersion1 = dataBlockParents[0].LatestPublishedVersion!;
            var unattachedDataBlockVersion2 = dataBlockParents[1].LatestPublishedVersion!;
            var attachedDataBlockVersion1 = dataBlockParents[2].LatestPublishedVersion!;
            var attachedDataBlockVersion2 = dataBlockParents[3].LatestPublishedVersion!;

            var keyStat = new KeyStatisticDataBlock
            {
                ReleaseVersion = releaseVersion,
                // This Data Block is "attached" because it's used with a Key Stat.
                DataBlock = attachedDataBlockVersion1.ContentBlock,
            };

            releaseVersion.Content = _fixture
                .DefaultContentSection()
                .WithContentBlocks(ListOf<ContentBlock>(
                    // This Data Block is "attached" because it's used within Release Content.
                    attachedDataBlockVersion2.ContentBlock,
                    new HtmlBlock()
                ))
                .GenerateList(1);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Add an unrelated Data Block.
                await contentDbContext.ContentBlocks.AddRangeAsync(new DataBlock
                {
                    Name = "Unattached for different Release",
                    ContentSection = null,
                    ReleaseVersion = new ReleaseVersion()
                });
                await contentDbContext.DataBlockParents.AddRangeAsync(dataBlockParents);
                await contentDbContext.KeyStatisticsDataBlock.AddRangeAsync(keyStat);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataBlockService(contentDbContext: contentDbContext);
                var result = await service.GetUnattachedDataBlocks(
                    releaseVersion.Id);

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
            var result = await service.GetUnattachedDataBlocks(
                Guid.NewGuid());

            result.AssertNotFound();
        }

        private static DataBlockService BuildDataBlockService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IReleaseFileService? releaseFileService = null,
            IUserService? userService = null,
            IPrivateBlobCacheService? privateCacheService = null,
            ICacheKeyService? cacheKeyService = null)
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
}
