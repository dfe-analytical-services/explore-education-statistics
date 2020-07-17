using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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
            var mocks = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Publication"
            };
            
            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate"))
            {
                context.Add(new ReleaseType {Id = new Guid("484e6b5c-4a0f-47fd-914e-ac4dac5bdd1c"), Title = "Ad Hoc",});
                context.Add(publication);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate"))
            {
                var releaseService = BuildReleaseService(context, mocks);

                var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publication.Id,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = "2050-06-30",
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
                    });

                var publishScheduled = new DateTime(2050, 6, 30, 0, 0, 0, DateTimeKind.Unspecified);

                Assert.Equal("Academic Year 2018/19", result.Result.Right.Title);
                Assert.Null(result.Result.Right.Published);
                Assert.Equal(publishScheduled, result.Result.Right.PublishScheduled);
                Assert.False(result.Result.Right.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, result.Result.Right.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public void CreateReleaseWithTemplate()
        {
            var mocks = Mocks();
            
            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 1",
                Order = 2,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = "Comment 1 Text"
                    },
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = "Comment 2 Text"
                    }
                }
            };

            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 2"
            };
            
            var templateReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f");
            
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
                            Id = templateReleaseId,
                            ReleaseName = "2018",
                            Content = new List<ReleaseContentSection>
                            {
                                new ReleaseContentSection
                                {
                                    ReleaseId = Guid.NewGuid(),
                                    ContentSection = new ContentSection
                                    {
                                        Id = Guid.NewGuid(),
                                        Caption = "Template caption index 0",
                                        Heading = "Template heading index 0",
                                        Type = ContentSectionType.Generic,
                                        Order = 1,
                                        Content = new List<ContentBlock>
                                        {
                                            new HtmlBlock
                                            {
                                                Id = Guid.NewGuid(),
                                                Body = @"<div></div>",
                                                Order = 1,
                                                Comments = new List<Comment>
                                                {
                                                    new Comment
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        Content = "Comment 1 Text"
                                                    },
                                                    new Comment
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        Content = "Comment 2 Text"
                                                    }
                                                }
                                            },
                                            dataBlock1
                                        }
                                    }
                                },
                            },
                            Version = 0,
                            PreviousVersionId = templateReleaseId,
                            ContentBlocks = new List<ReleaseContentBlock>
                            {
                                new ReleaseContentBlock
                                {
                                    ReleaseId = templateReleaseId,
                                    ContentBlock = dataBlock1,
                                    ContentBlockId = dataBlock1.Id,
                                },
                                new ReleaseContentBlock
                                {
                                    ReleaseId = templateReleaseId,
                                    ContentBlock = dataBlock2,
                                    ContentBlockId = dataBlock2.Id,
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                    var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = templateReleaseId,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = "2050-01-01",
                        TypeId = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8")
                    });

                // Do an in depth check of the saved release
                var newRelease = context.Releases
                    .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .Single(r => r.Id == result.Result.Right.Id);

                var contentSections = newRelease.GenericContent.ToList();
                
                Assert.Single(contentSections);
                Assert.Equal("Template caption index 0", contentSections[0].Caption);
                Assert.Equal("Template heading index 0", contentSections[0].Heading);
                Assert.Single(contentSections);
                Assert.Equal(1, contentSections[0].Order);
                // Content should not be copied when create from template
                Assert.Empty(contentSections[0].Content);
                Assert.Empty(contentSections[0].Content.AsReadOnly());

                Assert.Equal(ContentSectionType.ReleaseSummary, newRelease.SummarySection.Type);
                Assert.Equal(ContentSectionType.Headlines, newRelease.HeadlinesSection.Type);
                Assert.Equal(ContentSectionType.KeyStatistics, newRelease.KeyStatisticsSection.Type);
                Assert.Equal(ContentSectionType.KeyStatisticsSecondary, newRelease.KeyStatisticsSecondarySection.Type);
            }
        }

        [Fact]
        public async void LatestReleaseCorrectlyReported()
        {
            var mocks = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            var notLatestRelease = new Release
            {
                Id = new Guid("a941444a-687a-4364-9f7d-d39c35d91b9e"),
                ReleaseName = "2019",
                TimePeriodCoverage = TimeIdentifier.December,
                PublicationId = publication.Id,
                Published = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = new Guid("a941444a-687a-4364-9f7d-d39c35d91b9e")
            };

            var latestRelease = new Release
            {
                Id = new Guid("8909d1b4-78fc-4070-bb3d-90e055f39b39"),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.June,
                PublicationId = publication.Id,
                Published = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = new Guid("8909d1b4-78fc-4070-bb3d-90e055f39b39")
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
                var releaseService = BuildReleaseService(context, mocks);
                var notLatest = (await releaseService.GetReleaseForIdAsync(notLatestRelease.Id)).Right;
                
                Assert.Equal(notLatestRelease.Id, notLatest.Id);
                Assert.False(notLatest.LatestRelease);
            }
            
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                var latest = (await releaseService.GetReleaseForIdAsync(latestRelease.Id)).Right;
                
                Assert.Equal(latestRelease.Id, latest.Id);
                Assert.True(latest.LatestRelease);
            }
        }

        [Fact]
        public async void UpdateRelease()
        {
            var mocks = Mocks();

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
                            Version = 0,
                            PreviousVersionId = releaseId
                        }
                    }
                });
                context.SaveChanges();
            }

            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            var typeEdited = officialStatisticsReleaseType;
            const string releaseNameEdited = "2035";
            const TimeIdentifier timePeriodCoverageEdited = TimeIdentifier.March;
            
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                var edited = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseRequest
                        {
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = nextReleaseDateEdited,
                            TypeId = typeEdited.Id,
                            ReleaseName = releaseNameEdited,
                            TimePeriodCoverage = timePeriodCoverageEdited
                        });

                var publishScheduled = new DateTime(2051, 6, 30, 0, 0, 0, DateTimeKind.Unspecified);

                Assert.Equal(publishScheduled, edited.Right.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, edited.Right.NextReleaseDate);
                Assert.Equal(typeEdited, edited.Right.Type);
                Assert.Equal(releaseNameEdited, edited.Right.ReleaseName);
                Assert.Equal(timePeriodCoverageEdited, edited.Right.TimePeriodCoverage);
            }
        }
        
        [Fact]
        public async void GetReleaseSummaryAsync()
        {
            var mocks = Mocks();
            
            var releaseId = new Guid("5cf345d4-7f7b-425c-8267-de785cfc040b");
            var adhocReleaseType = new ReleaseType
            {
                Id = new Guid("19b024dc-339c-4e2c-b2ca-b55e5c509ad2"),
                Title = "Ad Hoc"
            };
            var publishScheduled = new DateTime(2020, 6, 29, 0, 0, 0).AsStartOfDayUtc();
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
                            Version = 0,
                            PreviousVersionId = releaseId
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleaseSummaryAsync"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                
                // Method under test 
                var summaryResult = await releaseService.GetReleaseSummaryAsync(releaseId);
                var summary = summaryResult.Right;
                
                Assert.Equal(new DateTime(2020, 6, 29, 0, 0, 0), summary.PublishScheduled);
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
            var mocks = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            
            var notLatestRelease = new Release
            {
                Id = new Guid("1cf74d85-2a20-4b2a-a944-6b74f79e56a4"),
                Published = DateTime.UtcNow,
                PublicationId = publication.Id,
                ReleaseName = "2035",
                TimePeriodCoverage = TimeIdentifier.December,
                Version = 0,
                PreviousVersionId = new Guid("1cf74d85-2a20-4b2a-a944-6b74f79e56a4")
            };
            
            var latestReleaseV0 = new Release
            {
                Id = new Guid("7ef22424-a66f-47b9-85b0-50bdf2a622fc"),
                Published = DateTime.UtcNow,
                PublicationId = publication.Id,
                ReleaseName = "2036",
                TimePeriodCoverage = TimeIdentifier.June,
                Version = 0,
                PreviousVersionId = new Guid("7ef22424-a66f-47b9-85b0-50bdf2a622fc")
            };
            
            var latestReleaseV1 = new Release
            {
                Id = new Guid("d301f5b7-a89b-4d7e-b020-53f8631c72b2"),
                Published = DateTime.UtcNow,
                PublicationId = publication.Id,
                ReleaseName = "2036",
                TimePeriodCoverage = TimeIdentifier.June,
                Version = 1,
                PreviousVersionId = new Guid("7ef22424-a66f-47b9-85b0-50bdf2a622fc")
            };
            
            var latestReleaseV2Deleted = new Release
            {
                Id = new Guid("efc6d4bd-9bf4-4179-a1fb-88cdfa2e19f6"),
                Published = DateTime.UtcNow,
                PublicationId = publication.Id,
                ReleaseName = "2036",
                TimePeriodCoverage = TimeIdentifier.June,
                Version = 2,
                PreviousVersionId = new Guid("d301f5b7-a89b-4d7e-b020-53f8631c72b2"),
                SoftDeleted = true
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
                    notLatestRelease, latestReleaseV0, latestReleaseV1, latestReleaseV2Deleted
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                var latest = releaseService.GetLatestReleaseAsync(publication.Id).Result.Right;

                Assert.NotNull(latest);
                Assert.Equal(latestReleaseV1.Id, latest.Id);
                Assert.Equal("June 2036", latest.Title);
            }
        }
        
        [Fact]
        public void DeleteReleaseAsync()
        {
            var mocks = Mocks();

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            
            var release = new Release
            {
                Id = new Guid("defb0361-5084-43e8-a570-4841657041e2"),
                PublicationId = publication.Id,
                Version = 0,
                PreviousVersionId = new Guid("defb0361-5084-43e8-a570-4841657041e2")
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
                Id = new Guid("863cf537-c9cd-48d9-9874-cc222bdab0a7"),
                PublicationId = publication.Id,
                Version = 0,
                PreviousVersionId = new Guid("863cf537-c9cd-48d9-9874-cc222bdab0a7")
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
                var releaseService = BuildReleaseService(context, mocks);
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
            var mocks = Mocks();

            var release = new Release
            {
                Id = new Guid("33c2b77a-6a70-485d-ada3-fc99edac95dd"),
                Status = ReleaseStatus.Approved,
                Version = 0,
                PreviousVersionId = new Guid("33c2b77a-6a70-485d-ada3-fc99edac95dd")
            };

            using (var context = InMemoryApplicationDbContext("PublishReleaseAsync"))
            {
                context.Add(release);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PublishReleaseAsync"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                var result = releaseService.PublishReleaseAsync(release.Id).Result.Right;

                mocks.PublishingService.Verify(mock => mock.QueueValidateReleaseAsync(release.Id, true), Times.Once());

                Assert.True(result);
            }
        }

        [Fact]
        public void PublishReleaseContentAsync()
        {
            var mocks = Mocks();

            var release = new Release
            {
                Id = new Guid("af032e3c-67c2-4562-9717-9a305a468263"),
                Status = ReleaseStatus.Approved,
                Version = 0,
                PreviousVersionId = new Guid("af032e3c-67c2-4562-9717-9a305a468263")
            };

            using (var context = InMemoryApplicationDbContext("PublishReleaseContentAsync"))
            {
                context.Add(release);
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PublishReleaseContentAsync"))
            {
                var releaseService = BuildReleaseService(context, mocks);
                var result = releaseService.PublishReleaseAsync(release.Id).Result.Right;

                mocks.PublishingService.Verify(mock => mock.QueueValidateReleaseAsync(release.Id, true), Times.Once());

                Assert.True(result);
            }
        }

        private static ReleaseService BuildReleaseService(ContentDbContext context,
            (Mock<IUserService> userService,
                Mock<IPublishingService> publishingService,
                Mock<IReleaseRepository> releaseRepository,
                Mock<ISubjectService> subjectService,
                Mock<ITableStorageService> tableStorageService,
                Mock<IFileStorageService> fileStorageService,
                Mock<IImportStatusService> importStatusService,
                Mock<IFootnoteService> footnoteService,
                Mock<StatisticsDbContext> statisticsDbContext,
                Mock<IDataBlockService> dataBlockService,
                Mock<IReleaseSubjectService> releaseSubjectService) mocks)
        {
            var (userService, publishingService, releaseRepository, subjectService,
                tableStorageService, fileStorageService, importStatusService, footnoteService, statisticsDbContext,
                dataBlockService, releaseSubjectService) = mocks;

            return new ReleaseService(
                context, AdminMapper(), publishingService.Object, new PersistenceHelper<ContentDbContext>(context),
                userService.Object, releaseRepository.Object, subjectService.Object, tableStorageService.Object,
                fileStorageService.Object, importStatusService.Object, footnoteService.Object,
                statisticsDbContext.Object, dataBlockService.Object, releaseSubjectService.Object, new SequentialGuidGenerator());
        }

        private (Mock<IUserService> UserService,
            Mock<IPublishingService> PublishingService,
            Mock<IReleaseRepository> ReleaseRepository,
            Mock<ISubjectService> SubjectService,
            Mock<ITableStorageService> TableStorageService,
            Mock<IFileStorageService> FileStorageService,
            Mock<IImportStatusService> ImportStatusService,
            Mock<IFootnoteService> FootnoteService,
            Mock<StatisticsDbContext> StatisticsDbContext,
            Mock<IDataBlockService> DataBlockService,
            Mock<IReleaseSubjectService> ReleaseSubjectService) Mocks()
        {
            var userService = MockUtils.AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            return (
                userService,
                new Mock<IPublishingService>(), 
                new Mock<IReleaseRepository>(),
                new Mock<ISubjectService>(), 
                new Mock<ITableStorageService>(), 
                new Mock<IFileStorageService>(),
                new Mock<IImportStatusService>(),
                new Mock<IFootnoteService>(),
                new Mock<StatisticsDbContext>(),
                new Mock<IDataBlockService>(),
                new Mock<IReleaseSubjectService>());
        }
    }
}