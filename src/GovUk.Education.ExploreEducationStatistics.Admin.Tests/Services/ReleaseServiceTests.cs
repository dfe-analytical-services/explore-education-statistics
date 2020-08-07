using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public async void CreateReleaseNoTemplate()
        {
            var publication = new Publication
            {
                Title = "Publication"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new ReleaseType
                    {
                        Title = "Ad Hoc",
                    }
                );
                context.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var releaseService = BuildReleaseService(context, mocks);

                var result = releaseService.CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publication.Id,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = "2050-06-30",
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
                    }
                );

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

            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new ReleaseType
                    {
                        Id = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8"), Title = "Ad Hoc",
                    }
                );
                context.Add(
                    new Publication
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
                    }
                );
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
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
                    }
                );

                // Do an in depth check of the saved release
                var newRelease = context.Releases
                    .Include(r => r.Content)
                    .ThenInclude(join => @join.ContentSection)
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                context.AddRange(
                    new List<Release>
                    {
                        notLatestRelease, latestRelease
                    }
                );
                context.SaveChanges();
            }

            var mocks = Mocks();

            // Note that we use different contexts for each method call - this is to avoid misleadingly optimistic
            // loading of the entity graph as we go.
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context, mocks);
                var notLatest = (await releaseService.GetReleaseForIdAsync(notLatestRelease.Id)).Right;

                Assert.Equal(notLatestRelease.Id, notLatest.Id);
                Assert.False(notLatest.LatestRelease);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
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
            var releaseId = Guid.NewGuid();

            var adHocReleaseType = new ReleaseType
            {
                Title = "Ad Hoc"
            };

            var officialStatisticsReleaseType = new ReleaseType
            {
                Title = "Official Statistics"
            };

            var newPublication = new Publication
            {
                Title = "New publication"
            };

            var release = new Release
            {
                Id = releaseId,
                Type = adHocReleaseType,
                Publication = new Publication
                {
                    Title = "Old publication"
                },
                ReleaseName = "2030",
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate
                {
                    Day = "15", Month = "6", Year = "2039"
                },
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(adHocReleaseType, officialStatisticsReleaseType);

                context.Add(newPublication);
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var releaseService = BuildReleaseService(context, mocks);

                var nextReleaseDateEdited = new PartialDate
                {
                    Day = "1", Month = "1", Year = "2040"
                };

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = newPublication.Id,
                            PublishScheduled = "2051-06-30",
                            NextReleaseDate = nextReleaseDateEdited,
                            TypeId = officialStatisticsReleaseType.Id,
                            ReleaseName = "2035",
                            TimePeriodCoverage = TimeIdentifier.March
                        }
                    );

                Assert.True(result.IsRight);

                Assert.Equal(newPublication.Id, result.Right.PublicationId);
                Assert.Equal(new DateTime(2051, 6, 30, 0, 0, 0, DateTimeKind.Unspecified), result.Right.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, result.Right.NextReleaseDate);
                Assert.Equal(officialStatisticsReleaseType, result.Right.Type);
                Assert.Equal("2035", result.Right.ReleaseName);
                Assert.Equal(TimeIdentifier.March, result.Right.TimePeriodCoverage);

                var saved = await context.Releases.FindAsync(result.Right.Id);

                Assert.Equal(newPublication.Id, saved.PublicationId);
                Assert.Equal(new DateTime(2051, 6, 29, 23, 0, 0, DateTimeKind.Utc), saved.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, saved.NextReleaseDate);
                Assert.Equal(officialStatisticsReleaseType, saved.Type);
                Assert.Equal("2035-march", saved.Slug);
                Assert.Equal("2035", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.March, saved.TimePeriodCoverage);
            }
        }

        [Fact]
        public async void UpdateRelease_FailsNonUniqueSlug()
        {
            var releaseType = new ReleaseType
            {
                Title = "Ad Hoc"
            };

            var publication = new Publication
            {
                Title = "Old publication"
            };

            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId,
                Type = releaseType,
                Publication = publication,
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = releaseId
            };

            var otherReleaseId = Guid.NewGuid();
            var otherRelease = new Release
            {
                Id = otherReleaseId,
                Type = releaseType,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = otherReleaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(releaseType);
                await context.AddRangeAsync(release, otherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var releaseService = BuildReleaseService(context, mocks);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = release.PublicationId,
                            PublishScheduled = "2051-06-30",
                            TypeId = releaseType.Id,
                            ReleaseName = "2035",
                            TimePeriodCoverage = TimeIdentifier.CalendarYear
                        }
                    );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateRelease_Amendment_NoUniqueSlugFailure()
        {
            var releaseType = new ReleaseType
            {
                Title = "Ad Hoc"
            };

            var publication = new Publication
            {
                Title = "Old publication"
            };

            var initialReleaseId = Guid.NewGuid();
            var initialRelease = new Release
            {
                Id = initialReleaseId,
                Type = releaseType,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = initialReleaseId
            };

            var amendedRelease = new Release
            {
                Type = releaseType,
                Publication = publication,
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 1,
                PreviousVersionId = initialRelease.Id
            };


            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(releaseType);
                await context.AddRangeAsync(amendedRelease, initialRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var releaseService = BuildReleaseService(context, mocks);

                var result = await releaseService
                    .UpdateRelease(
                        amendedRelease.Id,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = amendedRelease.PublicationId,
                            PublishScheduled = "2051-06-30",
                            TypeId = releaseType.Id,
                            ReleaseName = "2035",
                            TimePeriodCoverage = TimeIdentifier.CalendarYear
                        }
                    );

                Assert.True(result.IsRight);
                Assert.Equal("2035", result.Right.ReleaseName);
                Assert.Equal(TimeIdentifier.CalendarYear, result.Right.TimePeriodCoverage);
            }
        }

        [Fact]
        public async void UpdateRelease_Approved_FailsDataFilesNotUploaded()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId,
                Type = new ReleaseType
                {
                    Title = "Ad Hoc"
                },
                Publication = new Publication
                {
                    Title = "Old publication",
                },
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var cloudTableMock = TableStorageTestUtils.MockCloudTable();

                cloudTableMock
                    .Setup(table => table.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<DatafileImport>>(), null))
                    .ReturnsAsync(
                        TableStorageTestUtils.CreateTableQuerySegment(
                            new List<DatafileImport>
                            {
                                new DatafileImport()
                            }
                        )
                    );

                mocks.TableStorageService
                    .Setup(service => service.GetTableAsync(DatafileImportsTableName, true))
                    .ReturnsAsync(cloudTableMock.Object);

                var releaseService = BuildReleaseService(context, mocks);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = release.PublicationId,
                            PublishScheduled = "2051-06-30",
                            TypeId = release.Type.Id,
                            ReleaseName = "2030",
                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                            Status = ReleaseStatus.Approved
                        }
                    );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
                Assert.Equal("ALL_DATAFILES_UPLOADED_MUST_BE_COMPLETE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateRelease_Approved_FailsMethodologyNotApproved()
        {
            var releaseId = Guid.NewGuid();

            var release = new Release
            {
                Id = releaseId,
                Type = new ReleaseType
                {
                    Title = "Ad Hoc"
                },
                Publication = new Publication
                {
                    Title = "Old publication",
                    Methodology = new Methodology
                    {
                        Title = "Test methodology",
                        Status = MethodologyStatus.Draft
                    }
                },
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var cloudTableMock = TableStorageTestUtils.MockCloudTable();

                cloudTableMock
                    .Setup(table => table.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<DatafileImport>>(), null))
                    .ReturnsAsync(TableStorageTestUtils.CreateTableQuerySegment(new List<DatafileImport>()));

                mocks.TableStorageService
                    .Setup(service => service.GetTableAsync(DatafileImportsTableName, true))
                    .ReturnsAsync(cloudTableMock.Object);

                var releaseService = BuildReleaseService(context, mocks);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = release.PublicationId,
                            PublishScheduled = "2051-06-30",
                            TypeId = release.Type.Id,
                            ReleaseName = "2030",
                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                            Status = ReleaseStatus.Approved
                        }
                    );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
                Assert.Equal("METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateRelease_Approved_FailsChangingToDraft()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId,
                Type = new ReleaseType
                {
                    Title = "Ad Hoc"
                },
                Publication = new Publication
                {
                    Title = "Old publication",
                },
                ReleaseName = "2030",
                Slug = "2030",
                Published = DateTime.Now,
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var cloudTableMock = TableStorageTestUtils.MockCloudTable();

                cloudTableMock
                    .Setup(table => table.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<DatafileImport>>(), null))
                    .ReturnsAsync(TableStorageTestUtils.CreateTableQuerySegment(new List<DatafileImport>()));

                mocks.TableStorageService
                    .Setup(service => service.GetTableAsync(DatafileImportsTableName, true))
                    .ReturnsAsync(cloudTableMock.Object);
                var releaseService = BuildReleaseService(context, mocks);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = release.PublicationId,
                            PublishScheduled = "2051-06-30",
                            TypeId = release.Type.Id,
                            ReleaseName = "2030",
                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                            Status = ReleaseStatus.Draft
                        }
                    );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
                Assert.Equal("PUBLISHED_RELEASE_CANNOT_BE_UNAPPROVED", details.Errors[""].First());
            }
        }

        [Fact]
        public async void UpdateRelease_Approved_FailsNoPublishScheduledDate()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId,
                Type = new ReleaseType
                {
                    Title = "Ad Hoc"
                },
                Publication = new Publication
                {
                    Title = "Old publication",
                },
                ReleaseName = "2030",
                Slug = "2030",
                Published = DateTime.Now,
                PublishScheduled = DateTime.UtcNow,
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();
            }


            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var cloudTableMock = TableStorageTestUtils.MockCloudTable();

                cloudTableMock
                    .Setup(table => table.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<DatafileImport>>(), null))
                    .ReturnsAsync(TableStorageTestUtils.CreateTableQuerySegment(new List<DatafileImport>()));

                mocks.TableStorageService
                    .Setup(service => service.GetTableAsync(DatafileImportsTableName, true))
                    .ReturnsAsync(cloudTableMock.Object);
                var releaseService = BuildReleaseService(context, mocks);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new UpdateReleaseViewModel
                        {
                            PublicationId = release.PublicationId,
                            TypeId = release.Type.Id,
                            ReleaseName = "2030",
                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                            Status = ReleaseStatus.Approved,
                            PublishMethod = PublishMethod.Scheduled
                        }
                    );

                Assert.True(result.IsLeft);
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
                Assert.Equal("APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE", details.Errors[""].First());
            }
        }

        [Fact]
        public async void GetReleaseSummaryAsync()
        {
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.AddRange(
                    new List<ReleaseType>
                    {
                        adhocReleaseType,
                    }
                );
                context.Add(
                    new Publication
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
                    }
                );
                context.SaveChanges();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
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
        public async void GetLatestReleaseAsync()
        {
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new UserReleaseRole
                    {
                        UserId = _userId,
                        ReleaseId = notLatestRelease.Id
                    }
                );

                context.Add(publication);
                context.AddRange(
                    new List<Release>
                    {
                        notLatestRelease, latestReleaseV0, latestReleaseV1, latestReleaseV2Deleted
                    }
                );
                context.SaveChanges();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var releaseService = BuildReleaseService(context, mocks);

                var latest = releaseService.GetLatestReleaseAsync(publication.Id).Result.Right;

                Assert.NotNull(latest);
                Assert.Equal(latestReleaseV1.Id, latest.Id);
                Assert.Equal("June 2036", latest.Title);
            }
        }

        [Fact]
        public async void DeleteReleaseAsync()
        {
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

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                context.AddRange(release, anotherRelease);
                context.AddRange(userReleaseRole, anotherUserReleaseRole);
                context.AddRange(userReleaseInvite, anotherUserReleaseInvite);
                context.SaveChanges();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var mocks = Mocks();
                var releaseService = BuildReleaseService(context, mocks);

                var result = releaseService.DeleteReleaseAsync(release.Id).Result.Right;
                Assert.True(result);

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

        private static ReleaseService BuildReleaseService(
            ContentDbContext context,
            (Mock<IUserService> userService,
                Mock<IPublishingService> publishingService,
                Mock<IReleaseRepository> releaseRepository,
                Mock<ISubjectService> subjectService,
                Mock<ITableStorageService> tableStorageService,
                Mock<IReleaseFilesService> fileStorageService,
                Mock<IImportStatusService> importStatusService,
                Mock<IFootnoteService> footnoteService,
                Mock<StatisticsDbContext> statisticsDbContext,
                Mock<IDataBlockService> dataBlockService,
                Mock<IReleaseSubjectService> releaseSubjectService) mocks)
        {
            var (
                userService,
                publishingService,
                releaseRepository,
                subjectService,
                tableStorageService,
                fileStorageService,
                importStatusService,
                footnoteService,
                statisticsDbContext,
                dataBlockService,
                releaseSubjectService) = mocks;
            return new ReleaseService(
                context,
                AdminMapper(),
                publishingService.Object,
                new PersistenceHelper<ContentDbContext>(context),
                userService.Object,
                releaseRepository.Object,
                subjectService.Object,
                tableStorageService.Object,
                fileStorageService.Object,
                importStatusService.Object,
                footnoteService.Object,
                statisticsDbContext.Object,
                dataBlockService.Object,
                releaseSubjectService.Object,
                new SequentialGuidGenerator()
            );
        }

        private (Mock<IUserService> UserService,
            Mock<IPublishingService> PublishingService,
            Mock<IReleaseRepository> ReleaseRepository,
            Mock<ISubjectService> SubjectService,
            Mock<ITableStorageService> TableStorageService,
            Mock<IReleaseFilesService> FileStorageService,
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
                new Mock<IReleaseFilesService>(),
                new Mock<IImportStatusService>(),
                new Mock<IFootnoteService>(),
                new Mock<StatisticsDbContext>(),
                new Mock<IDataBlockService>(),
                new Mock<IReleaseSubjectService>());
        }
    }
}