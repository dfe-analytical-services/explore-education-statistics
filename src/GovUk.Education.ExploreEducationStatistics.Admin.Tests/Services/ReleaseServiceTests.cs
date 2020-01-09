using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {
        [Fact]
        public void CreateReleaseNoTemplate()
        {
            var (userService, releaseHelper, publishingService, repository) = Mocks();

            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate"))
            {
                context.Add(new ReleaseType {Id = new Guid("484e6b5c-4a0f-47fd-914e-ac4dac5bdd1c"), Title = "Ad Hoc",});
                context.Add(new Publication
                    {Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Publication"});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(), 
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
                
                var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
                    });

                Assert.Equal("Academic Year 2018/19", result.Result.Right.Title);
                Assert.Null(result.Result.Right.Published);
                Assert.False(result.Result.Right.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, result.Result.Right.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public void CreateReleaseWithTemplate()
        {
            var (userService, releaseHelper, publishingService, repository) = Mocks();

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                context.Add(new ReleaseType {Id = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8"), Title = "Ad Hoc",});
                context.Add(new Publication
                {
                    Id = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                    Title = "Publication",
                    Releases = new List<Release>
                    {
                        new Release // Template release
                        {
                            Id = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                            ReleaseName = "2018",
                            Content = new List<ReleaseContentSection>
                            {
                                new ReleaseContentSection
                                {
                                    ReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                                    ContentSection = new ContentSection
                                    {
                                        Id = new Guid("3cb10587-7b05-4c30-9f13-9f2025aca6a0"),
                                        Caption = "Template caption index 0", // Should be copied 
                                        Heading = "Template heading index 0", // Should be copied
                                        Type = ContentSectionType.Generic,
                                        Order = 1,
                                        Content = new List<IContentBlock>
                                        {
                                            // TODO currently is not copied - should it be?
                                            new HtmlBlock
                                            {
                                                Id = new Guid("e2b96bea-fbbb-4089-ad9c-fecba58ee054"),
                                                Body = @"<div></div>"
                                            }
                                        }
                                    }
                                },
                    
                                new ReleaseContentSection
                                {
                                    ReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                                    ContentSection = new ContentSection
                                    {
                                        Id = new Guid("8e804c94-61b3-4955-9d71-83a56d133a89"),
                                        Caption = "Template caption index 1", // Should be copied 
                                        Heading = "Template heading index 1", // Should be copied
                                        Type = ContentSectionType.Generic,
                                        Order = 2,
                                        Content = new List<IContentBlock>
                                        {
                                            // TODO currently is not copied - should it be?
                                            new InsetTextBlock
                                            {
                                                Id = new Guid("34884271-a30a-4cbd-9c08-e6d11d7f8c8e"),
                                                Body = "Text",
                                                Heading = "Heading"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
                
                // Service method under test
                var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        TypeId = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8")
                    });

                // Do an in depth check of the saved release
                var release = context.Releases
                    .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .Single(r => r.Id == result.Result.Right.Id);

                var contentSections = release.GenericContent.ToList();
                
                Assert.Equal(2, contentSections.Count);
                Assert.Equal("Template caption index 0", release.Content[0].ContentSection.Caption);
                Assert.Equal("Template heading index 0", release.Content[0].ContentSection.Heading);
                Assert.Equal(1, release.Content[0].ContentSection.Order);
                Assert.Empty(contentSections[0].Content); // TODO currently is not copied - should it be?

                Assert.Equal("Template caption index 1", release.Content[1].ContentSection.Caption);
                Assert.Equal("Template heading index 1", release.Content[1].ContentSection.Heading);
                Assert.Equal(2, release.Content[1].ContentSection.Order);
                Assert.Empty(contentSections[1].Content); // TODO currently is not copied - should it be?
                
                Assert.Equal(ContentSectionType.ReleaseSummary, release.SummarySection.Type);
                Assert.Equal(ContentSectionType.Headlines, release.HeadlinesSection.Type);
                Assert.Equal(ContentSectionType.KeyStatistics, release.KeyStatisticsSection.Type);
                Assert.Equal(ContentSectionType.KeyStatisticsSecondary, release.KeyStatisticsSecondarySection.Type);
            }
        }

        [Fact]
        public async void LatestReleaseCorrectlyReported()
        {
            var (userService, releaseHelper, publishingService, repository) = Mocks();

            var latestReleaseId = new Guid("274d4621-7d21-431b-80de-77b62a4374d2");
            var notLatestReleaseId = new Guid("49b73c2f-141a-4dc7-a2d4-69316aef8bbc");
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                context.Add(new Publication
                {
                    Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                    Title = "Publication",
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = notLatestReleaseId,
                            Order = 0,
                            Published = DateTime.Now.AddDays(-2) // Is published but not the latest by order
                        },
                        new Release
                        {
                            Id = latestReleaseId,
                            Order = 1,
                            Published = DateTime.Now.AddDays(-1) // Is published and the the latest by order
                        }
                    }
                });

                context.SaveChanges();
            }
            // Note that we use different contexts for each method call - this is to avoid misleadingly optimistic
            // loading of the entity graph as we go.
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
                
                // Method under test
                var notLatest =
                    await releaseService.GetReleaseForIdAsync(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
                
                // Method under test
                var notLatest =
                    await releaseService.GetReleaseForIdAsync(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }
        }

        [Fact]
        public async void EditReleaseSummary()
        {
            var (userService, releaseHelper, publishingService, repository) = Mocks();

            var releaseId = new Guid("02c73027-3e06-4495-82a4-62b778c005a9");
            var addHocReleaseTypeId = new Guid("f3800c32-1e1c-4d42-8165-d1bcb3c8b47c");
            var officialStatisticsReleaseType = new ReleaseType
            {
                Id = new Guid("fdc4dd4c-85f7-49dd-87a4-e04446bc606f"),
                Title = "Official Statistics"
            };

            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    new ReleaseType
                    {
                        Id = addHocReleaseTypeId,
                        Title = "Ad Hoc"
                    },
                    officialStatisticsReleaseType
                });
                context.Add(new Publication
                {
                    Id = new Guid("f7da23e2-304a-4b47-a8f5-dba28a554de9"),
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = releaseId,
                            TypeId = addHocReleaseTypeId,
                            ReleaseSummary = new ReleaseSummary()
                            {
                                Id = new Guid("0071f344-4b99-4010-ab2f-62bf7b0035b0"),
                                Versions = new List<ReleaseSummaryVersion>() {
                                    new ReleaseSummaryVersion()
                                    {    
                                        Id = new Guid("25f43cba-faee-4b0a-a9d4-a3d114a5f6df"),
                                        Created = DateTime.Now,
                                        TypeId = addHocReleaseTypeId,
                                    }
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }
            
            var publishScheduledEdited = DateTime.Now.AddDays(1);
            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            var typeEdited = officialStatisticsReleaseType;
            const string releaseNameEdited = "2035";
            const TimeIdentifier timePeriodCoverageEdited = TimeIdentifier.March;
            
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
                
                // Method under test 
                var edited = await releaseService
                    .EditReleaseSummaryAsync(
                        releaseId,
                        new UpdateReleaseSummaryRequest
                        {
                            PublishScheduled = publishScheduledEdited,
                            NextReleaseDate = nextReleaseDateEdited,
                            TypeId = typeEdited.Id,
                            ReleaseName = releaseNameEdited,
                            TimePeriodCoverage = timePeriodCoverageEdited
                        });

                Assert.Equal(publishScheduledEdited, edited.Right.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, edited.Right.NextReleaseDate);
                Assert.Equal(typeEdited, edited.Right.Type);
                Assert.Equal(releaseNameEdited, edited.Right.ReleaseName);
                Assert.Equal(timePeriodCoverageEdited, edited.Right.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public async void GetReleaseSummaryAsync()
        {
            var (userService, releaseHelper, publishingService, repository) = Mocks();
            
            var releaseId = new Guid("5cf345d4-7f7b-425c-8267-de785cfc040b");
            var adhocReleaseType = new ReleaseType
            {
                Id = new Guid("19b024dc-339c-4e2c-b2ca-b55e5c509ad2"),
                Title = "Ad Hoc"
            };
            var publishScheduled = DateTime.Now.AddDays(1);
            var nextReleaseDate = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            const string releaseName = "2035";
            const TimeIdentifier timePeriodCoverage = TimeIdentifier.January;
            using (var context = InMemoryApplicationDbContext("GetReleaseSummaryAsync"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    adhocReleaseType,
                });

                context.Add(new Publication
                {
                    Id = new Guid("f7da23e2-304a-4b47-a8f5-dba28a554de9"),
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = releaseId,
                            TypeId = adhocReleaseType.Id,
                            Type = adhocReleaseType,
                            TimePeriodCoverage = TimeIdentifier.January,
                            PublishScheduled = publishScheduled,
                            NextReleaseDate = nextReleaseDate,
                            ReleaseName = releaseName,
                            ReleaseSummary = new ReleaseSummary()
                            {
                                Id = new Guid("0071f344-4b99-4010-ab2f-62bf7b0035b0"),
                                Versions = new List<ReleaseSummaryVersion>() {
                                    new ReleaseSummaryVersion()
                                    {    
                                        Id = new Guid("25f43cba-faee-4b0a-a9d4-a3d114a5f6df"),
                                        Created = DateTime.Now,
                                        TypeId = adhocReleaseType.Id,
                                        Type = adhocReleaseType,
                                        PublishScheduled = publishScheduled,
                                        NextReleaseDate = nextReleaseDate,
                                        ReleaseName = releaseName,
                                        TimePeriodCoverage = timePeriodCoverage
                                    }
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleaseSummaryAsync"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);
                
                // Method under test 
                var summaryResult = await releaseService.GetReleaseSummaryAsync(releaseId);
                var summary = summaryResult.Right;
                
                Assert.Equal(publishScheduled, summary.PublishScheduled);
                Assert.Equal(nextReleaseDate, summary.NextReleaseDate);
                Assert.Equal(adhocReleaseType, summary.Type);
                Assert.Equal(releaseName, summary.ReleaseName);
                Assert.Equal(timePeriodCoverage, summary.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public async void GetReleasesForPublicationAsync()
        {
            var (userService, releaseHelper, publishingService, repository) = Mocks();
            
            var addHocReleaseTypeId = new Guid("19b024dc-339c-4e2c-b2ca-b55e5c509ad2");
            var publicationId = new Guid("94af186f-5dbe-4f46-8a8e-f5480ed9f4fc");
            var publishScheduledForFirstRelease = DateTime.Now.AddDays(1);
            var publishScheduledForSecondRelease = DateTime.Now.AddDays(2);
            var firstReleaseId = new Guid("8781ebf8-790c-484b-8a34-19ea501c9c74");
            var secondReleaseId = new Guid("cace48cf-9f08-4471-815c-b3703aade096");
                
            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    new ReleaseType
                    {
                        Id = addHocReleaseTypeId,
                        Title = "Ad Hoc"
                    },
                });
                context.Add(new Publication
                {
                    Id = publicationId,
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = firstReleaseId,
                            TypeId = addHocReleaseTypeId,
                            TimePeriodCoverage = TimeIdentifier.February,
                            PublishScheduled = publishScheduledForFirstRelease,
                            NextReleaseDate = new PartialDate{Day = "01", Month = "01", Year = "2040"},
                            ReleaseName = "2035",
                        },
                        new Release
                        {
                            Id = secondReleaseId,
                            TypeId = addHocReleaseTypeId,
                            TimePeriodCoverage = TimeIdentifier.January,
                            PublishScheduled = publishScheduledForSecondRelease,
                            NextReleaseDate = new PartialDate{Day = "01", Month = "01", Year = "2041"},
                            ReleaseName = "2036"
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync"))
            {
                var releaseService = new ReleaseService(context, MapperForProfile<MappingProfiles>(),
                    publishingService.Object, releaseHelper.Object, userService.Object, repository.Object);

                // Method under test 
                var summary = await releaseService.GetReleasesForPublicationAsync(publicationId);
                Assert.Equal(2, summary.Count);
                Assert.True(summary.Exists(r => r.Id == firstReleaseId && r.TypeId == addHocReleaseTypeId));
                Assert.True(summary.Exists(r => r.Id == firstReleaseId && r.TimePeriodCoverage== TimeIdentifier.February));
                Assert.True(summary.Exists(r => r.Id == firstReleaseId && r.PublishScheduled == publishScheduledForFirstRelease));
                Assert.True(summary.Exists(r => r.Id == firstReleaseId && r.ReleaseName == "2035"));
            }
        }

        private (
            Mock<IUserService> UserService, 
            Mock<IPersistenceHelper<Release,Guid>> ReleaseHelper, 
            Mock<IPublishingService> PublishingService,
            Mock<IReleaseRepository>) Mocks()
        {
            var userService = new Mock<IUserService>();

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases))
                .ReturnsAsync(true);
            
            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<Release>(), SecurityPolicies.CanViewSpecificRelease))
                .ReturnsAsync(true);

            var releaseHelper = new Mock<IPersistenceHelper<Release, Guid>>();

            releaseHelper
                .Setup(s => s.CheckEntityExistsActionResult(It.IsAny<Guid>(), null))
                .ReturnsAsync(new Either<ActionResult, Release>(new Release()));
            
            return (userService, releaseHelper, new Mock<IPublishingService>(), new Mock<IReleaseRepository>());
        }
    }
}