#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var dataBlock = new DataBlock
            {
                Heading = "Test heading",
                Name = "Test name",
                HighlightName = "Test highlight name",
                HighlightDescription = "Test highlight description",
                Source = "Test source",
                Order = 5,
                Query = new ObservationQueryContext
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
                        Title = "Test chart",
                        Height = 400,
                        Width = 500,
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = new Release(),
                        ContentBlock = dataBlock,
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(dataBlock.Id);

                var retrievedResult = result.AssertRight();

                Assert.Equal(dataBlock.Heading, retrievedResult.Heading);
                Assert.Equal(dataBlock.Name, retrievedResult.Name);
                Assert.Equal(dataBlock.HighlightName, retrievedResult.HighlightName);
                Assert.Equal(dataBlock.HighlightDescription, retrievedResult.HighlightDescription);
                Assert.Equal(dataBlock.Source, retrievedResult.Source);
                Assert.Equal(dataBlock.Order, retrievedResult.Order);

                dataBlock.Query.AssertDeepEqualTo(retrievedResult.Query);
                dataBlock.Table.AssertDeepEqualTo(retrievedResult.Table);
                dataBlock.Charts.AssertDeepEqualTo(retrievedResult.Charts);
            }
        }

        [Fact]
        public async Task Get_ChartWithoutTitleReturnsHeading()
        {
            var dataBlock = new DataBlock
            {
                Heading = "Test heading",
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        // No title
                        Height = 400,
                        Width = 500,
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = new Release(),
                        ContentBlock = dataBlock,
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(dataBlock.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(dataBlock.Heading, viewModel.Heading);
                dataBlock.Charts.AssertDeepEqualTo(viewModel.Charts);

                Assert.Single(viewModel.Charts);
                Assert.Equal(dataBlock.Heading, viewModel.Charts[0].Title);
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
            var release = new Release();

            var dataBlock1 = new DataBlock
            {
                Heading = "Test heading 1",
                Name = "Test name 1",
                HighlightName = "Test highlight name 1",
                HighlightDescription = "Test highlight description 1",
                Source = "Test source 1",
                Order = 5,
                Created = new DateTime(2000, 1, 1),
                ContentSectionId = Guid.NewGuid(),
                Query = new ObservationQueryContext
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
            };

            var dataBlock2 = new DataBlock
            {
                Heading = "Test heading 2",
                Name = "Test name 2",
                HighlightName = "Test highlight name 2",
                HighlightDescription = "Test highlight description 2",
                Source = "Test source 2",
                Order = 7,
                Created = new DateTime(2001, 2, 2),
                Query = new ObservationQueryContext
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
                Charts = new List<IChart>()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock1
                    },
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock2
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(release.Id);

                var listResult = result.AssertRight();

                Assert.Equal(2, listResult.Count);

                Assert.Equal(dataBlock1.Heading, listResult[0].Heading);
                Assert.Equal(dataBlock1.Name, listResult[0].Name);
                Assert.Equal(dataBlock1.Created, listResult[0].Created);
                Assert.Equal(dataBlock1.HighlightName, listResult[0].HighlightName);
                Assert.Equal(dataBlock1.HighlightDescription, listResult[0].HighlightDescription);
                Assert.Equal(dataBlock1.Source, listResult[0].Source);
                Assert.Equal(1, listResult[0].ChartsCount);
                Assert.True(listResult[0].InContent);

                Assert.Equal(dataBlock2.Heading, listResult[1].Heading);
                Assert.Equal(dataBlock2.Name, listResult[1].Name);
                Assert.Equal(dataBlock2.Created, listResult[1].Created);
                Assert.Equal(dataBlock2.HighlightName, listResult[1].HighlightName);
                Assert.Equal(dataBlock2.HighlightDescription, listResult[1].HighlightDescription);
                Assert.Equal(dataBlock2.Source, listResult[1].Source);
                Assert.Equal(0, listResult[1].ChartsCount);
                Assert.False(listResult[1].InContent);
            }
        }

        [Fact]
        public async Task List_KeyStatisticInContent()
        {
            var release = new Release();

            var dataBlock = new DataBlock
            {
                ContentSectionId = null,
            };

            var keyStatistic = new KeyStatisticDataBlock
            {
                Release = release,
                DataBlock = dataBlock,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock,
                    }
                );
                await context.KeyStatisticsDataBlock.AddAsync(keyStatistic);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(release.Id);

                var listResult = result.AssertRight();

                var responseDataBlock = Assert.Single(listResult);

                Assert.Equal(dataBlock.Id, responseDataBlock.Id);
                Assert.True(responseDataBlock.InContent);
            }
        }

        [Fact]
        public async Task List_FiltersUnrelated()
        {
            var release = new Release();

            var dataBlock1 = new DataBlock
            {
                Heading = "Test heading 1",
                Name = "Test name 1",
                HighlightName = "Test highlight name 1",
                HighlightDescription = "Test highlight description 1",
                Source = "Test source 1",
                Order = 5,
                Created = new DateTime(2000, 1, 1),
                ContentSectionId = Guid.NewGuid(),
                Query = new ObservationQueryContext
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
            };

            var dataBlock2 = new DataBlock
            {
                Name = "Test name 2",
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock1
                    },
                    new ReleaseContentBlock
                    {
                        Release = new Release(),
                        ContentBlock = dataBlock2
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(release.Id);

                var viewModel = Assert.Single(result.AssertRight());

                Assert.Equal(dataBlock1.Heading, viewModel.Heading);
                Assert.Equal(dataBlock1.Name, viewModel.Name);
                Assert.Equal(dataBlock1.Created, viewModel.Created);
                Assert.Equal(dataBlock1.HighlightName, viewModel.HighlightName);
                Assert.Equal(dataBlock1.HighlightDescription, viewModel.HighlightDescription);
                Assert.Equal(dataBlock1.Source, viewModel.Source);
                Assert.Equal(1, viewModel.ChartsCount);
                Assert.True(viewModel.InContent);
            }
        }

        [Fact]
        public async Task GetDeletePlan()
        {
            var release = new Release();
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
                await context.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock
                    }
                );
                await context.AddAsync(file);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.GetDeletePlan(release.Id, dataBlock.Id);

                var deletePlan = result.AssertRight();
                
                Assert.Equal(release.Id, deletePlan.ReleaseId);

                var dependentBlocks = deletePlan.DependentDataBlocks;

                Assert.Single(dependentBlocks);

                Assert.Equal(dataBlock.Id, dependentBlocks[0].Id);
                Assert.Equal(dataBlock.Name, dependentBlocks[0].Name);
                Assert.Equal(dataBlock.ContentSection.Heading, dependentBlocks[0].ContentSectionHeading);
                Assert.False(dependentBlocks[0].IsKeyStatistic);

                Assert.Single(dependentBlocks[0].InfographicFilesInfo);

                Assert.Equal(file.Id, dependentBlocks[0].InfographicFilesInfo[0].Id);
                Assert.Equal(file.Filename, dependentBlocks[0].InfographicFilesInfo[0].Filename);
            }
        }

        [Fact]
        public async Task GetDeletePlan_DependentDataBlockIsKeyStatistic()
        {
            var release = new Release();

            var dataBlock = new DataBlock();

            var keyStatistic = new KeyStatisticDataBlock
            {
                DataBlock = dataBlock,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock
                    }
                );
                await context.KeyStatisticsDataBlock.AddAsync(keyStatistic);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.GetDeletePlan(release.Id, dataBlock.Id);

                var deletePlan = result.AssertRight();

                Assert.Equal(release.Id, deletePlan.ReleaseId);

                var dependentBlocks = deletePlan.DependentDataBlocks;

                Assert.Single(dependentBlocks);

                Assert.Equal(dataBlock.Id, dependentBlocks[0].Id);
                Assert.Equal(dataBlock.Name, dependentBlocks[0].Name);
                Assert.Null(dataBlock.ContentSection);
                Assert.True(dependentBlocks[0].IsKeyStatistic);
            }
        }


        [Fact]
        public async Task GetDeletePlan_NotFound()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.GetDeletePlan(release.Id, Guid.NewGuid());

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
                await context.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = new Release(),
                        ContentBlock = dataBlock
                    }
                );
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
            var dataBlockId = Guid.NewGuid();
            var keyStatId = Guid.NewGuid();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication    = new Publication
                {
                    Id = Guid.NewGuid(),
                },
                KeyStatistics = new List<KeyStatistic>
                {
                    new KeyStatisticDataBlock
                    {
                        Id = keyStatId,
                        DataBlockId = dataBlockId,
                    },
                },
            };

            var fileId = Guid.NewGuid();

            var dataBlock = new DataBlock
            {
                Id = dataBlockId,
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
                }
            };
            
            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = release,
                ContentBlock = dataBlock
            };

            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(releaseContentBlock);
                await context.AddAsync(file);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var releaseFileService = new Mock<IReleaseFileService>(Strict);

                releaseFileService
                    .Setup(
                        s =>
                            s.Delete(release.Id, new List<Guid> {fileId}, false)
                    )
                    .ReturnsAsync(Unit.Instance);

                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(releaseContentBlock);
                
                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(release.Id, dataBlock.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var cacheService = new Mock<IBlobCacheService>(Strict);

                cacheService
                    .Setup(s => s.DeleteItem(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context, 
                    releaseFileService: releaseFileService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    cacheService: cacheService.Object);
                
                var result = await service.Delete(release.Id, dataBlock.Id);

                VerifyAllMocks(releaseFileService, cacheKeyService, cacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.Empty(context.DataBlocks.ToList());
                Assert.Empty(context.ContentBlocks.ToList());
                Assert.Empty(context.ReleaseContentBlocks.ToList());
                Assert.Empty(context.KeyStatistics.ToList());
            }
        }

        [Fact]
        public async Task Delete_NotFound()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Delete(release.Id, Guid.NewGuid());

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
                await context.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = new Release(),
                        ContentBlock = dataBlock
                    }
                );
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
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var createRequest = new DataBlockCreateViewModel
            {
                Heading = "Test heading",
                Name = "Test name",
                HighlightName = "Test highlight name",
                HighlightDescription = "Test highlight description",
                Source = "Test source",
                Query = new ObservationQueryContext
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
                        Title = "Test chart",
                        Height = 600,
                        Width = 700,
                    }
                },
            };

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Create(release.Id, createRequest);

                var viewModel = result.AssertRight();

                Assert.Equal(createRequest.Heading, viewModel.Heading);
                Assert.Equal(createRequest.Name, viewModel.Name);
                Assert.Equal(createRequest.HighlightName, viewModel.HighlightName);
                Assert.Equal(createRequest.HighlightDescription, viewModel.HighlightDescription);
                Assert.Equal(createRequest.Source, viewModel.Source);

                createRequest.Query.AssertDeepEqualTo(viewModel.Query);
                Assert.Equal(createRequest.Table, viewModel.Table);
                Assert.Equal(createRequest.Charts, viewModel.Charts);

                Assert.Single(viewModel.Charts);
                Assert.NotEqual(createRequest.Heading, viewModel.Charts[0].Title);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var dataBlocks = context.DataBlocks.ToList();

                var dataBlock = Assert.Single(dataBlocks);

                // Validate Created date is in the DB, even if not returned in result
                Assert.True(dataBlock.Created.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(dataBlock.Created!.Value).Milliseconds, 0, 1500);

                Assert.Equal(createRequest.Heading, dataBlock.Heading);
                Assert.Equal(createRequest.Name, dataBlock.Name);
                Assert.Equal(createRequest.HighlightName, dataBlock.HighlightName);
                Assert.Equal(createRequest.HighlightDescription, dataBlock.HighlightDescription);
                Assert.Equal(createRequest.Source, dataBlock.Source);

                createRequest.Query.AssertDeepEqualTo(dataBlock.Query);
                createRequest.Table.AssertDeepEqualTo(dataBlock.Table);
                createRequest.Charts.AssertDeepEqualTo(dataBlock.Charts);

                var savedRelease = await context.Releases
                    .Include(r => r.ContentBlocks)
                    .FirstOrDefaultAsync(r => r.Id == release.Id);

                Assert.NotNull(savedRelease);
                Assert.Single(savedRelease!.ContentBlocks);
                Assert.Equal(dataBlock, savedRelease.ContentBlocks[0].ContentBlock);
            }
        }

        [Fact]
        public async Task Create_BlankChartTitleUsesHeading()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            var createRequest = new DataBlockCreateViewModel
            {
                Heading = "Test heading",
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
                var result = await service.Create(release.Id, createRequest);

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

            var createRequest = new DataBlockCreateViewModel();

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
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var dataBlock = new DataBlock
            {
                Heading = "Old heading",
                Name = "Old name",
                HighlightName = "Old highlight name",
                HighlightDescription = "Old highlight description",
                Source = "Old source",
                Order = 5,
                Query = new ObservationQueryContext
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
                            new(Guid.NewGuid().ToString(), TableHeaderType.Indicator)
                        },
                        Columns = new List<TableHeader>
                        {
                            new(Guid.NewGuid().ToString(), TableHeaderType.Filter)
                        }
                    }
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Title = "Old chart",
                        Height = 400,
                        Width = 500,
                    }
                }
            };
            
            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = release,
                ContentBlock = dataBlock
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(
                    releaseContentBlock
                );
                await context.SaveChangesAsync();
            }

            var updateRequest = new DataBlockUpdateViewModel
            {
                Heading = "New heading",
                Name = "New name",
                HighlightName = "New highlight name",
                HighlightDescription = "New highlight description",
                Source = "New source",
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
                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(releaseContentBlock);
                
                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(release.Id, dataBlock.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var cacheService = new Mock<IBlobCacheService>(Strict);

                cacheService
                    .Setup(s => s.DeleteItem(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context, 
                    cacheKeyService: cacheKeyService.Object, 
                    cacheService: cacheService.Object);
                
                var result = await service.Update(dataBlock.Id, updateRequest);
                
                VerifyAllMocks(cacheKeyService, cacheService);
                
                var updateResult = result.AssertRight();

                Assert.Equal(dataBlock.Id, updateResult.Id);
                Assert.Equal(updateRequest.Heading, updateResult.Heading);
                Assert.Equal(updateRequest.Name, updateResult.Name);
                Assert.Equal(updateRequest.HighlightName, updateResult.HighlightName);
                Assert.Equal(updateRequest.HighlightDescription, updateResult.HighlightDescription);
                Assert.Equal(updateRequest.Source, updateResult.Source);
                Assert.Equal(dataBlock.Order, updateResult.Order);

                Assert.Equal(updateRequest.Query, updateResult.Query);
                Assert.Equal(updateRequest.Table, updateResult.Table);
                Assert.Equal(updateRequest.Charts, updateResult.Charts);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataBlock = await context.DataBlocks.FindAsync(dataBlock.Id);

                Assert.Equal(updateRequest.Heading, updatedDataBlock!.Heading);
                Assert.Equal(updateRequest.Name, updatedDataBlock.Name);
                Assert.Equal(updateRequest.HighlightName, updatedDataBlock.HighlightName);
                Assert.Equal(updateRequest.HighlightDescription, updatedDataBlock.HighlightDescription);
                Assert.Equal(updateRequest.Source, updatedDataBlock.Source);

                updateRequest.Query.AssertDeepEqualTo(updatedDataBlock.Query);
                updateRequest.Table.AssertDeepEqualTo(updatedDataBlock.Table);
                updateRequest.Charts.AssertDeepEqualTo(updatedDataBlock.Charts);
            }
        }

        [Fact]
        public async Task Update_HeadingUpdateAlsoChangesChartTitle()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };

            var dataBlock = new DataBlock
            {
                Heading = "Old heading",
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        // No title
                        Height = 400,
                        Width = 500,
                    }
                },
            };
            
            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = release,
                ContentBlock = dataBlock
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(releaseContentBlock);
                await context.SaveChangesAsync();
            }

            var updateRequest = new DataBlockUpdateViewModel
            {
                Heading = "New heading",
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

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(releaseContentBlock);
                
                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(release.Id, dataBlock.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var cacheService = new Mock<IBlobCacheService>(Strict);

                cacheService
                    .Setup(s => s.DeleteItem(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context, 
                    cacheKeyService: cacheKeyService.Object, 
                    cacheService: cacheService.Object);
                
                var result = await service.Update(dataBlock.Id, updateRequest);
                
                VerifyAllMocks(cacheKeyService, cacheService);

                var viewModel = result.AssertRight();

                Assert.Equal(dataBlock.Id, viewModel.Id);
                Assert.Equal(updateRequest.Heading, viewModel.Heading);
                Assert.Equal(updateRequest.Charts, viewModel.Charts);

                Assert.Single(viewModel.Charts);
                Assert.Equal(updateRequest.Heading, viewModel.Charts[0].Title);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataBlock = await context.DataBlocks.FindAsync(dataBlock.Id);

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
            var result = await service.Update(Guid.NewGuid(), new DataBlockUpdateViewModel());

            result.AssertNotFound();
        }

        [Fact]
        public async Task Update_RemoveOldInfographic()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid()
                }
            };
            
            var fileId = Guid.NewGuid();

            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>
                {
                    new InfographicChart
                    {
                        Title = "Old chart",
                        FileId = fileId.ToString(),
                        Height = 400,
                        Width = 500,
                    }
                },
            };
            
            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = release,
                ContentBlock = dataBlock
            };

            var file = new File
            {
                Id = fileId,
                Filename = "test-infographic.jpg"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(releaseContentBlock);
                await context.AddAsync(file);
                await context.SaveChangesAsync();
            }

            var updateRequest = new DataBlockUpdateViewModel
            {
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
                    .Setup(s => s.Delete(release.Id, fileId, false))
                    .ReturnsAsync(Unit.Instance);
                
                var cacheKeyService = new Mock<ICacheKeyService>(Strict);

                var dataBlockCacheKey = new DataBlockTableResultCacheKey(releaseContentBlock);
                
                cacheKeyService
                    .Setup(s => s.CreateCacheKeyForDataBlock(release.Id, dataBlock.Id))
                    .ReturnsAsync(new Either<ActionResult, DataBlockTableResultCacheKey>(dataBlockCacheKey));

                var cacheService = new Mock<IBlobCacheService>(Strict);

                cacheService
                    .Setup(s => s.DeleteItem(dataBlockCacheKey))
                    .Returns(Task.CompletedTask);

                var service = BuildDataBlockService(
                    context, 
                    releaseFileService: releaseFileService.Object,
                    cacheKeyService: cacheKeyService.Object, 
                    cacheService: cacheService.Object);

                var result = await service.Update(dataBlock.Id, updateRequest);
                
                VerifyAllMocks(releaseFileService, cacheKeyService, cacheService);

                var updateResult = result.AssertRight();

                Assert.Equal(updateRequest.Charts, updateResult.Charts);
            }
        }

        [Fact]
        public async Task GetUnattachedDataBlocks()
        {
            var release = new Release();

            var unattachedDataBlock1Id = Guid.NewGuid();
            var unattachedDataBlock2Id = Guid.NewGuid();

            var attachedDataBlock1Id = Guid.NewGuid();
            var keyStat = new KeyStatisticDataBlock
            {
                Release = release,
                DataBlockId = attachedDataBlock1Id,
            };

            var releaseContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    Release = release,
                    ContentBlock = new DataBlock
                    {
                        Id = attachedDataBlock1Id, // attached to key stat
                        Name = "Attached 1",
                        ContentSectionId = null,
                    },
                },
                new()
                {
                    Release = release,
                    ContentBlock = new DataBlock
                    {
                        Id = unattachedDataBlock1Id,
                        Name = "Unattached 1",
                        ContentSection = null,
                    },
                },
                new()
                {
                    Release = release,
                    ContentBlock = new HtmlBlock(),
                },
                new()
                {
                    Release = release,
                    ContentBlock = new DataBlock
                    {
                        Id = unattachedDataBlock2Id,
                        Name = "Unattached 2",
                        ContentSection = null,
                    },
                },
                new()
                {
                    Release = release,
                    ContentBlock = new DataBlock
                    {
                        Name = "Attached 2", // because has content section
                        ContentSection = new ContentSection(),
                    },
                },
                new()
                {
                    Release = new Release(),
                    ContentBlock = new DataBlock
                    {
                        Name = "Attached 3", // because different release
                        ContentSection = null,
                    }
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentBlocks.AddRangeAsync(releaseContentBlocks);
                await contentDbContext.KeyStatisticsDataBlock.AddRangeAsync(keyStat);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataBlockService(contentDbContext: contentDbContext);
                var result = await service.GetUnattachedDataBlocks(
                    release.Id);

                var unattachedDataBlocks = result.AssertRight();

                Assert.Equal(2, unattachedDataBlocks.Count);

                Assert.Equal(unattachedDataBlock1Id, unattachedDataBlocks[0].Id);
                Assert.Equal("Unattached 1", unattachedDataBlocks[0].Name);
                Assert.Equal(unattachedDataBlock2Id, unattachedDataBlocks[1].Id);
                Assert.Equal("Unattached 2", unattachedDataBlocks[1].Name);
            }
        }

        [Fact]
        public async Task GetUnattachedDataBlocks_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildDataBlockService(contentDbContext: contentDbContext);
                var result = await service.GetUnattachedDataBlocks(
                    Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        private static DataBlockService BuildDataBlockService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseContentBlockRepository? releaseContentBlockRepository = null,
            IUserService? userService = null,
            IBlobCacheService? cacheService = null,
            ICacheKeyService? cacheKeyService = null)
        {
            var service = new DataBlockService(
                contentDbContext ?? Mock.Of<ContentDbContext>(Strict),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                releaseContentBlockRepository ?? new ReleaseContentBlockRepository(contentDbContext),
                userService ?? AlwaysTrueUserService().Object,
                AdminMapper(),
                cacheService ?? Mock.Of<IBlobCacheService>(Strict),
                cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict)
            );

            return service;
        }
    }
}
