using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class MethodologyServiceTests
    {
        [Fact]
        public async Task Get()
        {
            const string slug = "methodology-1";

            // Set up a methodology with a published version
            var methodology1 = new MethodologyParent
            {
                Slug = slug,
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Approved,
                        Summary = "Methodology 1 summary",
                        Title = "Methodology 1 title",
                        Version = 0
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Approved,
                        Summary = "Methodology 1 summary updated",
                        Title = "Methodology 1 title updated",
                        Version = 1
                    },
                    // Latest version is a draft that should not be returned
                    new Methodology
                    {
                        Id = Guid.Parse("ad13999c-4caf-4f82-8df0-94db22acbcbc"),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Draft,
                        Summary = "Methodology 1 summary draft",
                        Title = "Methodology 1 title draft",
                        Version = 2
                    }
                )
            };

            // Set up a different methodology with a different slug to make sure it's not returned
            var methodology2 = new MethodologyParent
            {
                Slug = "methodology-2",
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Slug = "methodology-2",
                        Status = Approved,
                        Summary = "Methodology 2 summary",
                        Title = "Methodology 2 title",
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyServiceTests(contentDbContext: contentDbContext);

                var result = await service.Get(slug);

                Assert.True(result.IsRight);

                Assert.Equal(Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"), result.Right.Id);
                Assert.Equal("Methodology 1 title updated", result.Right.Title);
                Assert.Equal("Methodology 1 summary updated", result.Right.Summary);

                var annexes = result.Right.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Right.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }
        }

        [Fact]
        public async Task Get_MethodologyHasNoPublishedVersion()
        {
            const string slug = "methodology-1";

            // Set up a methodology with a draft version that should not be returned
            var methodology1 = new MethodologyParent
            {
                Slug = slug,
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Draft,
                        Summary = "Methodology 1 summary",
                        Title = "Methodology 1 title",
                        Version = 0
                    }
                )
            };

            // Set up a different approved methodology with a different slug to make sure it's not returned
            var methodology2 = new MethodologyParent
            {
                Slug = "methodology-2",
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.NewGuid(),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Slug = "methodology-2",
                        Status = Approved,
                        Summary = "Methodology 2 summary",
                        Title = "Methodology 2 title",
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyServiceTests(contentDbContext: contentDbContext);

                var result = await service.Get(slug);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Get_SlugNotFound()
        {
            // Set up a different methodology with a different slug to make sure it's not returned
            var methodology = new MethodologyParent
            {
                Slug = "some-other-slug",
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.NewGuid(),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Slug = "some-other-slug",
                        Status = Approved,
                        Summary = "Methodology summary",
                        Title = "Methodology title",
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyServiceTests(contentDbContext: contentDbContext);

                var result = await service.Get("methodology-slug");

                result.AssertNotFound();
            }
        }

        private static MethodologyService SetupMethodologyServiceTests(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IMapper mapper = null)
        {
            return new MethodologyService(
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
