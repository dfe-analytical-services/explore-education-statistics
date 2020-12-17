using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(dataBlock.Id);

                Assert.True(result.IsRight);

                Assert.Equal(dataBlock.Heading, result.Right.Heading);
                Assert.Equal(dataBlock.Name, result.Right.Name);
                Assert.Equal(dataBlock.HighlightName, result.Right.HighlightName);
                Assert.Equal(dataBlock.Source, result.Right.Source);
                Assert.Equal(dataBlock.Order, result.Right.Order);

                Assert.Equal(dataBlock.Query, result.Right.Query);
                Assert.Equal(dataBlock.Table, result.Right.Table);
                Assert.Equal(dataBlock.Charts, result.Right.Charts);
            }
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(Guid.NewGuid());

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(dataBlock);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Get(dataBlock.Id);

                Assert.True(result.IsLeft);
                Assert.IsType<ForbidResult>(result.Left);
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
                Source = "Test source 1",
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
                Source = "Test source 2",
                Order = 7,
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
                    new VerticalBarChart
                    {
                        Title = "Test chart 2",
                        Height = 600,
                        Width = 400,
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(release.Id);

                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(dataBlock1.Heading, result.Right[0].Heading);
                Assert.Equal(dataBlock1.Name, result.Right[0].Name);
                Assert.Equal(dataBlock1.HighlightName, result.Right[0].HighlightName);
                Assert.Equal(dataBlock1.Source, result.Right[0].Source);
                Assert.Equal(dataBlock1.Order, result.Right[0].Order);

                Assert.Equal(dataBlock1.Query, result.Right[0].Query);
                Assert.Equal(dataBlock1.Table, result.Right[0].Table);
                Assert.Equal(dataBlock1.Charts, result.Right[0].Charts);

                Assert.Equal(dataBlock2.Heading, result.Right[1].Heading);
                Assert.Equal(dataBlock2.Name, result.Right[1].Name);
                Assert.Equal(dataBlock2.HighlightName, result.Right[1].HighlightName);
                Assert.Equal(dataBlock2.Source, result.Right[1].Source);
                Assert.Equal(dataBlock2.Order, result.Right[1].Order);

                Assert.Equal(dataBlock2.Query, result.Right[1].Query);
                Assert.Equal(dataBlock2.Table, result.Right[1].Table);
                Assert.Equal(dataBlock2.Charts, result.Right[1].Charts);
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
                Source = "Test source 1",
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.List(release.Id);

                Assert.True(result.IsRight);

                Assert.Single(result.Right);

                Assert.Equal(dataBlock1.Heading, result.Right[0].Heading);
                Assert.Equal(dataBlock1.Name, result.Right[0].Name);
                Assert.Equal(dataBlock1.HighlightName, result.Right[0].HighlightName);
                Assert.Equal(dataBlock1.Source, result.Right[0].Source);
                Assert.Equal(dataBlock1.Order, result.Right[0].Order);

                Assert.Equal(dataBlock1.Query, result.Right[0].Query);
                Assert.Equal(dataBlock1.Table, result.Right[0].Table);
                Assert.Equal(dataBlock1.Charts, result.Right[0].Charts);
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.GetDeletePlan(release.Id, Guid.NewGuid());

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.GetDeletePlan(Guid.NewGuid(), dataBlock.Id);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                Assert.Empty(context.DataBlocks.ToList());
                Assert.Empty(context.ContentBlocks.ToList());
                Assert.Empty(context.ReleaseContentBlocks.ToList());
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            var updateRequest = new UpdateDataBlockViewModel
            {
                Heading = "New heading",
                Name = "New name",
                HighlightName = "New highlight name",
                Source = "New source",
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
                        Title = "New chart",
                        Height = 600,
                        Width = 700,
                    }
                },
            };

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildDataBlockService(context);
                var result = await service.Update(dataBlock.Id, updateRequest);

                Assert.True(result.IsRight);

                Assert.Equal(dataBlock.Id, result.Right.Id);
                Assert.Equal(updateRequest.Heading, result.Right.Heading);
                Assert.Equal(updateRequest.Name, result.Right.Name);
                Assert.Equal(updateRequest.HighlightName, result.Right.HighlightName);
                Assert.Equal(updateRequest.Source, result.Right.Source);
                Assert.Equal(dataBlock.Order, result.Right.Order);

                Assert.Equal(updateRequest.Query, result.Right.Query);
                Assert.Equal(updateRequest.Table, result.Right.Table);
                Assert.Equal(updateRequest.Charts, result.Right.Charts);
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var updatedDataBlock = await context.DataBlocks.FindAsync(dataBlock.Id);

                Assert.Equal(updateRequest.Heading, updatedDataBlock.Heading);
                Assert.Equal(updateRequest.Name, updatedDataBlock.Name);
                Assert.Equal(updateRequest.HighlightName, updatedDataBlock.HighlightName);
                Assert.Equal(updateRequest.Source, updatedDataBlock.Source);

                Assert.Equal(updateRequest.Query, updatedDataBlock.Query);
                Assert.Equal(updateRequest.Table, updatedDataBlock.Table);
                Assert.Equal(updateRequest.Charts, updatedDataBlock.Charts);
            }
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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

            var updateRequest = new UpdateDataBlockViewModel
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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
            IMapper mapper = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null,
            IReleaseFileService releaseFileService = null)
        {
            return new DataBlockService(
                contentDbContext,
                mapper ?? MapperUtils.AdminMapper(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object
            );
        }
    }
}