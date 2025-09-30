#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class KeyStatisticServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateKeyStatisticDataBlock()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockParents.AddRange(dataBlockParent);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService
                .Setup(s => s.GetUnattachedDataBlocks(releaseVersion.Id))
                .ReturnsAsync(ListOf(new DataBlockViewModel { Id = dataBlockVersion.Id }));

            dataBlockService
                .Setup(s =>
                    s.IsUnattachedDataBlock(
                        releaseVersion.Id,
                        It.Is<DataBlockVersion>(db => db.Id == dataBlockVersion.Id)
                    )
                )
                .ReturnsAsync(true);

            var keyStatisticService = SetupKeyStatisticService(
                context,
                dataBlockService: dataBlockService.Object
            );

            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                releaseVersion.Id,
                new KeyStatisticDataBlockCreateRequest
                {
                    DataBlockId = dataBlockVersion.Id,
                    Trend = "trend",
                    GuidanceTitle = "guidanceTitle",
                    GuidanceText = "guidanceText",
                }
            );

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlockVersion.Id, viewModel.DataBlockId);

            Assert.Equal("trend", viewModel.Trend);
            Assert.Equal("guidanceTitle", viewModel.GuidanceTitle);
            Assert.Equal("guidanceText", viewModel.GuidanceText);
            Assert.Equal(0, viewModel.Order);
            Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Created).Milliseconds, 0, 1500);
            Assert.Null(viewModel.Updated);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            var keyStat = Assert.Single(keyStatistics);
            var keyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(keyStat);

            Assert.Equal(dataBlockVersion.Id, keyStatDataBlock.DataBlockId);
            Assert.Equal(dataBlockParent.Id, keyStatDataBlock.DataBlockParentId);

            Assert.Equal(releaseVersion.Id, keyStatDataBlock.ReleaseVersionId);
            Assert.Equal("trend", keyStatDataBlock.Trend);
            Assert.Equal("guidanceTitle", keyStatDataBlock.GuidanceTitle);
            Assert.Equal("guidanceText", keyStatDataBlock.GuidanceText);
            Assert.Equal(0, keyStatDataBlock.Order);
            Assert.Equal(_userId, keyStatDataBlock.CreatedById);
            Assert.Null(keyStatDataBlock.UpdatedById);
            Assert.InRange(
                DateTime.UtcNow.Subtract(keyStatDataBlock.Created).Milliseconds,
                0,
                1500
            );
            Assert.Null(keyStatDataBlock.Updated);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_Order()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        releaseVersion.KeyStatistics = new List<KeyStatistic>
        {
            new KeyStatisticText { Order = 0 },
            new KeyStatisticText { Order = 1 },
            new KeyStatisticDataBlock { Order = 2 },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockParents.AddRange(dataBlockParent);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);

            dataBlockService
                .Setup(s => s.GetUnattachedDataBlocks(releaseVersion.Id))
                .ReturnsAsync(ListOf(new DataBlockViewModel { Id = dataBlockVersion.Id }));

            dataBlockService
                .Setup(s =>
                    s.IsUnattachedDataBlock(
                        releaseVersion.Id,
                        It.Is<DataBlockVersion>(db => db.Id == dataBlockVersion.Id)
                    )
                )
                .ReturnsAsync(true);

            var keyStatisticService = SetupKeyStatisticService(
                context,
                dataBlockService: dataBlockService.Object
            );

            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                releaseVersion.Id,
                new KeyStatisticDataBlockCreateRequest { DataBlockId = dataBlockVersion.Id }
            );

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlockVersion.Id, viewModel.DataBlockId);

            Assert.Null(viewModel.Trend);
            Assert.Null(viewModel.GuidanceTitle);
            Assert.Null(viewModel.GuidanceText);
            Assert.Equal(3, viewModel.Order);
            Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Created).Milliseconds, 0, 1500);
            Assert.Null(viewModel.Updated);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.OrderBy(ks => ks.Order).ToList();
            Assert.Equal(4, keyStatistics.Count);

            Assert.Equal(0, keyStatistics[0].Order);
            Assert.Equal(1, keyStatistics[1].Order);
            Assert.Equal(2, keyStatistics[2].Order);

            var keyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(keyStatistics[3]);

            Assert.Equal(dataBlockVersion.Id, keyStatDataBlock.DataBlockId);

            Assert.Equal(releaseVersion.Id, keyStatDataBlock.ReleaseVersionId);
            Assert.Null(keyStatDataBlock.Trend);
            Assert.Null(keyStatDataBlock.GuidanceTitle);
            Assert.Null(keyStatDataBlock.GuidanceText);
            Assert.Equal(3, keyStatDataBlock.Order);
            Assert.Equal(_userId, keyStatDataBlock.CreatedById);
            Assert.Null(keyStatDataBlock.UpdatedById);
            Assert.InRange(
                DateTime.UtcNow.Subtract(keyStatDataBlock.Created).Milliseconds,
                0,
                1500
            );
            Assert.Null(keyStatDataBlock.Updated);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_NoRelease()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockVersions.AddRange(dataBlockVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                Guid.NewGuid(),
                new KeyStatisticDataBlockCreateRequest { DataBlockId = dataBlockVersion.Id }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_NoDataBlock()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockVersions.AddRange(dataBlockVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                releaseVersion.Id,
                new KeyStatisticDataBlockCreateRequest { DataBlockId = Guid.NewGuid() }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_DataBlockAttachedToContent()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockParents.AddRange(dataBlockParent);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService
                .Setup(s => s.GetUnattachedDataBlocks(releaseVersion.Id))
                .ReturnsAsync(
                    new Either<ActionResult, List<DataBlockViewModel>>(
                        new List<DataBlockViewModel> { new(), new() }
                    )
                );

            dataBlockService
                .Setup(s =>
                    s.IsUnattachedDataBlock(
                        releaseVersion.Id,
                        It.Is<DataBlockVersion>(db => db.Id == dataBlockVersion.Id)
                    )
                )
                .ReturnsAsync(false);

            var keyStatisticService = SetupKeyStatisticService(
                context,
                dataBlockService: dataBlockService.Object
            );

            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                releaseVersion.Id,
                new KeyStatisticDataBlockCreateRequest { DataBlockId = dataBlockVersion.Id }
            );

            result.AssertBadRequest(DataBlockShouldBeUnattached);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticText()
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
            var keyStatisticService = SetupKeyStatisticService(context);

            var result = await keyStatisticService.CreateKeyStatisticText(
                releaseVersion.Id,
                new KeyStatisticTextCreateRequest
                {
                    Title = "title",
                    Statistic = "Over 9000!",
                    Trend = "trend",
                    GuidanceTitle = "guidanceTitle",
                    GuidanceText = "guidanceText",
                }
            );

            var viewModel = result.AssertRight();

            Assert.Equal("title", viewModel.Title);
            Assert.Equal("Over 9000!", viewModel.Statistic);
            Assert.Equal("trend", viewModel.Trend);
            Assert.Equal("guidanceTitle", viewModel.GuidanceTitle);
            Assert.Equal("guidanceText", viewModel.GuidanceText);
            Assert.Equal(0, viewModel.Order);
            Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Created).Milliseconds, 0, 1500);
            Assert.Null(viewModel.Updated);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            var keyStat = Assert.Single(keyStatistics);
            var keyStatText = Assert.IsType<KeyStatisticText>(keyStat);

            Assert.Equal("title", keyStatText.Title);
            Assert.Equal("Over 9000!", keyStatText.Statistic);
            Assert.Equal(releaseVersion.Id, keyStatText.ReleaseVersionId);
            Assert.Equal("trend", keyStatText.Trend);
            Assert.Equal("guidanceTitle", keyStatText.GuidanceTitle);
            Assert.Equal("guidanceText", keyStatText.GuidanceText);
            Assert.Equal(0, keyStatText.Order);
            Assert.Equal(_userId, keyStatText.CreatedById);
            Assert.Null(keyStatText.UpdatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(keyStatText.Created).Milliseconds, 0, 1500);
            Assert.Null(keyStatText.Updated);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticText_NoRelease()
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
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.CreateKeyStatisticText(
                Guid.NewGuid(),
                new KeyStatisticTextCreateRequest { Title = "title", Statistic = "Over 9000!" }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateKeyStatisticText_Order()
    {
        var releaseVersion = new ReleaseVersion
        {
            KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticText { Order = 0 },
                new KeyStatisticText { Order = 1 },
                new KeyStatisticDataBlock { Order = 2 },
            },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);

            var result = await keyStatisticService.CreateKeyStatisticText(
                releaseVersion.Id,
                new KeyStatisticTextCreateRequest { Title = "title", Statistic = "Over 9000!" }
            );

            var viewModel = result.AssertRight();

            Assert.Equal("title", viewModel.Title);
            Assert.Equal(3, viewModel.Order);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.OrderBy(ks => ks.Order).ToList();
            Assert.Equal(4, keyStatistics.Count);

            Assert.Equal(0, keyStatistics[0].Order);
            Assert.Equal(1, keyStatistics[1].Order);
            Assert.Equal(2, keyStatistics[2].Order);

            var keyStatText = Assert.IsType<KeyStatisticText>(keyStatistics[3]);
            Assert.Equal("title", keyStatText.Title);
            Assert.Equal(3, keyStatText.Order);
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersion.Id,
            DataBlockId = dataBlockVersion.Id,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        releaseVersion.KeyStatistics = ListOf<KeyStatistic>(keyStatisticDataBlock);

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockParents.AddRange(dataBlockParent);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                releaseVersion.Id,
                keyStatisticDataBlock.Id,
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlockVersion.Id, viewModel.DataBlockId);

            Assert.Equal("new trend", viewModel.Trend);
            Assert.Equal("new guidanceTitle", viewModel.GuidanceTitle);
            Assert.Equal("new guidanceText", viewModel.GuidanceText);
            Assert.Equal(0, viewModel.Order);
            Assert.Equal(new DateTime(2000, 1, 1), viewModel.Created);
            viewModel.Updated.AssertUtcNow();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            var keyStat = Assert.Single(keyStatistics);
            var keyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(keyStat);

            Assert.Equal(dataBlockVersion.Id, keyStatDataBlock.DataBlockId);

            Assert.Equal(releaseVersion.Id, keyStatDataBlock.ReleaseVersionId);
            Assert.Equal("new trend", keyStatDataBlock.Trend);
            Assert.Equal("new guidanceTitle", keyStatDataBlock.GuidanceTitle);
            Assert.Equal("new guidanceText", keyStatDataBlock.GuidanceText);
            Assert.Equal(0, keyStatDataBlock.Order);
            Assert.Equal(new DateTime(2000, 1, 1), keyStatDataBlock.Created);
            Assert.NotNull(keyStatDataBlock.Updated);
            Assert.Equal(keyStatisticDataBlock.CreatedById, keyStatDataBlock.CreatedById);
            Assert.Equal(_userId, keyStatDataBlock.UpdatedById);
            keyStatDataBlock.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock_NoRelease()
    {
        var releaseVersionId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersionId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            KeyStatistics = new List<KeyStatistic> { keyStatisticDataBlock },
        };
        var dataBlock = new DataBlock { ReleaseVersion = releaseVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                Guid.NewGuid(),
                keyStatisticDataBlock.Id,
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock_NoKeyStatisticDataBlock()
    {
        var releaseVersionId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersionId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            KeyStatistics = new List<KeyStatistic> { keyStatisticDataBlock },
        };
        var dataBlock = new DataBlock { ReleaseVersion = releaseVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                releaseVersion.Id,
                Guid.NewGuid(),
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock_KeyStatWrongType()
    {
        var releaseVersionId = Guid.NewGuid();
        var keyStatisticText = new KeyStatisticText
        {
            ReleaseVersionId = releaseVersionId,
            Title = "title",
            Statistic = "Over 9000",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            KeyStatistics = new List<KeyStatistic> { keyStatisticText },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                releaseVersion.Id,
                keyStatisticText.Id,
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText()
    {
        var releaseVersionId = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            ReleaseVersionId = releaseVersionId,
            Title = "title",
            Statistic = "statistic",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            KeyStatistics = new List<KeyStatistic> { keyStatisticText },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                releaseVersion.Id,
                keyStatisticText.Id,
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            var viewModel = result.AssertRight();

            Assert.Equal("new title", viewModel.Title);
            Assert.Equal("new statistic", viewModel.Statistic);
            Assert.Equal("new trend", viewModel.Trend);
            Assert.Equal("new guidanceTitle", viewModel.GuidanceTitle);
            Assert.Equal("new guidanceText", viewModel.GuidanceText);
            Assert.Equal(0, viewModel.Order);
            Assert.Equal(new DateTime(2000, 1, 1), viewModel.Created);
            viewModel.Updated.AssertUtcNow();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            var keyStat = Assert.Single(keyStatistics);
            var keyStatText = Assert.IsType<KeyStatisticText>(keyStat);

            Assert.Equal(releaseVersion.Id, keyStatText.ReleaseVersionId);
            Assert.Equal("new title", keyStatText.Title);
            Assert.Equal("new statistic", keyStatText.Statistic);
            Assert.Equal("new trend", keyStatText.Trend);
            Assert.Equal("new guidanceTitle", keyStatText.GuidanceTitle);
            Assert.Equal("new guidanceText", keyStatText.GuidanceText);
            Assert.Equal(0, keyStatText.Order);
            Assert.Equal(new DateTime(2000, 1, 1), keyStatText.Created);
            Assert.Equal(keyStatisticText.CreatedById, keyStatText.CreatedById);
            Assert.Equal(_userId, keyStatText.UpdatedById);
            keyStatText.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText_NoRelease()
    {
        var rele = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            ReleaseVersionId = rele,
            Title = "title",
            Statistic = "statistic",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = rele,
            KeyStatistics = new List<KeyStatistic> { keyStatisticText },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                Guid.NewGuid(),
                keyStatisticText.Id,
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText_NoKeyStatisticText()
    {
        var releaseVersionId = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            ReleaseVersionId = releaseVersionId,
            Title = "title",
            Statistic = "statistic",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            KeyStatistics = new List<KeyStatistic> { keyStatisticText },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                releaseVersion.Id,
                Guid.NewGuid(),
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText_KeyStatWrongType()
    {
        var releaseVersionId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersionId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            KeyStatistics = new List<KeyStatistic> { keyStatisticDataBlock },
        };
        var dataBlock = new DataBlock { ReleaseVersion = releaseVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                releaseVersion.Id,
                keyStatisticDataBlock.Id,
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_KeyStatisticDataBlock()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersion.Id,
            DataBlockId = dataBlockVersion.Id,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        releaseVersion.KeyStatistics = ListOf<KeyStatistic>(
            new KeyStatisticText(),
            keyStatisticDataBlock,
            new KeyStatisticText()
        );

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockParents.AddRange(dataBlockParent);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(
                releaseVersion.Id,
                keyStatisticDataBlock.Id
            );

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            Assert.Equal(2, keyStatistics.Count);
            Assert.Null(keyStatistics.Find(ks => ks.Id == keyStatisticDataBlock.Id));

            var dataBlockList = context.DataBlocks.ToList();
            var retrievedDataBlock = Assert.Single(dataBlockList);
            Assert.Equal(dataBlockVersion.Id, retrievedDataBlock.Id);
        }
    }

    [Fact]
    public async Task Delete_KeyStatisticText()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var keyStatisticText = new KeyStatisticText
        {
            Title = "title",
            Statistic = "Over 9000",
            ReleaseVersionId = releaseVersion.Id,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        releaseVersion.KeyStatistics = ListOf<KeyStatistic>(
            new KeyStatisticText(),
            keyStatisticText,
            new KeyStatisticDataBlock()
        );

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(releaseVersion.Id, keyStatisticText.Id);

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            Assert.Equal(2, keyStatistics.Count);
            Assert.Null(keyStatistics.Find(ks => ks.Id == keyStatisticText.Id));
        }
    }

    [Fact]
    public async Task Delete_NoRelease()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersion.Id,
            DataBlockId = dataBlockVersion.Id,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockVersions.AddRange(dataBlockVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(Guid.NewGuid(), keyStatisticDataBlock.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_NoKeyStatistic()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseVersionId = releaseVersion.Id,
            DataBlockId = dataBlockVersion.Id,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        releaseVersion.KeyStatistics = ListOf<KeyStatistic>(keyStatisticDataBlock);

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.DataBlockParents.AddRange(dataBlockParent);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(releaseVersion.Id, Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Reorder()
    {
        var dataBlockId = Guid.NewGuid();

        var keyStat0 = new KeyStatisticText // keyStats will be reordered to match variable name
        {
            Order = 1,
            CreatedById = Guid.NewGuid(),
            UpdatedById = null,
        };
        var keyStat1 = new KeyStatisticDataBlock
        {
            Order = 3,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };
        var keyStat2 = new KeyStatisticText
        {
            Order = 2,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };
        var keyStat3 = new KeyStatisticDataBlock
        {
            Order = 0,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        var releaseVersion = new ReleaseVersion
        {
            KeyStatistics = new List<KeyStatistic> { keyStat0, keyStat1, keyStat2, keyStat3 },
        };
        var dataBlock = new DataBlock { Id = dataBlockId, ReleaseVersion = releaseVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Reorder(
                releaseVersion.Id,
                new List<Guid> { keyStat0.Id, keyStat1.Id, keyStat2.Id, keyStat3.Id }
            );

            var viewModelList = result.AssertRight();

            Assert.Equal(4, viewModelList.Count);

            Assert.Equal(keyStat0.Id, viewModelList[0].Id);
            Assert.Equal(0, viewModelList[0].Order);
            Assert.Equal(keyStat1.Id, viewModelList[1].Id);
            Assert.Equal(1, viewModelList[1].Order);
            Assert.Equal(keyStat2.Id, viewModelList[2].Id);
            Assert.Equal(2, viewModelList[2].Order);
            Assert.Equal(keyStat3.Id, viewModelList[3].Id);
            Assert.Equal(3, viewModelList[3].Order);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.OrderBy(ks => ks.Order).ToList();
            Assert.Equal(4, keyStatistics.Count);

            Assert.Equal(keyStat0.Id, keyStatistics[0].Id);
            Assert.Equal(0, keyStatistics[0].Order);
            Assert.Equal(keyStat0.CreatedById, keyStatistics[0].CreatedById);
            Assert.Equal(_userId, keyStatistics[0].UpdatedById);

            Assert.Equal(keyStat1.Id, keyStatistics[1].Id);
            Assert.Equal(1, keyStatistics[1].Order);
            Assert.Equal(keyStat1.CreatedById, keyStatistics[1].CreatedById);
            Assert.Equal(_userId, keyStatistics[1].UpdatedById);

            Assert.Equal(keyStat2.Id, keyStatistics[2].Id);
            Assert.Equal(2, keyStatistics[2].Order);
            Assert.Equal(keyStat2.CreatedById, keyStatistics[2].CreatedById);
            Assert.Equal(_userId, keyStatistics[2].UpdatedById);

            Assert.Equal(keyStat3.Id, keyStatistics[3].Id);
            Assert.Equal(3, keyStatistics[3].Order);
            Assert.Equal(keyStat3.CreatedById, keyStatistics[3].CreatedById);
            Assert.Equal(_userId, keyStatistics[3].UpdatedById);
        }
    }

    [Fact]
    public async Task Reorder_InvalidKeyStat()
    {
        var dataBlockId = Guid.NewGuid();

        var keyStat0 = new KeyStatisticText { Order = 0 };
        var keyStat1 = new KeyStatisticDataBlock { Order = 1 };
        var keyStat2 = new KeyStatisticText { Order = 2 };
        var keyStat3 = new KeyStatisticDataBlock { Order = 3 };

        var releaseVersion = new ReleaseVersion
        {
            KeyStatistics = new List<KeyStatistic> { keyStat0, keyStat1, keyStat2, keyStat3 },
        };
        var dataBlock = new DataBlock { Id = dataBlockId, ReleaseVersion = releaseVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var invalidKeyStatId = Guid.NewGuid();
            var result = await keyStatisticService.Reorder(
                releaseVersion.Id,
                new List<Guid> { keyStat0.Id, keyStat1.Id, invalidKeyStatId, keyStat3.Id }
            );

            result.AssertBadRequest(ProvidedKeyStatIdsDifferFromReleaseKeyStatIds);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.OrderBy(ks => ks.Order).ToList();
            Assert.Equal(4, keyStatistics.Count);

            // No key stats should have changed
            Assert.Equal(keyStat0.Id, keyStatistics[0].Id);
            Assert.Equal(0, keyStatistics[0].Order);
            Assert.Equal(keyStat1.Id, keyStatistics[1].Id);
            Assert.Equal(1, keyStatistics[1].Order);
            Assert.Equal(keyStat2.Id, keyStatistics[2].Id);
            Assert.Equal(2, keyStatistics[2].Order);
            Assert.Equal(keyStat3.Id, keyStatistics[3].Id);
            Assert.Equal(3, keyStatistics[3].Order);
        }
    }

    [Fact]
    public async Task Reorder_WrongNumberOfKeyStats()
    {
        var dataBlockId = Guid.NewGuid();

        var keyStat0 = new KeyStatisticText { Order = 0 };
        var keyStat1 = new KeyStatisticDataBlock { Order = 1 };
        var keyStat2 = new KeyStatisticText { Order = 2 };
        var keyStat3 = new KeyStatisticDataBlock { Order = 3 };

        var releaseVersion = new ReleaseVersion
        {
            KeyStatistics = new List<KeyStatistic> { keyStat0, keyStat1, keyStat2, keyStat3 },
        };
        var dataBlock = new DataBlock { Id = dataBlockId, ReleaseVersion = releaseVersion };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            context.ContentBlocks.AddRange(dataBlock);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Reorder(
                releaseVersion.Id,
                new List<Guid>
                {
                    keyStat0.Id,
                    keyStat1.Id,
                    keyStat2.Id,
                    keyStat3.Id,
                    Guid.NewGuid(),
                }
            );
            result.AssertBadRequest(ProvidedKeyStatIdsDifferFromReleaseKeyStatIds);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.OrderBy(ks => ks.Order).ToList();
            Assert.Equal(4, keyStatistics.Count);

            // No key stats should have changed
            Assert.Equal(keyStat0.Id, keyStatistics[0].Id);
            Assert.Equal(0, keyStatistics[0].Order);
            Assert.Equal(keyStat1.Id, keyStatistics[1].Id);
            Assert.Equal(1, keyStatistics[1].Order);
            Assert.Equal(keyStat2.Id, keyStatistics[2].Id);
            Assert.Equal(2, keyStatistics[2].Order);
            Assert.Equal(keyStat3.Id, keyStatistics[3].Id);
            Assert.Equal(3, keyStatistics[3].Order);
        }
    }

    [Fact]
    public async Task Reorder_NoRelease()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Reorder(Guid.NewGuid(), new List<Guid>());

            result.AssertNotFound();
        }
    }

    private KeyStatisticService SetupKeyStatisticService(
        ContentDbContext contentDbContext,
        IDataBlockService? dataBlockService = null,
        IUserService? userService = null
    )
    {
        return new KeyStatisticService(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            dataBlockService ?? Mock.Of<IDataBlockService>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService(_userId).Object,
            AdminMapper()
        );
    }
}
