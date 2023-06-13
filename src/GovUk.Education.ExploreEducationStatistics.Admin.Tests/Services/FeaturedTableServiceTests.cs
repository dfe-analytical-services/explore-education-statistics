#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FeaturedTableServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Get()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 2,
            DataBlock = dataBlock,
            Release = release,
            Created = new DateTime(2000, 1, 1),
            Updated = new DateTime(2000, 1, 2),
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                release.Id,
                dataBlock.Id);

            var viewModel = result.AssertRight();

            Assert.Equal(featuredTable.Id, viewModel.Id);
            Assert.Equal(featuredTable.Name, viewModel.Name);
            Assert.Equal(featuredTable.Description, viewModel.Description);
            Assert.Equal(featuredTable.Order, viewModel.Order);
            Assert.Equal(dataBlock.Id, viewModel.DataBlockId);
        }
    }

    [Fact]
    public async Task Get_NoFeaturedTable()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                release.Id,
                Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Get_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                Guid.NewGuid(),
                Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Get_ReleaseAndFeaturedTableNotAssociated()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            DataBlock = new DataBlock(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                release.Id,
                featuredTable.DataBlock.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task List()
    {
        var release = new Release();
        var featuredTable1 = new FeaturedTable
        {
            Name = "Featured table name 1",
            DataBlock = new DataBlock(),
            Release = release,
        };
        var releaseContentBlock1 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable1.DataBlock,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Featured table name 2",
            DataBlock = new DataBlock(),
            Release = release,
        };
        var releaseContentBlock2 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable2.DataBlock,
        };
        var unassociatedFeaturedTable = new FeaturedTable
        {
            Name = "Unassociated featured table",
            DataBlock = new DataBlock(),
            Release = new Release(),
        };
        var unassociatedReleaseContentBlock = new ReleaseContentBlock
        {
            Release = unassociatedFeaturedTable.Release,
            ContentBlock = unassociatedFeaturedTable.DataBlock,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(
                featuredTable1, featuredTable2, unassociatedFeaturedTable);
            await context.ReleaseContentBlocks.AddRangeAsync(
                releaseContentBlock1, releaseContentBlock2, unassociatedReleaseContentBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.List(release.Id);

            var featuredTableList = result.AssertRight();

            Assert.Equal(2, featuredTableList.Count);

            Assert.Equal(featuredTable1.Id, featuredTableList[0].Id);
            Assert.Equal(featuredTable1.Name, featuredTableList[0].Name);
            Assert.Equal(featuredTable1.DataBlock.Id, featuredTableList[0].DataBlockId);

            Assert.Equal(featuredTable2.Id, featuredTableList[1].Id);
            Assert.Equal(featuredTable2.Name, featuredTableList[1].Name);
            Assert.Equal(featuredTable2.DataBlock.Id, featuredTableList[1].DataBlockId);
        }
    }

    [Fact]
    public async Task List_Order()
    {
        var release = new Release();
        var featuredTable1 = new FeaturedTable
        {
            Name = "Featured table name 1",
            DataBlock = new DataBlock(),
            Release = release,
            Order = 1,
        };
        var releaseContentBlock1 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable1.DataBlock,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Featured table name 2",
            DataBlock = new DataBlock(),
            Release = release,
            Order = 2,
        };
        var releaseContentBlock2 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable2.DataBlock,
        };
        var featuredTable3 = new FeaturedTable
        {
            Name = "Unassociated featured table 3",
            DataBlock = new DataBlock(),
            Release = release,
            Order = 0,
        };
        var releaseContentBlock3 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable3.DataBlock,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(
                featuredTable1, featuredTable2, featuredTable3);
            await context.ReleaseContentBlocks.AddRangeAsync(
                releaseContentBlock1, releaseContentBlock2, releaseContentBlock3);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock> { featuredTable1.DataBlock, featuredTable2.DataBlock, featuredTable3.DataBlock, });

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.List(release.Id);

            var featuredTableList = result.AssertRight();

            Assert.Equal(3, featuredTableList.Count);

            Assert.Equal(featuredTable3.Id, featuredTableList[0].Id);
            Assert.Equal(0, featuredTableList[0].Order);
            Assert.Equal(featuredTable1.Id, featuredTableList[1].Id);
            Assert.Equal(1, featuredTableList[1].Order);
            Assert.Equal(featuredTable2.Id, featuredTableList[2].Id);
            Assert.Equal(2, featuredTableList[2].Order);
        }
    }

    [Fact]
    public async Task List_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.List(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Create()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Create(
                release.Id,
                new FeaturedTableCreateRequest
                {
                    Name = "New featured table",
                    Description = "New featured table description",
                    DataBlockId = dataBlock.Id,
                });

            var viewModel = result.AssertRight();

            Assert.Equal("New featured table", viewModel.Name);
            Assert.Equal("New featured table description", viewModel.Description);
            Assert.Equal(0, viewModel.Order);
            Assert.Equal(dataBlock.Id, viewModel.DataBlockId);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTable = context.FeaturedTables.Single();

            Assert.Equal("New featured table", featuredTable.Name);
            Assert.Equal("New featured table description", featuredTable.Description);
            Assert.Equal(0, featuredTable.Order);
            Assert.Equal(dataBlock.Id, featuredTable.DataBlockId);
            Assert.Equal(release.Id, featuredTable.ReleaseId);
            Assert.Equal(_userId, featuredTable.CreatedById);
            Assert.Null(featuredTable.UpdatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(featuredTable.Created).Milliseconds, 0, 1500);
            Assert.Null(featuredTable.Updated);
        }
    }

    [Fact]
    public async Task Create_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(
                context);

            var result = await featuredTableService.Create(
                Guid.NewGuid(),
                new FeaturedTableCreateRequest
                {
                    Name = "New featured table",
                    Description = "New featured table description",
                    DataBlockId = Guid.NewGuid(),
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Create_NoDataBlock()
    {
        var release = new Release();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Create(
                release.Id,
                new FeaturedTableCreateRequest
                {
                    Name = "New featured table",
                    Description = "New featured table description",
                    DataBlockId = Guid.NewGuid(),
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Create_DataBlockAlreadyHasFeaturedTable()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            DataBlock = dataBlock,
            Release = release,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Create(
                release.Id,
                new FeaturedTableCreateRequest
                {
                    Name = "New featured table",
                    Description = "New featured table description",
                    DataBlockId = dataBlock.Id,
                });

            result.AssertBadRequest(DataBlockAlreadyHasFeaturedTable);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbFeaturedTable = context.FeaturedTables.Single();

            Assert.Equal("Featured table name", dbFeaturedTable.Name);
            Assert.Equal("Featured table description", dbFeaturedTable.Description);
            Assert.Equal(dataBlock.Id, dbFeaturedTable.DataBlockId);
            Assert.Equal(release.Id, dbFeaturedTable.ReleaseId);
        }
    }

    [Fact]
    public async Task Update()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = dataBlock,
            Release = release,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Update(
                release.Id,
                dataBlock.Id,
                new FeaturedTableUpdateRequest
                {
                    Name = "Updated featured table name",
                    Description = "Updated featured table description",
                });

            var viewModel = result.AssertRight();

            Assert.Equal(featuredTable.Id, viewModel.Id);
            Assert.Equal("Updated featured table name", viewModel.Name);
            Assert.Equal("Updated featured table description", viewModel.Description);
            Assert.Equal(featuredTable.Order, viewModel.Order);
            Assert.Equal(dataBlock.Id, viewModel.DataBlockId);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbFeaturedTable = context.FeaturedTables.Single();

            Assert.Equal(featuredTable.Id, dbFeaturedTable.Id);
            Assert.Equal("Updated featured table name", dbFeaturedTable.Name);
            Assert.Equal("Updated featured table description", dbFeaturedTable.Description);
            Assert.Equal(dataBlock.Id, dbFeaturedTable.DataBlockId);
            Assert.Equal(release.Id, dbFeaturedTable.ReleaseId);
        }
    }

    [Fact]
    public async Task Update_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(
                context);

            var result = await featuredTableService.Update(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new FeaturedTableUpdateRequest
                {
                    Name = "Updated featured table name",
                    Description = "Updated featured table description",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Update_NoDataBlock()
    {
        var release = new Release();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Update(
                release.Id,
                Guid.NewGuid(),
                new FeaturedTableUpdateRequest
                {
                    Name = "Updated featured table name",
                    Description = "Updated featured table description",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Update_FeaturedTableNotAssociatedWithRelease()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = dataBlock,
            Release = new Release(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Update(
                release.Id,
                dataBlock.Id,
                new FeaturedTableUpdateRequest
                {
                    Name = "Updated featured table name",
                    Description = "Updated featured table description",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = dataBlock,
            Release = release,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Delete(
                release.Id,
                dataBlock.Id);

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableList = context.FeaturedTables.ToList();
            Assert.Empty(featuredTableList);
        }
    }

    [Fact]
    public async Task Delete_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(
                context);

            var result = await featuredTableService.Delete(
                Guid.NewGuid(),
                Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_NoFeaturedTable()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Delete(
                release.Id,
                Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_FeaturedTableNotAssociatedWithRelease()
    {
        var dataBlock = new DataBlock();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = dataBlock,
                },
            },
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = new DataBlock(),
            Release = new Release(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>());

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Delete(
                release.Id,
                featuredTable.DataBlock.Id);

            result.AssertNotFound();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableList = context.FeaturedTables.ToList();
            var dbFeaturedTable = Assert.Single(featuredTableList);
            Assert.Equal(featuredTable.Id, dbFeaturedTable.Id);
            Assert.Equal(featuredTable.Name, dbFeaturedTable.Name);
            Assert.Equal(featuredTable.Description, dbFeaturedTable.Description);
            Assert.Equal(featuredTable.Order, dbFeaturedTable.Order);
            Assert.Equal(featuredTable.DataBlockId, dbFeaturedTable.DataBlockId);
            Assert.Equal(featuredTable.ReleaseId, dbFeaturedTable.ReleaseId);
        }
    }

    [Fact]
    public async Task Reorder()
    {
        var release = new Release();
        var featuredTable1 = new FeaturedTable
        {
            Name = "Featured table name 1",
            Order = 3,
            DataBlock = new DataBlock(),
            Release = release,
        };
        var releaseContentBlock1 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable1.DataBlock,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Featured table name 2",
            Order = 1,
            DataBlock = new DataBlock(),
            Release = release,
        };
        var releaseContentBlock2 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable2.DataBlock,
        };
        var featuredTable3 = new FeaturedTable
        {
            Name = "Featured table name 3",
            Order = 2,
            DataBlock = new DataBlock(),
            Release = release,
        };
        var releaseContentBlock3 = new ReleaseContentBlock
        {
            Release = release,
            ContentBlock = featuredTable3.DataBlock,
        };
        var unassociatedFeaturedTable = new FeaturedTable
        {
            Name = "Unassociated featured table",
            Order = 4,
            DataBlock = new DataBlock(),
            Release = new Release(),
        };
        var unassociatedReleaseContentBlock = new ReleaseContentBlock
        {
            Release = unassociatedFeaturedTable.Release,
            ContentBlock = unassociatedFeaturedTable.DataBlock,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(
                featuredTable1, featuredTable2, featuredTable3, unassociatedFeaturedTable);
            await context.ReleaseContentBlocks.AddRangeAsync(
                releaseContentBlock1, releaseContentBlock2,
                releaseContentBlock3, unassociatedReleaseContentBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Reorder(
                release.Id,
                new List<Guid>
                {
                    featuredTable1.Id, featuredTable2.Id, featuredTable3.Id,
                });

            var featuredTableList = result.AssertRight();

            Assert.Equal(3, featuredTableList.Count);

            Assert.Equal(featuredTable1.Id, featuredTableList[0].Id);
            Assert.Equal(featuredTable1.Name, featuredTableList[0].Name);
            Assert.Equal(0, featuredTableList[0].Order);
            Assert.Equal(featuredTable1.DataBlock.Id, featuredTableList[0].DataBlockId);

            Assert.Equal(featuredTable2.Id, featuredTableList[1].Id);
            Assert.Equal(featuredTable2.Name, featuredTableList[1].Name);
            Assert.Equal(1, featuredTableList[1].Order);
            Assert.Equal(featuredTable2.DataBlock.Id, featuredTableList[1].DataBlockId);

            Assert.Equal(featuredTable3.Id, featuredTableList[2].Id);
            Assert.Equal(featuredTable3.Name, featuredTableList[2].Name);
            Assert.Equal(2, featuredTableList[2].Order);
            Assert.Equal(featuredTable3.DataBlock.Id, featuredTableList[2].DataBlockId);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbFeaturedTableList = context.FeaturedTables.ToList();
            Assert.Equal(4, dbFeaturedTableList.Count);

            var dbFeaturedTable1 = dbFeaturedTableList.Single(ft => ft.Id == featuredTable1.Id);
            Assert.Equal(0, dbFeaturedTable1.Order);

            var dbFeaturedTable2 = dbFeaturedTableList.Single(ft => ft.Id == featuredTable2.Id);
            Assert.Equal(1, dbFeaturedTable2.Order);

            var dbFeaturedTable3 = dbFeaturedTableList.Single(ft => ft.Id == featuredTable3.Id);
            Assert.Equal(2, dbFeaturedTable3.Order);

            var dbUnassociatedFeaturedTable = dbFeaturedTableList.Single(ft => ft.Id == unassociatedFeaturedTable.Id);
            Assert.Equal(4, dbUnassociatedFeaturedTable.Order);
        }
    }

    [Fact]
    public async Task Reorder_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(
                context);

            var result = await featuredTableService.Reorder(
                Guid.NewGuid(),
                new List<Guid>
                {
                    Guid.NewGuid(), Guid.NewGuid(),
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Reorder_ProvidedIdsDifferFromReleaseFeaturedTableIds()
    {
        var release = new Release();
        var dataBlock = new DataBlock();
        var featuredTable1 = new FeaturedTable
        {
            Name = "Featured table name 1",
            Order = 3,
            DataBlock = dataBlock,
            Release = release,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Featured table name 2",
            Order = 1,
            DataBlock = dataBlock,
            Release = release,
        };
        var featuredTable3 = new FeaturedTable
        {
            Name = "Featured table name 3",
            Order = 2,
            DataBlock = new DataBlock(),
            Release = release,
        };
        var unassociatedFeaturedTable = new FeaturedTable
        {
            Name = "Unassociated featured table",
            Order = 4,
            DataBlock = new DataBlock(),
            Release = new Release(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.FeaturedTables.AddRangeAsync(
                featuredTable1, featuredTable2, featuredTable3, unassociatedFeaturedTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.ListDataBlocks(release.Id))
                .ReturnsAsync(new List<DataBlock>
                {
                    featuredTable1.DataBlock, featuredTable2.DataBlock, featuredTable3.DataBlock
                });

            var featuredTableService = SetupService(
                context,
                dataBlockService: dataBlockService.Object);

            var result = await featuredTableService.Reorder(
                release.Id,
                new List<Guid>
                {
                    featuredTable1.Id, featuredTable2.Id, unassociatedFeaturedTable.Id,
                });

            result.AssertBadRequest(
                ProvidedFeaturedTableIdsDifferFromReleaseFeaturedTableIds);
        }
    }

    private FeaturedTableService SetupService(
        ContentDbContext contentDbContext,
        IDataBlockService? dataBlockService = null,
        IUserService? userService = null)
    {
        return new FeaturedTableService(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            dataBlockService ?? Mock.Of<IDataBlockService>(),
            userService ?? MockUtils.AlwaysTrueUserService(_userId).Object,
            AdminMapper()
        );
    }
}
