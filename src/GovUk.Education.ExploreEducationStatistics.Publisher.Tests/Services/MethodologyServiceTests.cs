using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class MethodologyServiceTests
    {
        private static readonly Theme Theme = new Theme
        {
            Id = Guid.NewGuid(),
            Title = "Theme A",
            Slug = "theme-a",
            Summary = "The first theme"
        };

        private static readonly Topic Topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = "Topic A",
            ThemeId = Theme.Id,
            Slug = "topic-a",
        };

        private static readonly Methodology MethodologyA = new Methodology
        {
            Id = Guid.NewGuid(),
            Slug = "methodology-a",
            Title = "Methodology A",
            Summary = "first methodology",
            Published = new DateTime(2019, 1, 01),
            Updated = new DateTime(2019, 1, 15),
            Annexes = new List<ContentSection>(),
            Content = new List<ContentSection>()
        };

        private static readonly Methodology MethodologyB = new Methodology
        {
            Id = Guid.NewGuid(),
            Slug = "methodology-b",
            Title = "Methodology B",
            Summary = "second methodology",
            Published = new DateTime(2019, 3, 01),
            Updated = new DateTime(2019, 3, 15),
            Annexes = new List<ContentSection>(),
            Content = new List<ContentSection>()
        };

        private static readonly Methodology MethodologyC = new Methodology
        {
            Id = Guid.NewGuid(),
            Slug = "methodology-c",
            Title = "Methodology C",
            Summary = "third methodology",
            Published = null,
            Updated = new DateTime(2019, 6, 15),
            Annexes = new List<ContentSection>(),
            Content = new List<ContentSection>()
        };

        private static readonly Publication PublicationA = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication A",
            TopicId = Topic.Id,
            Slug = "publication-a",
            Summary = "first publication",
            MethodologyId = MethodologyA.Id
        };

        private static readonly Publication PublicationB = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication B",
            TopicId = Topic.Id,
            Slug = "publication-b",
            Summary = "second publication",
            MethodologyId = MethodologyB.Id
        };

        private static readonly Release PublicationARelease1 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = new DateTime(2019, 1, 01),
            Status = Approved
        };

        private static readonly Release PublicationBRelease1 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationB.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Published = null,
            Status = Draft
        };

        [Fact]
        public async Task GetTree()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(new List<Methodology>
                {
                    MethodologyA, MethodologyB
                });

                await contentDbContext.AddAsync(Theme);
                await contentDbContext.AddAsync(Topic);

                await contentDbContext.AddRangeAsync(new List<Publication>
                {
                    PublicationA, PublicationB
                });

                await contentDbContext.AddRangeAsync(new List<Release>
                {
                    PublicationARelease1, PublicationBRelease1
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = new MethodologyService(contentDbContext, MapperForProfile<MappingProfiles>());

                var result = service.GetTree(Enumerable.Empty<Guid>());

                Assert.Single(result);
                var theme = result.First();
                Assert.Equal("Theme A", theme.Title);

                Assert.Single(theme.Topics);
                var topic = theme.Topics.First();
                Assert.Equal("Topic A", topic.Title);

                Assert.Single(topic.Publications);
                var publication = topic.Publications.First();
                Assert.Equal("Publication A", publication.Title);

                var methodology = publication.Methodology;
                Assert.Equal("methodology-a", methodology.Slug);
                Assert.Equal("first methodology", methodology.Summary);
                Assert.Equal("Methodology A", methodology.Title);
            }
        }

        [Fact]
        public async Task GetFiles()
        {
            var methodology = new Methodology();

            var imageFile1 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var otherFile = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2, otherFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = new MethodologyService(contentDbContext, MapperForProfile<MappingProfiles>());

                var result = await service.GetFiles(methodology.Id, Image);

                Assert.Equal(2, result.Count);
                Assert.Equal(imageFile1.File.Id, result[0].Id);
                Assert.Equal(imageFile2.File.Id, result[1].Id);
            }
        }

        [Fact]
        public async Task GetViewModelAsync()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(MethodologyA);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = new MethodologyService(contentDbContext, MapperForProfile<MappingProfiles>());

                var result = await service.GetViewModelAsync(MethodologyA.Id, PublishContext());

                Assert.Equal(MethodologyA.Id, result.Id);
                Assert.Equal("Methodology A", result.Title);
                Assert.Equal(new DateTime(2019, 1, 01), result.Published);
                Assert.Equal(new DateTime(2019, 1, 15), result.Updated);
                Assert.Equal("first methodology", result.Summary);

                var annexes = result.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }
        }

        [Fact]
        public async Task GetViewModelAsync_NotYetPublished()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(MethodologyC);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = new MethodologyService(contentDbContext, MapperForProfile<MappingProfiles>());

                var context = PublishContext();
                var result = await service.GetViewModelAsync(MethodologyC.Id, context);

                Assert.Equal(MethodologyC.Id, result.Id);
                Assert.Equal("Methodology C", result.Title);
                Assert.Equal(context.Published, result.Published);
                Assert.Equal(new DateTime(2019, 6, 15), result.Updated);
                Assert.Equal("third methodology", result.Summary);

                var annexes = result.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }
        }

        private static PublishContext PublishContext()
        {
            var published = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            return new PublishContext(published, true);
        }
    }
}
