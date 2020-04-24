using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();
        
        [Fact]
        public void CreateReleaseNoTemplate()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService, dataBlockService) = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Publication"
            };
            
            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate"))
            {
                context.Add(new ReleaseType {Id = new Guid("484e6b5c-4a0f-47fd-914e-ac4dac5bdd1c"), Title = "Ad Hoc"});
                context.Add(publication);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(), 
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);

                var publishScheduled = new DateTime(2050, 6, 30, 14, 0, 0);
                var publishScheduledMidnight = new DateTime(2050, 6, 30, 0, 0, 0);
                
                var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publication.Id,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = publishScheduled,
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
                    });

                Assert.Equal("Academic Year 2018/19", result.Result.Right.Title);
                Assert.Null(result.Result.Right.Published);
                Assert.Equal(publishScheduledMidnight, result.Result.Right.PublishScheduled);
                Assert.False(result.Result.Right.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, result.Result.Right.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public void CreateReleaseWithTemplate()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService, dataBlockService) = Mocks();

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
                                            new MarkDownBlock
                                            {
                                                Id = new Guid("e2b96bea-fbbb-4089-ad9c-fecba58ee054"),
                                                Body = SampleMarkDownContent.Content[new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f")]
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
                                            new MarkDownBlock
                                            {
                                                Id = new Guid("e4e88dd8-eef7-4ed5-a3f2-a92e5a328e05"),
                                                Body = SampleMarkDownContent.Content[new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f")]
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
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);
                
                // Service method under test
                var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = new DateTime(2050,6,30,14,0,0),
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
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService,
                importStatusService, footnoteService, dataBlockService) = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            var notLatestRelease = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2019",
                TimePeriodCoverage = TimeIdentifier.December,
                PublicationId = publication.Id,
                Published = DateTime.UtcNow
            };

            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.June,
                PublicationId = publication.Id,
                Published = DateTime.UtcNow
            };

            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                context.Add(publication);
                context.AddRange(new List<Release>
                {
                    notLatestRelease, latestRelease
                });
                context.SaveChanges();
            }

            // Note that we use different contexts for each method call - this is to avoid misleadingly optimistic
            // loading of the entity graph as we go.
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);
                
                // Method under test
                var notLatest = (await releaseService.GetReleaseForIdAsync(notLatestRelease.Id)).Right;
                Assert.Equal(notLatestRelease.Id, notLatest.Id);
                Assert.False(notLatest.LatestRelease);
            }
            
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);
                
                // Method under test
                var latest = (await releaseService.GetReleaseForIdAsync(latestRelease.Id)).Right;
                Assert.Equal(latestRelease.Id, latest.Id);
                Assert.True(latest.LatestRelease);
            }
        }

        [Fact]
        public async void EditReleaseSummary()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService,
                importStatusService, footnoteService, dataBlockService) = Mocks();

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
                        }
                    }
                });
                context.SaveChanges();
            }
            
            var publishScheduledEdited = new DateTime(2051, 6, 30, 14, 0, 0);
            var publishScheduledEditedMidnight = new DateTime(2051, 6, 30, 0, 0, 0);
            
            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            var typeEdited = officialStatisticsReleaseType;
            const string releaseNameEdited = "2035";
            const TimeIdentifier timePeriodCoverageEdited = TimeIdentifier.March;
            
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);
                
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

                Assert.Equal(publishScheduledEditedMidnight, edited.Right.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, edited.Right.NextReleaseDate);
                Assert.Equal(typeEdited, edited.Right.Type);
                Assert.Equal(releaseNameEdited, edited.Right.ReleaseName);
                Assert.Equal(timePeriodCoverageEdited, edited.Right.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public async void GetReleaseSummaryAsync()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService,
                importStatusService, footnoteService, dataBlockService) = Mocks();
            
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
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleaseSummaryAsync"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);
                
                // Method under test 
                var summaryResult = await releaseService.GetReleaseSummaryAsync(releaseId);
                var summary = summaryResult.Right;
                
                Assert.Equal(publishScheduled, summary.PublishScheduled);
                Assert.Equal(nextReleaseDate, summary.NextReleaseDate);
                Assert.Equal(adhocReleaseType, summary.Type);
                Assert.Equal(releaseName, summary.ReleaseName);
                Assert.Equal(timePeriodCoverage, summary.TimePeriodCoverage);
                Assert.Equal("2035", summary.YearTitle);
            }
        }
        
        [Fact]
        public void GetLatestReleaseAsync()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService,
                importStatusService, footnoteService, dataBlockService) = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            
            var notLatestRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow,
                PublicationId = publication.Id,
                ReleaseName = "2035",
                TimePeriodCoverage = TimeIdentifier.December
            };
            
            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow,
                PublicationId = publication.Id,
                ReleaseName = "2036",
                TimePeriodCoverage = TimeIdentifier.June
            };

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync"))
            {
                context.Add(new UserReleaseRole
                {
                    UserId = _userId,
                    ReleaseId = notLatestRelease.Id
                });

                context.Add(publication);
                context.AddRange(new List<Release>
                {
                    notLatestRelease, latestRelease
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object,
                    footnoteService.Object, dataBlockService.Object);

                // Method under test 
                var latest = releaseService.GetLatestReleaseAsync(publication.Id).Result.Right;
                Assert.NotNull(latest);
                Assert.Equal(latestRelease.Id, latest.Id);
                Assert.Equal("June 2036", latest.Title);
            }
        }
        
        [Fact]
        public void DeleteReleaseAsync()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService, importStatusService, footnoteService, dataBlockService) = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id
            };
            
            var userReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                ReleaseId = release.Id
            };

            var userReleaseInvite = new UserReleaseInvite
            {
                Id = Guid.NewGuid(),
                ReleaseId = release.Id
            };
            
            var anotherRelease = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id
            };
            
            var anotherUserReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                ReleaseId = anotherRelease.Id
            };

            var anotherUserReleaseInvite = new UserReleaseInvite
            {
                Id = Guid.NewGuid(),
                ReleaseId = anotherRelease.Id
            };
            
            using (var context = InMemoryApplicationDbContext("DeleteReleaseAsync"))
            {
                context.Add(publication);
                context.AddRange(release, anotherRelease);
                context.AddRange(userReleaseRole, anotherUserReleaseRole);
                context.AddRange(userReleaseInvite, anotherUserReleaseInvite);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("DeleteReleaseAsync"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object, repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object, importStatusService.Object, footnoteService.Object, dataBlockService.Object);

                // Method under test 
                var result = releaseService.DeleteReleaseAsync(release.Id).Result.Right;
                Assert.True(result);
            }
            
            using (var context = InMemoryApplicationDbContext("DeleteReleaseAsync"))
            {
                // assert that soft-deleted entities are no longer discoverable by default
                var unableToFindDeletedRelease = context
                    .Releases
                    .FirstOrDefault(r => r.Id == release.Id);
                
                Assert.Null(unableToFindDeletedRelease);
                
                var unableToFindDeletedReleaseRole = context
                    .UserReleaseRoles
                    .FirstOrDefault(r => r.Id == userReleaseRole.Id);
                
                Assert.Null(unableToFindDeletedReleaseRole);

                var unableToFindDeletedReleaseInvite = context
                    .UserReleaseInvites
                    .FirstOrDefault(r => r.Id == userReleaseInvite.Id);
                
                Assert.Null(unableToFindDeletedReleaseInvite);
                
                // assert that soft-deleted entities do not appear via references from other entities by default
                var publicationWithoutDeletedRelease = context
                    .Publications
                    .Include(p => p.Releases)
                    .AsNoTracking()
                    .First(p => p.Id == publication.Id);
                Assert.Single(publicationWithoutDeletedRelease.Releases);
                Assert.Equal(anotherRelease.Id, publicationWithoutDeletedRelease.Releases[0].Id);

                // assert that soft-deleted entities have had their soft-deleted flag set to true
                var updatedRelease = context
                    .Releases
                    .IgnoreQueryFilters()
                    .First(r => r.Id == release.Id);
                
                Assert.True(updatedRelease.SoftDeleted);

                var updatedReleaseRole = context
                    .UserReleaseRoles
                    .IgnoreQueryFilters()
                    .First(r => r.Id == userReleaseRole.Id);
                
                Assert.True(updatedReleaseRole.SoftDeleted);

                var updatedReleaseInvite = context
                    .UserReleaseInvites
                    .IgnoreQueryFilters()
                    .First(r => r.Id == userReleaseInvite.Id);
                
                Assert.True(updatedReleaseInvite.SoftDeleted);
                
                // assert that soft-deleted entities appear via references from other entities when explicitly searched for
                var publicationWithDeletedRelease = context
                    .Publications
                    .Include(p => p.Releases)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .First(p => p.Id == publication.Id);
                Assert.Equal(2, publicationWithDeletedRelease.Releases.Count);
                Assert.Equal(updatedRelease.Id, publicationWithDeletedRelease.Releases[0].Id);
                Assert.Equal(anotherRelease.Id, publicationWithDeletedRelease.Releases[1].Id);
                Assert.True(publicationWithDeletedRelease.Releases[0].SoftDeleted);
                Assert.False(publicationWithDeletedRelease.Releases[1].SoftDeleted);
                
                // assert that other entities were not accidentally soft-deleted
                var retrievedAnotherReleaseRole = context
                    .UserReleaseRoles
                    .First(r => r.Id == anotherUserReleaseRole.Id);
                
                Assert.False(retrievedAnotherReleaseRole.SoftDeleted);

                var retrievedAnotherReleaseInvite = context
                    .UserReleaseInvites
                    .First(r => r.Id == anotherUserReleaseInvite.Id);
                
                Assert.False(retrievedAnotherReleaseInvite.SoftDeleted);
            }
        }

        [Fact]
        public void PublishReleaseAsync()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService,
                importStatusService, footnoteService, dataBlockService) = Mocks();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Approved
            };

            using (var context = InMemoryApplicationDbContext("PublishReleaseAsync"))
            {
                context.Add(release);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PublishReleaseAsync"))
            {
                var releaseService = new ReleaseService(context,
                    AdminMapper(),
                    publishingService.Object,
                    new PersistenceHelper<ContentDbContext>(context), userService.Object,
                    repository.Object,
                    subjectService.Object,
                    tableStorageService.Object,
                    fileStorageService.Object,
                    importStatusService.Object,
                    footnoteService.Object,
                    dataBlockService.Object);

                var result = releaseService.PublishReleaseAsync(release.Id).Result.Right;

                publishingService.Verify(mock => mock.QueueValidateReleaseAsync(release.Id, true), Times.Once());

                Assert.True(result);
            }
        }

        [Fact]
        public void PublishReleaseContentAsync()
        {
            var (userService, _, publishingService, repository, subjectService, tableStorageService, fileStorageService,
                importStatusService, footnoteService, dataBlockService) = Mocks();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Status = ReleaseStatus.Approved
            };

            using (var context = InMemoryApplicationDbContext("PublishReleaseContentAsync"))
            {
                context.Add(release);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PublishReleaseContentAsync"))
            {
                var releaseService = new ReleaseService(context, AdminMapper(),
                    publishingService.Object, new PersistenceHelper<ContentDbContext>(context), userService.Object,
                    repository.Object,
                    subjectService.Object, tableStorageService.Object, fileStorageService.Object,
                    importStatusService.Object,
                    footnoteService.Object,
                    dataBlockService.Object);

                var result = releaseService.PublishReleaseAsync(release.Id).Result.Right;

                publishingService.Verify(mock => mock.QueueValidateReleaseAsync(release.Id, true), Times.Once());

                Assert.True(result);
            }
        }

        private (
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IPublishingService>,
            Mock<IReleaseRepository>,
            Mock<ISubjectService>,
            Mock<ITableStorageService>,
            Mock<IFileStorageService>,
            Mock<IImportStatusService>,
            Mock<IFootnoteService>,
            Mock<IDataBlockService>) Mocks()
        {
            var userService = MockUtils.AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall<ContentDbContext, Release>(persistenceHelper);
            MockUtils.SetupCall<ContentDbContext, Publication>(persistenceHelper);
            
            return (
                userService, 
                persistenceHelper, 
                new Mock<IPublishingService>(), 
                new Mock<IReleaseRepository>(),
                new Mock<ISubjectService>(), 
                new Mock<ITableStorageService>(), 
                new Mock<IFileStorageService>(),
                new Mock<IImportStatusService>(),
                new Mock<IFootnoteService>(),
                new Mock<IDataBlockService>());
        }
    }
}