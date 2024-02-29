#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class LegacyReleaseServiceTests
    {
        private const string PublicationSlug = "publication-slug";

        [Fact]
        public async Task GetLegacyRelease()
        {
            var id = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Add(new Publication
                {
                    LegacyReleases = new List<LegacyRelease>
                    {
                        new()
                        {
                            Id = id,
                            Description = "Test description",
                            Url = "https://test.com",
                            Order = 1,
                        }
                    }
                });

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var legacyReleaseService = BuildLegacyReleaseService(context);

                // Service method under test
                var result = await legacyReleaseService.GetLegacyRelease(id);

                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("https://test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);
            }
        }

        //[Fact]
        //public async Task ListLegacyReleases() // @MarkFix
        //{
        //    var release1Id = Guid.NewGuid();
        //    var release2Id = Guid.NewGuid();
        //    var release3Id = Guid.NewGuid();

        //    var publication = new Publication
        //    {
        //        LegacyReleases = new()
        //        {
        //            new()
        //            {
        //                Id = release1Id,
        //                Description = "Release 1",
        //                Url = "https://test-1.com",
        //            },
        //            new()
        //            {
        //                Id = release3Id,
        //                Description = "Release 3",
        //                Url = "https://test-3.com",
        //            },
        //            new()
        //            {
        //                Id = release2Id,
        //                Description = "Release 2",
        //                Url = "https://test-2.com",
        //            }
        //        },
        //        ReleaseSeriesView = new()
        //        {
        //            new()
        //            {
        //                ReleaseId = release1Id,
        //                IsDraft = false,
        //                IsLegacy = true,
        //                Order = 1
        //            },
        //            new()
        //            {
        //                ReleaseId = release3Id,
        //                IsDraft = false,
        //                IsLegacy = true,
        //                Order = 3
        //            },
        //            new()
        //            {
        //                ReleaseId = release2Id,
        //                IsDraft = false,
        //                IsLegacy = true,
        //                Order = 2
        //            }
        //        }
        //    };

        //    var contentDbContextId = Guid.NewGuid().ToString();

        //    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        //    {
        //        contentDbContext.Publications.Add(publication);
        //        await contentDbContext.SaveChangesAsync();
        //    }

        //    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        //    {
        //        var service = BuildLegacyReleaseService(contentDbContext);

        //        var result = await service.ListLegacyReleases(publication.Id);
        //        var viewModels = result.AssertRight();

        //        var releaseSeries = contentDbContext.Publications
        //            .Find(publication.Id)!.ReleaseSeriesView
        //            .OrderByDescending(ro => ro.Order)
        //            .ToList();

        //        Assert.Equal(3, viewModels.Count);

        //        Assert.Equal("Release 3", viewModels[0].Description);
        //        Assert.Equal("https://test-3.com", viewModels[0].Url);
        //        Assert.Equal(3, viewModels[0].Order);
        //        Assert.Equal(publication.LegacyReleases[1].Id, releaseSeries[0].ReleaseId);
        //        Assert.Equal(3, releaseSeries[0].Order);
        //        Assert.True(releaseSeries[0].IsLegacy);
        //        Assert.False(releaseSeries[0].IsDraft);

        //        Assert.Equal("Release 2", viewModels[1].Description);
        //        Assert.Equal("https://test-2.com", viewModels[1].Url);
        //        Assert.Equal(2, viewModels[1].Order);
        //        Assert.Equal(publication.LegacyReleases[2].Id, releaseSeries[1].ReleaseId);
        //        Assert.Equal(2, releaseSeries[1].Order);
        //        Assert.True(releaseSeries[1].IsLegacy);
        //        Assert.False(releaseSeries[1].IsDraft);

        //        Assert.Equal("Release 1", viewModels[2].Description);
        //        Assert.Equal("https://test-1.com", viewModels[2].Url);
        //        Assert.Equal(1, viewModels[2].Order);
        //        Assert.Equal(publication.LegacyReleases[0].Id, releaseSeries[2].ReleaseId);
        //        Assert.Equal(1, releaseSeries[2].Order);
        //        Assert.True(releaseSeries[2].IsLegacy);
        //        Assert.False(releaseSeries[2].IsDraft);
        //    }
        //}

        [Fact]
        public async Task CreateLegacyRelease()
        {
            // Arrange
            var publicationId = Guid.NewGuid();
            var contentDbContextId = Guid.NewGuid().ToString();
            var publication = new Publication
            {
                Id = publicationId,
                Slug = PublicationSlug
            };

            using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Add(publication);

                await context.SaveChangesAsync();
            }

            var publicationReleaseSeriesViewService = new Mock<IPublicationReleaseSeriesViewService>(Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationReleaseSeriesViewService.Setup(s => s.CreateForCreateLegacyRelease(
                publicationId,
                It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

            publicationCacheService.Setup(s => s.UpdatePublication(PublicationSlug))
                .ReturnsAsync(new PublicationCacheViewModel());

            using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicationCacheService: publicationCacheService.Object,
                    publicationReleaseSeriesViewService: publicationReleaseSeriesViewService.Object);

                // Act
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new LegacyReleaseCreateViewModel
                    {
                        Description = "Test description",
                        Url = "https://test.com",
                        PublicationId = publicationId
                    });

                // Assert
                VerifyAllMocks(publicationReleaseSeriesViewService);
                VerifyAllMocks(publicationCacheService);

                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("https://test.com", result.Right.Url);
                Assert.Equal(0, result.Right.Order); // No longer set (ordering moved to Publication.ReleaseSeriesView)

                var savedLegacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Test description", savedLegacyRelease.Description);
                Assert.Equal("https://test.com", savedLegacyRelease.Url);
                Assert.Equal(0, savedLegacyRelease.Order); // No longer set (ordering moved to Publication.ReleaseSeriesView)
                Assert.Equal(publicationId, savedLegacyRelease.PublicationId);
            }
        }

        [Fact]
        public async Task UpdateLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    Slug = PublicationSlug,
                    LegacyReleases = new List<LegacyRelease>
                    {
                        new()
                        {
                            Id = id,
                            Description = "Test description",
                            Url = "https://test.com",
                        },
                    }
                });

                await context.SaveChangesAsync();
            }

            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationCacheService.Setup(s => s.UpdatePublication(PublicationSlug))
                .ReturnsAsync(new PublicationCacheViewModel());

            using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicationCacheService: publicationCacheService.Object);

                // Service method under test
                var result = await legacyReleaseService.UpdateLegacyRelease(
                    id,
                    new LegacyReleaseUpdateViewModel()
                    {
                        Description = "Updated test description",
                        Url = "https://updated-test.com",
                        PublicationId = publicationId,
                    });

                VerifyAllMocks(publicationCacheService);

                Assert.Equal("Updated test description", result.Right.Description);
                Assert.Equal("https://updated-test.com", result.Right.Url);

                var savedLegacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Updated test description", savedLegacyRelease.Description);
                Assert.Equal("https://updated-test.com", savedLegacyRelease.Url);
                Assert.Equal(publicationId, savedLegacyRelease.PublicationId);
            }
        }

        //[Fact] // @MarkFix
        //public async Task DeleteLegacyRelease()
        //{
        //    var id = Guid.NewGuid();
        //    var publicationId = Guid.NewGuid();

        //    var contentDbContextId = Guid.NewGuid().ToString();

        //    using (var context = InMemoryApplicationDbContext(contentDbContextId))
        //    {
        //        context.Add(new Publication
        //        {
        //            Id = publicationId,
        //            Slug = PublicationSlug,
        //            LegacyReleases = new List<LegacyRelease>
        //            {
        //                new()
        //                {
        //                    Id = id,
        //                    Description = "Test description",
        //                    Url = "https://test.com",
        //                }
        //            },
        //            ReleaseSeriesView = new()
        //            {
        //                new()
        //                {
        //                    ReleaseId = id,
        //                    Order = 1,
        //                    IsLegacy = true
        //                }
        //            }
        //        });

        //        await context.SaveChangesAsync();
        //    }

        //    var publicationReleaseSeriesViewService = new Mock<IPublicationReleaseSeriesViewService>(Strict);
        //    var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        //    publicationReleaseSeriesViewService.Setup(s => s.DeleteForDeleteLegacyRelease(
        //        It.IsAny<Guid>()))
        //    .Returns(Task.CompletedTask);

        //    publicationCacheService
        //        .Setup(s => s.UpdatePublication(PublicationSlug))
        //        .ReturnsAsync(new PublicationCacheViewModel());

        //    using (var context = InMemoryApplicationDbContext(contentDbContextId))
        //    {
        //        var legacyReleaseService = BuildLegacyReleaseService(
        //            context: context,
        //            publicationCacheService: publicationCacheService.Object,
        //            publicationReleaseSeriesViewService: publicationReleaseSeriesViewService.Object);

        //        // Act
        //        await legacyReleaseService.DeleteLegacyRelease(id);

        //        // Assert
        //        VerifyAllMocks(publicationReleaseSeriesViewService);
        //        VerifyAllMocks(publicationCacheService);

        //        Assert.Empty(context.Publications
        //                .Single(publication => publication.Id == publicationId)
        //                .LegacyReleases);
        //    }
        //}

        private static LegacyReleaseService BuildLegacyReleaseService(
            ContentDbContext context,
            IMapper? mapper = null,
            IUserService? userService = null,
            IPublicationCacheService? publicationCacheService = null,
            IPublicationReleaseSeriesViewService? publicationReleaseSeriesViewService = null)
        {
            return new LegacyReleaseService(
                context,
                mapper ?? AdminMapper(),
                userService ?? AlwaysTrueUserService().Object,
                new PersistenceHelper<ContentDbContext>(context),
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
                publicationReleaseSeriesViewService ?? Mock.Of<IPublicationReleaseSeriesViewService>(Strict)
            );
        }
    }
}
