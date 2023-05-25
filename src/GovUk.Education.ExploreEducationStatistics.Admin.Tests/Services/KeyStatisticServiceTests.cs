#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class KeyStatisticServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CreateKeyStatisticDataBlock()
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
            dataBlockService.Setup(s => s.GetUnattachedDataBlocks(release.Id))
                .ReturnsAsync(new Either<ActionResult, List<DataBlockViewModel>>(
                    new List<DataBlockViewModel>
                    {
                        new ()
                        {
                            Id = dataBlock.Id
                        },
                    }));

            dataBlockService.Setup(s =>
                    s.IsUnattachedDataBlock(
                        release.Id,
                        It.Is<DataBlock>(db => db.Id == dataBlock.Id)))
                .ReturnsAsync(true);

            var keyStatisticService = SetupKeyStatisticService(context,
                dataBlockService: dataBlockService.Object);

            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                release.Id,
                new KeyStatisticDataBlockCreateRequest
                {
                    DataBlockId = dataBlock.Id,
                    Trend = "trend",
                    GuidanceTitle = "guidanceTitle",
                    GuidanceText = "guidanceText",
                });

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlock.Id, viewModel.DataBlockId);

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

            Assert.Equal(dataBlock.Id, keyStatDataBlock.DataBlockId);

            Assert.Equal(release.Id, keyStatDataBlock.ReleaseId);
            Assert.Equal("trend", keyStatDataBlock.Trend);
            Assert.Equal("guidanceTitle", keyStatDataBlock.GuidanceTitle);
            Assert.Equal("guidanceText", keyStatDataBlock.GuidanceText);
            Assert.Equal(0, keyStatDataBlock.Order);
            Assert.Equal(_userId, keyStatDataBlock.CreatedById);
            Assert.Null(keyStatDataBlock.UpdatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(keyStatDataBlock.Created).Milliseconds, 0, 1500);
            Assert.Null(keyStatDataBlock.Updated);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_Order()
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
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            dataBlockService.Setup(s => s.GetUnattachedDataBlocks(release.Id))
                .ReturnsAsync(new Either<ActionResult, List<DataBlockViewModel>>(
                    new List<DataBlockViewModel>
                    {
                        new()
                        {
                            Id = dataBlock.Id
                        },
                    }));
            dataBlockService.Setup(s =>
                    s.IsUnattachedDataBlock(
                        release.Id,
                        It.Is<DataBlock>(db => db.Id == dataBlock.Id)))
                .ReturnsAsync(true);


            var keyStatisticService = SetupKeyStatisticService(context,
                dataBlockService: dataBlockService.Object);

            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                release.Id,
                new KeyStatisticDataBlockCreateRequest
                {
                    DataBlockId = dataBlock.Id,
                });

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlock.Id, viewModel.DataBlockId);

            Assert.Null(viewModel.Trend);
            Assert.Null(viewModel.GuidanceTitle);
            Assert.Null(viewModel.GuidanceText);
            Assert.Equal(3, viewModel.Order);
            Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Created).Milliseconds, 0, 1500);
            Assert.Null(viewModel.Updated);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics
                .OrderBy(ks => ks.Order)
                .ToList();
            Assert.Equal(4, keyStatistics.Count);

            Assert.Equal(0, keyStatistics[0].Order);
            Assert.Equal(1, keyStatistics[1].Order);
            Assert.Equal(2, keyStatistics[2].Order);

            var keyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(keyStatistics[3]);

            Assert.Equal(dataBlock.Id, keyStatDataBlock.DataBlockId);

            Assert.Equal(release.Id, keyStatDataBlock.ReleaseId);
            Assert.Null(keyStatDataBlock.Trend);
            Assert.Null(keyStatDataBlock.GuidanceTitle);
            Assert.Null(keyStatDataBlock.GuidanceText);
            Assert.Equal(3, keyStatDataBlock.Order);
            Assert.Equal(_userId, keyStatDataBlock.CreatedById);
            Assert.Null(keyStatDataBlock.UpdatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(keyStatDataBlock.Created).Milliseconds, 0, 1500);
            Assert.Null(keyStatDataBlock.Updated);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_NoRelease()
    {
        var dataBlockId = Guid.NewGuid();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
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
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                Guid.NewGuid(),
                new KeyStatisticDataBlockCreateRequest
                {
                    DataBlockId = dataBlockId,
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_NoDataBlock()
    {
        var dataBlockId = Guid.NewGuid();
        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
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
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                release.Id,
                new KeyStatisticDataBlockCreateRequest
                {
                    DataBlockId = Guid.NewGuid(),
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateKeyStatisticDataBlock_DataBlockAttachedToContent()
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
            KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticDataBlock
                {
                    DataBlock = dataBlock,
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
            dataBlockService.Setup(s => s.GetUnattachedDataBlocks(release.Id))
                .ReturnsAsync(new Either<ActionResult, List<DataBlockViewModel>>(
                    new List<DataBlockViewModel>
                    {
                        new (),
                        new (),
                    }));

            dataBlockService.Setup(s =>
                    s.IsUnattachedDataBlock(
                        release.Id,
                        It.Is<DataBlock>(db => db.Id == dataBlock.Id)))
                .ReturnsAsync(false);

            var keyStatisticService = SetupKeyStatisticService(context,
                dataBlockService: dataBlockService.Object);

            var result = await keyStatisticService.CreateKeyStatisticDataBlock(
                release.Id,
                new KeyStatisticDataBlockCreateRequest
                {
                    DataBlockId = dataBlock.Id,
                });
            result.AssertBadRequest(DataBlockShouldBeUnattached);
        }
    }

    [Fact]
    public async Task CreateKeyStatisticText()
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
            var keyStatisticService = SetupKeyStatisticService(context);

            var result = await keyStatisticService.CreateKeyStatisticText(
                release.Id,
                new KeyStatisticTextCreateRequest
                {
                    Title = "title",
                    Statistic = "Over 9000!",
                    Trend = "trend",
                    GuidanceTitle = "guidanceTitle",
                    GuidanceText = "guidanceText",
                });

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
            Assert.Equal(release.Id, keyStatText.ReleaseId);
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
        var release = new Release();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.CreateKeyStatisticText(
                Guid.NewGuid(),
                new KeyStatisticTextCreateRequest
                {
                    Title = "title",
                    Statistic = "Over 9000!",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateKeyStatisticText_Order()
    {
        var release = new Release
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
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);

            var result = await keyStatisticService.CreateKeyStatisticText(
                release.Id,
                new KeyStatisticTextCreateRequest
                {
                    Title = "title",
                    Statistic = "Over 9000!",
                });

            var viewModel = result.AssertRight();

            Assert.Equal("title", viewModel.Title);
            Assert.Equal(3, viewModel.Order);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics
                .OrderBy(ks => ks.Order)
                .ToList();
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
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
            CreatedById = Guid.NewGuid(),
            UpdatedById = Guid.NewGuid(),
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticDataBlock,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                release.Id,
                keyStatisticDataBlock.Id,
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                });

            var viewModel = result.AssertRight();

            Assert.Equal(dataBlockId, viewModel.DataBlockId);

            Assert.Equal("new trend", viewModel.Trend);
            Assert.Equal("new guidanceTitle", viewModel.GuidanceTitle);
            Assert.Equal("new guidanceText", viewModel.GuidanceText);
            Assert.Equal(0, viewModel.Order);
            Assert.Equal(new DateTime(2000, 1, 1), viewModel.Created);
            Assert.NotNull(viewModel.Updated);
            Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Updated!.Value).Milliseconds, 0, 1500);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            var keyStat = Assert.Single(keyStatistics);
            var keyStatDataBlock = Assert.IsType<KeyStatisticDataBlock>(keyStat);

            Assert.Equal(dataBlockId, keyStatDataBlock.DataBlockId);

            Assert.Equal(release.Id, keyStatDataBlock.ReleaseId);
            Assert.Equal("new trend", keyStatDataBlock.Trend);
            Assert.Equal("new guidanceTitle", keyStatDataBlock.GuidanceTitle);
            Assert.Equal("new guidanceText", keyStatDataBlock.GuidanceText);
            Assert.Equal(0, keyStatDataBlock.Order);
            Assert.Equal(new DateTime(2000, 1, 1), keyStatDataBlock.Created);
            Assert.NotNull(keyStatDataBlock.Updated);
            Assert.Equal(keyStatisticDataBlock.CreatedById, keyStatDataBlock.CreatedById);
            Assert.Equal(_userId, keyStatDataBlock.UpdatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(keyStatDataBlock.Updated!.Value).Milliseconds, 0, 1500);
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock_NoRelease()
    {
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticDataBlock,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
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
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock_NoKeyStatisticDataBlock()
    {
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticDataBlock,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                release.Id,
                Guid.NewGuid(),
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock_KeyStatWrongType()
    {
        var releaseId = Guid.NewGuid();
        var keyStatisticText = new KeyStatisticText
        {
            ReleaseId = releaseId,
            Title = "title",
            Statistic = "Over 9000",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticText,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticDataBlock(
                release.Id,
                keyStatisticText.Id,
                new KeyStatisticDataBlockUpdateRequest
                {
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText()
    {
        var releaseId = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            ReleaseId = releaseId,
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

        var release = new Release
        {
            Id = releaseId,
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticText,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                release.Id,
                keyStatisticText.Id,
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                });

            var viewModel = result.AssertRight();

            Assert.Equal("new title", viewModel.Title);
            Assert.Equal("new statistic", viewModel.Statistic);
            Assert.Equal("new trend", viewModel.Trend);
            Assert.Equal("new guidanceTitle", viewModel.GuidanceTitle);
            Assert.Equal("new guidanceText", viewModel.GuidanceText);
            Assert.Equal(0, viewModel.Order);
            Assert.Equal(new DateTime(2000, 1, 1), viewModel.Created);
            Assert.NotNull(viewModel.Updated);
            Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Updated!.Value).Milliseconds, 0, 1500);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            var keyStat = Assert.Single(keyStatistics);
            var keyStatText = Assert.IsType<KeyStatisticText>(keyStat);

            Assert.Equal(release.Id, keyStatText.ReleaseId);
            Assert.Equal("new title", keyStatText.Title);
            Assert.Equal("new statistic", keyStatText.Statistic);
            Assert.Equal("new trend", keyStatText.Trend);
            Assert.Equal("new guidanceTitle", keyStatText.GuidanceTitle);
            Assert.Equal("new guidanceText", keyStatText.GuidanceText);
            Assert.Equal(0, keyStatText.Order);
            Assert.Equal(new DateTime(2000, 1, 1), keyStatText.Created);
            Assert.NotNull(keyStatText.Updated);
            Assert.Equal(keyStatisticText.CreatedById, keyStatText.CreatedById);
            Assert.Equal(_userId, keyStatText.UpdatedById);
            Assert.InRange(DateTime.UtcNow.Subtract(keyStatText.Updated!.Value).Milliseconds, 0, 1500);
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText_NoRelease()
    {
        var releaseId = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            ReleaseId = releaseId,
            Title = "title",
            Statistic = "statistic",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticText,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
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
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText_NoKeyStatisticText()
    {
        var releaseId = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            ReleaseId = releaseId,
            Title = "title",
            Statistic = "statistic",
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticText,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                release.Id,
                Guid.NewGuid(),
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateKeyStatisticText_KeyStatWrongType()
    {
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticDataBlock,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.UpdateKeyStatisticText(
                release.Id,
                keyStatisticDataBlock.Id,
                new KeyStatisticTextUpdateRequest
                {
                    Title = "new title",
                    Statistic = "new statistic",
                    Trend = "new trend",
                    GuidanceTitle = "new guidanceTitle",
                    GuidanceText = "new guidanceText",
                });

            result.AssertNotFound();
        }
    }


    [Fact]
    public async Task Delete_KeyStatisticDataBlock()
    {
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticText(),
                keyStatisticDataBlock,
                new KeyStatisticText(),
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(
                release.Id,
                keyStatisticDataBlock.Id);

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            Assert.Equal(2, keyStatistics.Count);
            Assert.Null(keyStatistics.Find(ks => ks.Id == keyStatisticDataBlock.Id));

            var dataBlockList = context.DataBlocks.ToList();
            var dataBlock = Assert.Single(dataBlockList);
            Assert.Equal(dataBlockId, dataBlock.Id);
        }
    }

    [Fact]
    public async Task Delete_KeyStatisticText()
    {
        var releaseId = Guid.NewGuid();

        var keyStatisticText = new KeyStatisticText
        {
            Title = "title",
            Statistic = "Over 9000",
            ReleaseId = releaseId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            KeyStatistics = new List<KeyStatistic>
            {
                new KeyStatisticText(),
                keyStatisticText,
                new KeyStatisticDataBlock(),
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(
                release.Id,
                keyStatisticText.Id);

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
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticDataBlock,
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(
                Guid.NewGuid(),
                keyStatisticDataBlock.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_NoKeyStatistic()
    {
        var releaseId = Guid.NewGuid();
        var dataBlockId = Guid.NewGuid();

        var keyStatisticDataBlock = new KeyStatisticDataBlock
        {
            ReleaseId = releaseId,
            DataBlockId = dataBlockId,
            Trend = "trend",
            GuidanceTitle = "guidanceTitle",
            GuidanceText = "guidanceText",
            Created = new DateTime(2000, 1, 1),
            Updated = null,
        };

        var release = new Release
        {
            Id = releaseId,
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock { Id = dataBlockId },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStatisticDataBlock,
                new KeyStatisticText(),
                new KeyStatisticText(),
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(release);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(
                release.Id,
                Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_KeyStatAttachedToDifferentRelease()
    {
        var release = new Release();
        var incorrectRelease = new Release();

        var keyStatisticText = new KeyStatisticText
        {
            Release = release,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.Releases.AddRangeAsync(incorrectRelease, release);
            await context.KeyStatistics.AddRangeAsync(keyStatisticText);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Delete(
                incorrectRelease.Id,
                keyStatisticText.Id);

            result.AssertNotFound();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics.ToList();
            Assert.Single(keyStatistics);
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

        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStat0, keyStat1, keyStat2, keyStat3,
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
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Reorder(
                release.Id,
                new List<Guid>
                {
                    keyStat0.Id,
                    keyStat1.Id,
                    keyStat2.Id,
                    keyStat3.Id,
                });

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
            var keyStatistics = context.KeyStatistics
                .OrderBy(ks => ks.Order)
                .ToList();
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

        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStat0, keyStat1, keyStat2, keyStat3,
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
            var keyStatisticService = SetupKeyStatisticService(context);
            var invalidKeyStatId = Guid.NewGuid();
            var result = await keyStatisticService.Reorder(
                release.Id,
                new List<Guid>
                {
                    keyStat0.Id,
                    keyStat1.Id,
                    invalidKeyStatId,
                    keyStat3.Id,
                });

            result.AssertBadRequest(ProvidedKeyStatIdsDifferFromReleaseKeyStatIds);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics
                .OrderBy(ks => ks.Order)
                .ToList();
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

        var release = new Release
        {
            ContentBlocks = new List<ReleaseContentBlock>
            {
                new()
                {
                    ContentBlock = new DataBlock
                    {
                        Id = dataBlockId,
                    },
                },
            },
            KeyStatistics = new List<KeyStatistic>
            {
                keyStat0, keyStat1, keyStat2, keyStat3,
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
            var keyStatisticService = SetupKeyStatisticService(context);
            var result = await keyStatisticService.Reorder(
                release.Id,
                new List<Guid>
                {
                    keyStat0.Id,
                    keyStat1.Id,
                    keyStat2.Id,
                    keyStat3.Id,
                    Guid.NewGuid(),
                });
            result.AssertBadRequest(ProvidedKeyStatIdsDifferFromReleaseKeyStatIds);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var keyStatistics = context.KeyStatistics
                .OrderBy(ks => ks.Order)
                .ToList();
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
            var result = await keyStatisticService.Reorder(
                Guid.NewGuid(),
                new List<Guid>());

            result.AssertNotFound();
        }
    }

    private KeyStatisticService SetupKeyStatisticService(
        ContentDbContext contentDbContext,
        IDataBlockService? dataBlockService = null,
        IUserService? userService = null)
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
