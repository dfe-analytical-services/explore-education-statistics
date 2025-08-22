#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FeaturedTableServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Get()
    {
        var dataBlockVersion = _fixture
            .DefaultDataBlockVersion()
            .Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(dataBlockVersion)
            .Generate();

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(ListOf(dataBlockParent.LatestDraftVersion!))
            .Generate();

        var featuredTable = _fixture
            .DefaultFeaturedTable()
            .WithDataBlock(dataBlockVersion.ContentBlock)
            .WithDataBlockParent(dataBlockParent)
            .WithReleaseVersion(releaseVersion)
            .WithCreated(DateTime.Now.AddDays(-3), createdById: Guid.NewGuid())
            .WithUpdated(DateTime.Now.AddDays(-2), updatedById: Guid.NewGuid())
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: dataBlockVersion.Id);

            var viewModel = result.AssertRight();

            Assert.Equal(featuredTable.Id, viewModel.Id);
            Assert.Equal(featuredTable.Name, viewModel.Name);
            Assert.Equal(featuredTable.Description, viewModel.Description);
            Assert.Equal(featuredTable.Order, viewModel.Order);
            Assert.Equal(dataBlockVersion.Id, viewModel.DataBlockId);
            Assert.Equal(dataBlockParent.Id, viewModel.DataBlockParentId);
        }
    }

    [Fact]
    public async Task Get_NoFeaturedTable()
    {
        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: Guid.NewGuid());

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
        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .Generate();

        var featuredTable = new FeaturedTable
        {
            DataBlock = _fixture
                .DefaultDataBlockVersion()
                .Generate()
                .ContentBlock
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Get(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: featuredTable.DataBlock.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task List()
    {
        var dataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .Generate())
            .GenerateList(2);

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(dataBlockParents
                .Select(p => p.LatestPublishedVersion!))
            .Generate();

        var featuredTable1 = _fixture
            .DefaultFeaturedTable()
            .WithDataBlock(dataBlockParents[0].LatestPublishedVersion!.ContentBlock)
            .WithDataBlockParent(dataBlockParents[0])
            .WithReleaseVersion(releaseVersion)
            .Generate();

        var featuredTable2 = _fixture
            .DefaultFeaturedTable()
            .WithDataBlock(dataBlockParents[1].LatestPublishedVersion!.ContentBlock)
            .WithDataBlockParent(dataBlockParents[1])
            .WithReleaseVersion(releaseVersion)
            .Generate();

        var unassociatedDataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(_fixture
                .DefaultDataBlockVersion()
                .Generate())
            .Generate();

        var unassociatedReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(ListOf(unassociatedDataBlockParent.LatestDraftVersion!))
            .Generate();

        var unassociatedFeaturedTable = new FeaturedTable
        {
            Name = "Unassociated featured table",
            DataBlock = unassociatedDataBlockParent.LatestPublishedVersion!.ContentBlock,
            ReleaseVersion = unassociatedReleaseVersion
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion, unassociatedReleaseVersion);
            context.FeaturedTables.AddRange(
                featuredTable1, featuredTable2, unassociatedFeaturedTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.List(releaseVersion.Id);

            var featuredTableList = result.AssertRight();

            Assert.Equal(2, featuredTableList.Count);

            Assert.Equal(featuredTable1.Id, featuredTableList[0].Id);
            Assert.Equal(featuredTable1.Name, featuredTableList[0].Name);
            Assert.Equal(featuredTable1.DataBlock.Id, featuredTableList[0].DataBlockId);
            Assert.Equal(featuredTable1.DataBlockParent.Id, featuredTableList[0].DataBlockParentId);

            Assert.Equal(featuredTable2.Id, featuredTableList[1].Id);
            Assert.Equal(featuredTable2.Name, featuredTableList[1].Name);
            Assert.Equal(featuredTable2.DataBlock.Id, featuredTableList[1].DataBlockId);
            Assert.Equal(featuredTable2.DataBlockParent.Id, featuredTableList[1].DataBlockParentId);
        }
    }

    [Fact]
    public async Task List_Order()
    {
        var releaseVersion = new ReleaseVersion();
        var featuredTable1 = new FeaturedTable
        {
            Name = "Featured table name 1",
            DataBlock = new DataBlock
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion,
            Order = 1,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Featured table name 2",
            DataBlock = new DataBlock
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion,
            Order = 2,
        };
        var featuredTable3 = new FeaturedTable
        {
            Name = "Featured table name 3",
            DataBlock = new DataBlock
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion,
            Order = 0,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable1, featuredTable2, featuredTable3);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.List(releaseVersion.Id);

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
        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(_fixture
                .DefaultDataBlockVersion()
                .Generate())
            .Generate();

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(ListOf(dataBlockParent.LatestDraftVersion!))
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Create(
                releaseVersion.Id,
                new FeaturedTableCreateRequest
                {
                    Name = "New featured table",
                    Description = "New featured table description",
                    DataBlockId = releaseVersion.DataBlockVersions[0].Id,
                });

            var viewModel = result.AssertRight();

            Assert.Equal("New featured table", viewModel.Name);
            Assert.Equal("New featured table description", viewModel.Description);
            Assert.Equal(0, viewModel.Order);
            Assert.Equal(releaseVersion.DataBlockVersions[0].Id, viewModel.DataBlockId);
            Assert.Equal(dataBlockParent.Id, viewModel.DataBlockParentId);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTable = context.FeaturedTables.Single();

            Assert.Equal("New featured table", featuredTable.Name);
            Assert.Equal("New featured table description", featuredTable.Description);
            Assert.Equal(0, featuredTable.Order);
            Assert.Equal(releaseVersion.DataBlockVersions[0].Id, featuredTable.DataBlockId);
            Assert.Equal(releaseVersion.Id, featuredTable.ReleaseVersionId);
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
        var releaseVersion = new ReleaseVersion();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Create(
                releaseVersion.Id,
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
        var releaseVersion = new ReleaseVersion();
        var dataBlock = new DataBlock
        {
            ReleaseVersion = releaseVersion
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            DataBlock = dataBlock,
            ReleaseVersion = releaseVersion,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Create(
                releaseVersion.Id,
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
            Assert.Equal(releaseVersion.Id, dbFeaturedTable.ReleaseVersionId);
        }
    }

    [Fact]
    public async Task Update()
    {
        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .Generate())
            .Generate();

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(ListOf(dataBlockParent.LatestPublishedVersion!))
            .Generate();

        var featuredTable = _fixture
            .DefaultFeaturedTable()
            .WithDataBlock(dataBlockParent.LatestPublishedVersion!.ContentBlock)
            .WithDataBlockParent(dataBlockParent)
            .WithReleaseVersion(releaseVersion)
            .WithOrder(65)
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Update(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: featuredTable.DataBlock.Id,
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
            Assert.Equal(dataBlockParent.Id, viewModel.DataBlockParentId);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbFeaturedTable = context.FeaturedTables.Single();

            Assert.Equal(featuredTable.Id, dbFeaturedTable.Id);
            Assert.Equal("Updated featured table name", dbFeaturedTable.Name);
            Assert.Equal("Updated featured table description", dbFeaturedTable.Description);
            Assert.Equal(featuredTable.DataBlock.Id, dbFeaturedTable.DataBlockId);
            Assert.Equal(releaseVersion.Id, dbFeaturedTable.ReleaseVersionId);
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
        var releaseVersion = new ReleaseVersion();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Update(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: Guid.NewGuid(),
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
        var releaseVersion = new ReleaseVersion();
        var dataBlock = new DataBlock
        {
            ReleaseVersion = releaseVersion
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = dataBlock,
            ReleaseVersion = new ReleaseVersion(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Update(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: dataBlock.Id,
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
        var releaseVersion = new ReleaseVersion();
        var dataBlock = new DataBlock
        {
            ReleaseVersion = releaseVersion
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = dataBlock,
            ReleaseVersion = releaseVersion,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Delete(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: dataBlock.Id);

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
        var releaseVersion = new ReleaseVersion();
        var dataBlock = new DataBlock
        {
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Delete(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_FeaturedTableNotAssociatedWithRelease()
    {
        var releaseVersion = new ReleaseVersion();
        var dataBlock = new DataBlock
        {
            ReleaseVersion = releaseVersion
        };
        var featuredTable = new FeaturedTable
        {
            Name = "Featured table name",
            Description = "Featured table description",
            Order = 65,
            DataBlock = new DataBlock(),
            ReleaseVersion = new ReleaseVersion(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Delete(
                releaseVersionId: releaseVersion.Id,
                dataBlockId: featuredTable.DataBlock.Id);

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
            Assert.Equal(featuredTable.ReleaseVersionId, dbFeaturedTable.ReleaseVersionId);
        }
    }

    [Fact]
    public async Task Reorder()
    {
        var dataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .Generate())
            .GenerateList(3);

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(dataBlockParents
                .Select(p => p.LatestPublishedVersion!))
            .Generate();

        var featuredTables = _fixture
            .DefaultFeaturedTable()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, s => s
                .SetDataBlock(dataBlockParents[0].LatestPublishedVersion!.ContentBlock)
                .SetDataBlockParent(dataBlockParents[0])
                .SetOrder(3))
            .ForIndex(1, s => s
                .SetDataBlock(dataBlockParents[1].LatestPublishedVersion!.ContentBlock)
                .SetDataBlockParent(dataBlockParents[1])
                .SetOrder(1))
            .ForIndex(2, s => s
                .SetDataBlock(dataBlockParents[2].LatestPublishedVersion!.ContentBlock)
                .SetDataBlockParent(dataBlockParents[2])
                .SetOrder(2))
            .GenerateList(3);

        var unassociatedDataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .Generate())
            .Generate();

        var unassociatedReleaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithDataBlockVersions(ListOf(unassociatedDataBlockParent.LatestPublishedVersion!))
            .Generate();

        var unassociatedFeaturedTable = _fixture
            .DefaultFeaturedTable()
            .WithReleaseVersion(unassociatedReleaseVersion)
            .WithOrder(4)
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion, unassociatedReleaseVersion);
            context.FeaturedTables.AddRange(featuredTables);
            context.FeaturedTables.AddRange(unassociatedFeaturedTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Reorder(
                releaseVersion.Id,
                new List<Guid>
                {
                    featuredTables[0].Id, featuredTables[1].Id, featuredTables[2].Id,
                });

            var featuredTableList = result.AssertRight();

            Assert.Equal(3, featuredTableList.Count);

            Assert.Equal(featuredTables[0].Id, featuredTableList[0].Id);
            Assert.Equal(featuredTables[0].Name, featuredTableList[0].Name);
            Assert.Equal(0, featuredTableList[0].Order);
            Assert.Equal(featuredTables[0].DataBlock.Id, featuredTableList[0].DataBlockId);
            Assert.Equal(featuredTables[0].DataBlockParent.Id, featuredTableList[0].DataBlockParentId);

            Assert.Equal(featuredTables[1].Id, featuredTableList[1].Id);
            Assert.Equal(featuredTables[1].Name, featuredTableList[1].Name);
            Assert.Equal(1, featuredTableList[1].Order);
            Assert.Equal(featuredTables[1].DataBlock.Id, featuredTableList[1].DataBlockId);
            Assert.Equal(featuredTables[1].DataBlockParent.Id, featuredTableList[1].DataBlockParentId);

            Assert.Equal(featuredTables[2].Id, featuredTableList[2].Id);
            Assert.Equal(featuredTables[2].Name, featuredTableList[2].Name);
            Assert.Equal(2, featuredTableList[2].Order);
            Assert.Equal(featuredTables[2].DataBlock.Id, featuredTableList[2].DataBlockId);
            Assert.Equal(featuredTables[2].DataBlockParent.Id, featuredTableList[2].DataBlockParentId);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbFeaturedTableList = context.FeaturedTables.ToList();
            Assert.Equal(4, dbFeaturedTableList.Count);

            var dbFeaturedTable1 = dbFeaturedTableList.Single(ft => ft.Id == featuredTables[0].Id);
            Assert.Equal(0, dbFeaturedTable1.Order);

            var dbFeaturedTable2 = dbFeaturedTableList.Single(ft => ft.Id == featuredTables[1].Id);
            Assert.Equal(1, dbFeaturedTable2.Order);

            var dbFeaturedTable3 = dbFeaturedTableList.Single(ft => ft.Id == featuredTables[2].Id);
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
        var releaseVersion = new ReleaseVersion();
        var dataBlock = new DataBlock();
        var featuredTable1 = new FeaturedTable
        {
            Name = "Featured table name 1",
            Order = 3,
            DataBlock = dataBlock,
            ReleaseVersion = releaseVersion,
        };
        var featuredTable2 = new FeaturedTable
        {
            Name = "Featured table name 2",
            Order = 1,
            DataBlock = dataBlock,
            ReleaseVersion = releaseVersion,
        };
        var featuredTable3 = new FeaturedTable
        {
            Name = "Featured table name 3",
            Order = 2,
            DataBlock = new DataBlock(),
            ReleaseVersion = releaseVersion,
        };
        var unassociatedFeaturedTable = new FeaturedTable
        {
            Name = "Unassociated featured table",
            Order = 4,
            DataBlock = new DataBlock(),
            ReleaseVersion = new ReleaseVersion(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.FeaturedTables.AddRange(
                featuredTable1, featuredTable2, featuredTable3, unassociatedFeaturedTable);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var featuredTableService = SetupService(context);

            var result = await featuredTableService.Reorder(
                releaseVersion.Id,
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
        IUserService? userService = null)
    {
        return new FeaturedTableService(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? MockUtils.AlwaysTrueUserService(_userId).Object,
            AdminMapper()
        );
    }
}
