using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
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

        [Fact]
        public async Task GetSummariesByPublication()
        {
            var publication = new Publication();

            var methodologies = AsList(
                new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-1",
                    Title = "Methodology 1 title",
                },
                new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-2",
                    Title = "Methodology 2 title",
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByPublication(publication.Id))
                .ReturnsAsync(methodologies);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyServiceTests(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetSummariesByPublication(publication.Id);

                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(methodologies[0].Id, result.Right[0].Id);
                Assert.Equal(methodologies[0].Slug, result.Right[0].Slug);
                Assert.Equal(methodologies[0].Title, result.Right[0].Title);

                Assert.Equal(methodologies[1].Id, result.Right[1].Id);
                Assert.Equal(methodologies[1].Slug, result.Right[1].Slug);
                Assert.Equal(methodologies[1].Title, result.Right[1].Title);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetSummariesByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByPublication(publication.Id))
                .ReturnsAsync(new List<Methodology>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyServiceTests(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetSummariesByPublication(publication.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetSummariesByPublication_PublicationNotFound()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupMethodologyServiceTests(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetSummariesByPublication(Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        private static MethodologyService SetupMethodologyServiceTests(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IMethodologyRepository methodologyRepository = null,
            IMapper mapper = null)
        {
            return new MethodologyService(
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.MapperForProfile<MappingProfiles>(),
                methodologyRepository ?? new Mock<IMethodologyRepository>().Object
            );
        }
    }
}
