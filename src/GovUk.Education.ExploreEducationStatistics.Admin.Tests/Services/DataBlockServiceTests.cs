using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

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

                Assert.True(result.IsRight);

                Assert.Equal(dataBlock.Heading, result.Right.Heading);
                Assert.Equal(dataBlock.Name, result.Right.Name);
                Assert.Equal(dataBlock.HighlightName, result.Right.HighlightName);
                Assert.Equal(dataBlock.HighlightDescription, result.Right.HighlightDescription);
                Assert.Equal(dataBlock.Source, result.Right.Source);
                Assert.Equal(dataBlock.Order, result.Right.Order);

                Assert.Equal(dataBlock.Query, result.Right.Query);
                Assert.Equal(dataBlock.Table, result.Right.Table);
                Assert.Equal(dataBlock.Charts, result.Right.Charts);
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
                Assert.Equal(dataBlock.Charts, viewModel.Charts);

                Assert.Single(viewModel.Charts);
                Assert.Equal(dataBlock.Heading, viewModel.Charts[0].Title);
            }
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(Guid.NewGuid());

                result.AssertNotFound();
            }
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

                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(dataBlock1.Heading, result.Right[0].Heading);
                Assert.Equal(dataBlock1.Name, result.Right[0].Name);
                Assert.Equal(dataBlock1.Created, result.Right[0].Created);
                Assert.Equal(dataBlock1.HighlightName, result.Right[0].HighlightName);
                Assert.Equal(dataBlock1.HighlightDescription, result.Right[0].HighlightDescription);
                Assert.Equal(dataBlock1.Source, result.Right[0].Source);
                Assert.Equal(1, result.Right[0].ChartsCount);
                Assert.Equal(dataBlock1.ContentSectionId, result.Right[0].ContentSectionId);

                Assert.Equal(dataBlock2.Heading, result.Right[1].Heading);
                Assert.Equal(dataBlock2.Name, result.Right[1].Name);
                Assert.Equal(dataBlock2.Created, result.Right[1].Created);
                Assert.Equal(dataBlock2.HighlightName, result.Right[1].HighlightName);
                Assert.Equal(dataBlock2.HighlightDescription, result.Right[1].HighlightDescription);
                Assert.Equal(dataBlock2.Source, result.Right[1].Source);
                Assert.Equal(0, result.Right[1].ChartsCount);
                Assert.Null(result.Right[1].ContentSectionId);
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

                Assert.True(result.IsRight);

                Assert.Single(result.Right);

                Assert.Equal(dataBlock1.Heading, result.Right[0].Heading);
                Assert.Equal(dataBlock1.Name, result.Right[0].Name);
                Assert.Equal(dataBlock1.Created, result.Right[0].Created);
                Assert.Equal(dataBlock1.HighlightName, result.Right[0].HighlightName);
                Assert.Equal(dataBlock1.HighlightDescription, result.Right[0].HighlightDescription);
                Assert.Equal(dataBlock1.Source, result.Right[0].Source);
                Assert.Equal(1, result.Right[0].ChartsCount);
                Assert.Equal(dataBlock1.ContentSectionId, result.Right[0].ContentSectionId);
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

                Assert.True(result.IsRight);
                Assert.Equal(release.Id, result.Right.ReleaseId);

                var dependentBlocks = result.Right.DependentDataBlocks;

                Assert.Single(dependentBlocks);

                Assert.Equal(dataBlock.Id, dependentBlocks[0].Id);
                Assert.Equal(dataBlock.Name, dependentBlocks[0].Name);
                Assert.Equal(dataBlock.ContentSection.Heading, dependentBlocks[0].ContentSectionHeading);

                Assert.Single(dependentBlocks[0].InfographicFilesInfo);

                Assert.Equal(file.Id, dependentBlocks[0].InfographicFilesInfo[0].Id);
                Assert.Equal(file.Filename, dependentBlocks[0].InfographicFilesInfo[0].Filename);
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
                var releaseFileService = new Mock<IReleaseFileService>();

                releaseFileService
                    .Setup(
                        s =>
                            s.Delete(release.Id, new List<Guid> {fileId}, false)
                    )
                    .ReturnsAsync(Unit.Instance);

                var service = BuildDataBlockService(context, releaseFileService: releaseFileService.Object);
                var result = await service.Delete(release.Id, dataBlock.Id);

                Assert.True(result.IsRight);

                MockUtils.VerifyAllMocks(releaseFileService);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                Assert.Empty(context.DataBlocks.ToList());
                Assert.Empty(context.ContentBlocks.ToList());
                Assert.Empty(context.ReleaseContentBlocks.ToList());
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

                Assert.Equal(createRequest.Query, viewModel.Query);
                Assert.Equal(createRequest.Table, viewModel.Table);
                Assert.Equal(createRequest.Charts, viewModel.Charts);

                Assert.Single(viewModel.Charts);
                Assert.NotEqual(createRequest.Heading, viewModel.Charts[0].Title);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var dataBlocks = context.DataBlocks.ToList();

                Assert.Single(dataBlocks);

                var dataBlock = dataBlocks[0];

                // Validate Created date is in the DB, even if not returned in result
                Assert.True(dataBlock.Created.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(dataBlock.Created.Value).Milliseconds, 0, 1500);

                Assert.Equal(createRequest.Heading, dataBlock.Heading);
                Assert.Equal(createRequest.Name, dataBlock.Name);
                Assert.Equal(createRequest.HighlightName, dataBlock.HighlightName);
                Assert.Equal(createRequest.HighlightDescription, dataBlock.HighlightDescription);
                Assert.Equal(createRequest.Source, dataBlock.Source);

                Assert.Equal(createRequest.Query, dataBlock.Query);
                Assert.Equal(createRequest.Table, dataBlock.Table);
                Assert.Equal(createRequest.Charts, dataBlock.Charts);

                var savedRelease = await context.Releases
                    .Include(r => r.ContentBlocks)
                    .FirstOrDefaultAsync(r => r.Id == release.Id);

                Assert.NotNull(savedRelease);
                Assert.Single(savedRelease.ContentBlocks);
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
                Assert.Equal(createRequest.Charts, dataBlock.Charts);

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
            var release = new Release();

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
                        Title = "Old chart",
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
                        Release = release,
                        ContentBlock = dataBlock
                    }
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
                var service = BuildDataBlockService(context);
                var result = await service.Update(dataBlock.Id, updateRequest);

                Assert.True(result.IsRight);

                Assert.Equal(dataBlock.Id, result.Right.Id);
                Assert.Equal(updateRequest.Heading, result.Right.Heading);
                Assert.Equal(updateRequest.Name, result.Right.Name);
                Assert.Equal(updateRequest.HighlightName, result.Right.HighlightName);
                Assert.Equal(updateRequest.HighlightDescription, result.Right.HighlightDescription);
                Assert.Equal(updateRequest.Source, result.Right.Source);
                Assert.Equal(dataBlock.Order, result.Right.Order);

                Assert.Equal(updateRequest.Query, result.Right.Query);
                Assert.Equal(updateRequest.Table, result.Right.Table);
                Assert.Equal(updateRequest.Charts, result.Right.Charts);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var updatedDataBlock = await context.DataBlocks.FindAsync(dataBlock.Id);

                Assert.Equal(updateRequest.Heading, updatedDataBlock.Heading);
                Assert.Equal(updateRequest.Name, updatedDataBlock.Name);
                Assert.Equal(updateRequest.HighlightName, updatedDataBlock.HighlightName);
                Assert.Equal(updateRequest.HighlightDescription, updatedDataBlock.HighlightDescription);
                Assert.Equal(updateRequest.Source, updatedDataBlock.Source);

                Assert.Equal(updateRequest.Query, updatedDataBlock.Query);
                Assert.Equal(updateRequest.Table, updatedDataBlock.Table);
                Assert.Equal(updateRequest.Charts, updatedDataBlock.Charts);
            }
        }

        [Fact]
        public async Task Update_HeadingUpdateAlsoChangesChartTitle()
        {
            var release = new Release();

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
                var service = BuildDataBlockService(context);
                var result = await service.Update(dataBlock.Id, updateRequest);

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

                Assert.Equal(updateRequest.Heading, updatedDataBlock.Heading);
                Assert.Equal(updateRequest.Charts, updatedDataBlock.Charts);

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
            var release = new Release();
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
                var releaseFileService = new Mock<IReleaseFileService>();

                releaseFileService
                    .Setup(s => s.Delete(release.Id, fileId, false))
                    .ReturnsAsync(Unit.Instance);

                var service = BuildDataBlockService(context, releaseFileService: releaseFileService.Object);
                var result = await service.Update(dataBlock.Id, updateRequest);

                Assert.True(result.IsRight);

                Assert.Equal(updateRequest.Charts, result.Right.Charts);

                MockUtils.VerifyAllMocks(releaseFileService);
            }
        }

        private static DataBlockService BuildDataBlockService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IReleaseFileService releaseFileService = null,
            IReleaseContentBlockRepository releaseContentBlockRepository = null,
            IUserService userService = null,
            IMapper mapper = null)
        {
            return new DataBlockService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                releaseContentBlockRepository ?? new ReleaseContentBlockRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                mapper ?? AdminMapper()
            );
        }
    }
}
