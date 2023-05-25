#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static Moq.MockBehavior;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {
        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public async Task CreateReleaseNoTemplate()
        {
            var publication = new Publication
            {
                Title = "Publication",
                Contact = new Contact(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = (await releaseService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = publication.Id,
                        Year = 2018,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics
                    }
                )).AssertRight();

                Assert.Equal("Academic year 2018/19", result.Title);
                Assert.Equal("2018/19", result.YearTitle);
                Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriodCoverage);
                Assert.Equal(ReleaseType.OfficialStatistics, result.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, result.ApprovalStatus);

                Assert.False(result.Amendment);
                Assert.False(result.LatestRelease); // Most recent - but not published yet.
                Assert.False(result.Live);
                Assert.Null(result.NextReleaseDate);
                Assert.Null(result.PublishScheduled);
                Assert.Null(result.Published);
                Assert.False(result.NotifySubscribers);
                Assert.False(result.UpdatePublishedDate);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var actual = await context.Releases
                    .SingleAsync(r => r.PublicationId == publication.Id);

                Assert.Equal(2018, actual.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actual.TimePeriodCoverage);
                Assert.Equal(ReleaseType.OfficialStatistics, actual.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, actual.ApprovalStatus);
                Assert.Equal(0, actual.Version);

                Assert.Null(actual.PreviousVersionId);
                Assert.Null(actual.PublishScheduled);
                Assert.Null(actual.Published);
                Assert.Null(actual.NextReleaseDate);
                Assert.Null(actual.NotifiedOn);
                Assert.False(actual.NotifySubscribers);
                Assert.False(actual.UpdatePublishedDate);
            }
        }

        [Fact]
        public async Task CreateReleaseWithTemplate()
        {
            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 1",
                Order = 2,
                Comments = new List<Comment>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Content = "Comment 1 Text"
                    },
                    new()
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

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(
                    new Publication
                    {
                        Id = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        Title = "Publication",
                        Contact = new Contact(),
                        Releases = new List<Release>
                        {
                            new() // Template release
                            {
                                Id = templateReleaseId,
                                ReleaseName = "2018",
                                Content = new List<ReleaseContentSection>
                                {
                                    new()
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
                                                        new()
                                                        {
                                                            Id = Guid.NewGuid(),
                                                            Content = "Comment 1 Text"
                                                        },
                                                        new()
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
                                    new()
                                    {
                                        ReleaseId = templateReleaseId,
                                        ContentBlock = dataBlock1,
                                        ContentBlockId = dataBlock1.Id,
                                    },
                                    new()
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
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = releaseService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = templateReleaseId,
                        Year = 2018,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics
                    }
                );

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
                Assert.Equal(ContentSectionType.KeyStatisticsSecondary, newRelease.KeyStatisticsSecondarySection.Type);
            }
        }

        [Fact]
        public async Task RemoveDataFiles()
        {
            var release = new Release
            {
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var file = new File
            {
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(file);
                await contentDbContext.SaveChangesAsync();
            }

            var cacheService = new Mock<IBlobCacheService>(Strict);
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
            var subjectRepository = new Mock<ISubjectRepository>(Strict);
            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);

            cacheService
                .Setup(service => service.DeleteItem(new PrivateSubjectMetaCacheKey(release.Id, subject.Id)))
                .Returns(Task.CompletedTask);

            dataBlockService.Setup(service => service.GetDeletePlan(release.Id, subject))
                .ReturnsAsync(new DeleteDataBlockPlan());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlan>()))
                .ReturnsAsync(Unit.Instance);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.COMPLETE
                });

            footnoteRepository.Setup(service => service.GetFootnotes(release.Id, subject.Id))
                .ReturnsAsync(new List<Footnote>());

            subjectRepository.Setup(service => service.Find(subject.Id)).ReturnsAsync(subject);

            releaseDataFileService.Setup(service => service.Delete(release.Id, file.Id, false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service => service.SoftDeleteReleaseSubject(release.Id, subject.Id))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    cacheService: cacheService.Object,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                VerifyAllMocks(cacheService,
                    dataBlockService,
                    dataImportService,
                    footnoteRepository,
                    subjectRepository,
                    releaseDataFileService,
                    releaseSubjectRepository
                );

                result.AssertRight();
            }
        }

        [Fact]
        public async Task RemoveDataFiles_FileImporting()
        {
            var release = new Release
            {
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var file = new File
            {
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(file);
                await contentDbContext.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.STAGE_1
                });

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataImportService: dataImportService.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                VerifyAllMocks(dataImportService);

                result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
            }
        }

        [Fact]
        public async Task RemoveDataFiles_ReplacementExists()
        {
            var release = new Release
            {
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var file = new File
            {
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = file
            };

            file.ReplacedBy = replacementFile;

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(file, replacementFile);
                await contentDbContext.SaveChangesAsync();
            }

            var cacheService = new Mock<IBlobCacheService>(Strict);
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
            var subjectRepository = new Mock<ISubjectRepository>(Strict);
            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);

            cacheService.Setup(service => service.DeleteItem(new PrivateSubjectMetaCacheKey(release.Id, subject.Id)))
                .Returns(Task.CompletedTask);
            cacheService.Setup(service =>
                    service.DeleteItem(new PrivateSubjectMetaCacheKey(release.Id, replacementSubject.Id)))
                .Returns(Task.CompletedTask);

            dataBlockService.Setup(service =>
                    service.GetDeletePlan(release.Id, It.IsIn(subject, replacementSubject)))
                .ReturnsAsync(new DeleteDataBlockPlan());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlan>()))
                .ReturnsAsync(Unit.Instance);

            dataImportService.Setup(service => service.GetImport(It.IsIn(file.Id, replacementFile.Id)))
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.COMPLETE
                });

            footnoteRepository.Setup(service =>
                    service.GetFootnotes(release.Id, It.IsIn(subject.Id, replacementSubject.Id)))
                .ReturnsAsync(new List<Footnote>());

            subjectRepository.Setup(service => service.Find(subject.Id)).ReturnsAsync(subject);
            subjectRepository.Setup(service => service.Find(replacementSubject.Id)).ReturnsAsync(replacementSubject);

            releaseDataFileService
                .Setup(service => service.Delete(release.Id, It.IsIn(file.Id, replacementFile.Id), false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service =>
                    service.SoftDeleteReleaseSubject(release.Id, It.IsIn(subject.Id, replacementSubject.Id)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    cacheService: cacheService.Object,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                VerifyAllMocks(cacheService,
                    dataBlockService,
                    dataImportService,
                    footnoteRepository,
                    subjectRepository,
                    releaseDataFileService,
                    releaseSubjectRepository);

                dataBlockService.Verify(
                    mock => mock.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlan>()),
                    Times.Exactly(2));

                releaseDataFileService.Verify(
                    mock => mock.Delete(
                        release.Id,
                        It.IsIn(file.Id, replacementFile.Id),
                        false
                    ),
                    Times.Exactly(2)
                );

                releaseSubjectRepository.Verify(
                    mock => mock.SoftDeleteReleaseSubject(release.Id,
                        It.IsIn(subject.Id, replacementSubject.Id)), Times.Exactly(2));

                result.AssertRight();
            }
        }

        [Fact]
        public async Task RemoveDataFiles_ReplacementFileImporting()
        {
            var release = new Release
            {
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var file = new File
            {
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = subject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = file
            };

            file.ReplacedBy = replacementFile;

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, file, replacementFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.COMPLETE
                });
            dataImportService.Setup(service => service.GetImport(replacementFile.Id))
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.STAGE_1
                });

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataImportService: dataImportService.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                VerifyAllMocks(dataImportService);

                result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
            }
        }

        [Fact]
        public async Task UpdateRelease()
        {
            var release = new Release
            {
                Type = ReleaseType.AdHocStatistics,
                Publication = new Publication
                {
                    Contact = new Contact(),
                },
                ReleaseName = "2030",
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate {Day = "15", Month = "6", Year = "2039"},
                PreReleaseAccessList = "Old access list",
                Version = 0
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService
                    .UpdateRelease(
                        release.Id,
                        new ReleaseUpdateRequest
                        {
                            Type = ReleaseType.OfficialStatistics,
                            Year = 2035,
                            TimePeriodCoverage = TimeIdentifier.March,
                            PreReleaseAccessList = "New access list",
                        }
                    );

                var viewModel = result.AssertRight();

                Assert.Equal(release.Publication.Id, viewModel.PublicationId);
                Assert.Equal(release.NextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(ReleaseType.OfficialStatistics, viewModel.Type);
                Assert.Equal(2035, viewModel.Year);
                Assert.Equal("2035", viewModel.YearTitle);
                Assert.Equal(TimeIdentifier.March, viewModel.TimePeriodCoverage);
                Assert.Equal("New access list", viewModel.PreReleaseAccessList);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == release.Id);

                Assert.Equal(release.Publication.Id, saved.PublicationId);
                Assert.Equal(release.NextReleaseDate, saved.NextReleaseDate);
                Assert.Equal(ReleaseType.OfficialStatistics, saved.Type);
                Assert.Equal("2035-march", saved.Slug);
                Assert.Equal("2035", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.March, saved.TimePeriodCoverage);
                Assert.Equal("New access list", saved.PreReleaseAccessList);

                Assert.Empty(saved.ReleaseStatuses);
            }
        }

        [Fact]
        public async Task UpdateRelease_FailsNonUniqueSlug()
        {
            var publication = new Publication();

            var release = new Release
            {
                Type = ReleaseType.AdHocStatistics,
                Publication = publication,
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0
            };

            var otherRelease = new Release
            {
                Type = ReleaseType.AdHocStatistics,
                Publication = publication,
                ReleaseName = "2035",
                Slug = "2035",
                PublishScheduled = DateTime.UtcNow,
                Version = 0
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release, otherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService
                    .UpdateRelease(
                        release.Id,
                        new ReleaseUpdateRequest
                        {
                            Type = ReleaseType.AdHocStatistics,
                            Year = 2035,
                            TimePeriodCoverage = TimeIdentifier.CalendarYear,
                            PreReleaseAccessList = "Test"
                        }
                    );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task GetRelease()
        {
            var nextReleaseDate = new PartialDate {Day = "1", Month = "1", Year = "2040"};

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Type = ReleaseType.AdHocStatistics,
                TimePeriodCoverage = TimeIdentifier.January,
                PublishScheduled = DateTime.Parse("2020-06-29T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = DateTime.Parse("2020-06-29T02:00:00.00Z"),
                NextReleaseDate = nextReleaseDate,
                ReleaseName = "2035",
                Slug = "2035-1",
                Version = 0,
                PreReleaseAccessList = "Test access list",
                ReleaseStatuses = new List<ReleaseStatus>
                {
                    new()
                    {
                        InternalReleaseNote = "Latest release note - 1 day ago",
                        Created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1))
                    },
                    new()
                    {
                        InternalReleaseNote = "Release note 2 days ago",
                        Created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2))
                    }
                },
                NotifySubscribers = true,
                UpdatePublishedDate = true
            };

            var publication = new Publication
            {
                Id = new Guid("f7da23e2-304a-4b47-a8f5-dba28a554de9"),
                Title = "Test publication",
                Summary = "Test summary",
                Slug = "test-publication",
                Contact = new Contact(),
                LatestPublishedRelease = release,
                Releases =
                {
                    release
                }
            };
            
            var nonPreReleaseUserRole = new UserReleaseRole
            {
                Role = ReleaseRole.Contributor,
                Release = release
            };
            
            var nonPreReleaseUserInvite = new UserReleaseInvite
            {
                Role = ReleaseRole.Contributor,
                Release = release
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.UserReleaseRoles.AddAsync(nonPreReleaseUserRole);
                await context.UserReleaseInvites.AddAsync(nonPreReleaseUserInvite);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetRelease(release.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(release.Id, viewModel.Id);
                Assert.Equal("January 2035", viewModel.Title);
                Assert.Equal(2035, viewModel.Year);
                Assert.Equal("2035", viewModel.YearTitle);
                Assert.Equal("2035-1", viewModel.Slug);
                Assert.Equal(publication.Id, viewModel.PublicationId);
                Assert.Equal("Test publication", viewModel.PublicationTitle);
                Assert.Equal("Test summary", viewModel.PublicationSummary);
                Assert.Equal("test-publication", viewModel.PublicationSlug);
                Assert.Equal("Latest release note - 1 day ago", viewModel.LatestInternalReleaseNote);
                Assert.Equal(DateTime.Parse("2020-06-29T01:00:00.00"), viewModel.PublishScheduled);
                Assert.Equal(DateTime.Parse("2020-06-29T02:00:00.00Z"), viewModel.Published);
                Assert.Equal("Test access list", viewModel.PreReleaseAccessList);
                Assert.Equal(nextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(ReleaseType.AdHocStatistics, viewModel.Type);
                Assert.Equal(TimeIdentifier.January, viewModel.TimePeriodCoverage);

                Assert.Null(viewModel.PreviousVersionId);
                Assert.True(viewModel.LatestRelease);
                Assert.True(viewModel.Live);
                Assert.False(viewModel.Amendment);
                Assert.True(viewModel.NotifySubscribers);
                Assert.True(viewModel.UpdatePublishedDate);
                Assert.False(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task GetRelease_WithPreReleaseUsers()
        {
            var release = new Release
            {
                TimePeriodCoverage = TimeIdentifier.January,
                ReleaseName = "2035",
            };

            var publication = new Publication
            {
                Contact = new Contact(),
                Releases =
                {
                    release
                }
            };

            var preReleaseUserRole = new UserReleaseRole
            {
                Role = ReleaseRole.PrereleaseViewer,
                Release = release
            };
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.UserReleaseRoles.AddAsync(preReleaseUserRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetRelease(release.Id);

                var viewModel = result.AssertRight();
                Assert.True(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task GetRelease_WithPreReleaseInvites()
        {
            var release = new Release
            {
                TimePeriodCoverage = TimeIdentifier.January,
                ReleaseName = "2035",
            };

            var publication = new Publication
            {
                Contact = new Contact(),
                Releases =
                {
                    release
                }
            };

            var preReleaseUserInvite = new UserReleaseInvite
            {
                Role = ReleaseRole.PrereleaseViewer,
                Release = release
            };
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.UserReleaseInvites.AddAsync(preReleaseUserInvite);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetRelease(release.Id);

                var viewModel = result.AssertRight();
                Assert.True(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task GetRelease_NotLatestPublishedRelease()
        {
            var publication = new Publication
            {
                Contact = new Contact(),
                LatestPublishedRelease = new Release
                {
                    ReleaseName = "2022",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Published = DateTime.UtcNow
                }
            };

            var notLatestRelease = new Release
            {
                Publication = publication,
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.Releases.AddAsync(notLatestRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);
                var result = await releaseService.GetRelease(notLatestRelease.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(notLatestRelease.Id, viewModel.Id);
                Assert.False(viewModel.LatestRelease);
            }
        }

        [Fact]
        public async Task GetRelease_NoLatestInternalReleaseNote()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.CalendarYear
            };

            var publication = new Publication
            {
                Contact = new Contact(),
                Releases = new List<Release>
                {
                    release
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetRelease(release.Id);

                var releaseViewModel = result.AssertRight();
                Assert.Null(releaseViewModel.LatestInternalReleaseNote);
            }
        }

        [Fact]
        public async Task GetLatestPublishedRelease()
        {
            var publication = new Publication
            {
                LatestPublishedRelease = new Release
                {
                    ReleaseName = "2022",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Published = DateTime.UtcNow
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetLatestPublishedRelease(publication.Id);
                var latestIdTitleViewModel = result.AssertRight();

                Assert.NotNull(latestIdTitleViewModel);
                Assert.Equal(publication.LatestPublishedReleaseId, latestIdTitleViewModel!.Id);
                Assert.Equal("Calendar year 2022", latestIdTitleViewModel.Title);
            }
        }

        [Fact]
        public async Task GetLatestPublishedRelease_NoPublishedRelease()
        {
            var publication = new Publication
            {
                LatestPublishedReleaseId = null
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetLatestPublishedRelease(publication.Id);
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetDeleteReleasePlan()
        {
            var releaseBeingDeleted = new Release
            {
                Id = Guid.NewGuid()
            };

            // This is just another unrelated Release that should not be affected.
            var releaseNotBeingDeleted = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology1ScheduledWithRelease1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseId = releaseBeingDeleted.Id,
                AlternativeTitle = "Methodology 1 with alternative title",
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology 1 owned Publication title"
                }
            };

            var methodology2ScheduledWithRelease1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseId = releaseBeingDeleted.Id,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology 2 with owned Publication title"
                }
            };

            var methodologyScheduledWithRelease2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseId = releaseNotBeingDeleted.Id
            };

            var methodologyNotScheduled = new MethodologyVersion
            {
                Id = Guid.NewGuid()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Releases.AddRangeAsync(releaseBeingDeleted, releaseNotBeingDeleted);
                await context.MethodologyVersions.AddRangeAsync(
                    methodology1ScheduledWithRelease1,
                    methodology2ScheduledWithRelease1,
                    methodologyScheduledWithRelease2,
                    methodologyNotScheduled);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetDeleteReleasePlan(releaseBeingDeleted.Id);
                var plan = result.AssertRight();

                // Assert that only the 2 Methodologies that were scheduled with the Release being deleted are flagged
                // up in the Plan.
                Assert.Equal(2, plan.ScheduledMethodologies.Count);
                var methodology1 = plan.ScheduledMethodologies.Single(m => m.Id == methodology1ScheduledWithRelease1.Id);
                var methodology2 = plan.ScheduledMethodologies.Single(m => m.Id == methodology2ScheduledWithRelease1.Id);

                Assert.Equal("Methodology 1 with alternative title", methodology1.Title);
                Assert.Equal("Methodology 2 with owned Publication title", methodology2.Title);
            }
        }

        [Fact]
        public async Task DeleteRelease()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
            };

            // This Methodology is scheduled to go out with the Release being deleted.
            var methodologyScheduledWithRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology scheduled with this Release"
                },
                InternalReleaseNote = "A note"
            };

            // This Methodology has nothing to do with the Release being deleted.
            var methodologyScheduledWithAnotherRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid(),
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology scheduled with another Release"
                }
            };

            var userReleaseRole = new UserReleaseRole
            {
                UserId = _userId,
                Release = release
            };

            var userReleaseInvite = new UserReleaseInvite
            {
                Release = release
            };

            var anotherRelease = new Release
            {
                Publication = publication,
                Version = 0
            };

            var anotherUserReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                Release = anotherRelease
            };

            var anotherUserReleaseInvite = new UserReleaseInvite
            {
                Id = Guid.NewGuid(),
                Release = anotherRelease
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(publication);
                await context.AddRangeAsync(release, anotherRelease);
                await context.AddRangeAsync(userReleaseRole, anotherUserReleaseRole);
                await context.AddRangeAsync(userReleaseInvite, anotherUserReleaseInvite);
                await context.AddRangeAsync(methodologyScheduledWithRelease, methodologyScheduledWithAnotherRelease);
                await context.SaveChangesAsync();
            }

            var releaseDataFilesService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);

            releaseDataFilesService.Setup(mock =>
                mock.DeleteAll(release.Id, false)).ReturnsAsync(Unit.Instance);

            releaseFileService.Setup(mock =>
                mock.DeleteAll(release.Id, false)).ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(mock =>
                mock.SoftDeleteAllReleaseSubjects(release.Id)).Returns(Task.CompletedTask);

            cacheService
                .Setup(mock => mock.DeleteCacheFolder(
                    ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(release.Id))))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context,
                    releaseDataFileService: releaseDataFilesService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    cacheService: cacheService.Object);

                var result = await releaseService.DeleteRelease(release.Id);

                releaseDataFilesService.Verify(mock =>
                    mock.DeleteAll(release.Id, false), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.DeleteAll(release.Id, false), Times.Once);

                VerifyAllMocks(cacheService,
                    releaseDataFilesService,
                    releaseFileService
                );

                result.AssertRight();

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

                // Assert that Methodologies that were scheduled to go out with this Release are no longer scheduled
                // to do so
                var retrievedMethodology = context.MethodologyVersions.Single(m => m.Id == methodologyScheduledWithRelease.Id);
                Assert.True(retrievedMethodology.ScheduledForPublishingImmediately);
                Assert.Null(retrievedMethodology.ScheduledWithReleaseId);
                Assert.Null(retrievedMethodology.InternalReleaseNote);
                Assert.Equal(MethodologyStatus.Draft, retrievedMethodology.Status);
                Assert.InRange(DateTime.UtcNow
                    .Subtract(retrievedMethodology.Updated!.Value).Milliseconds, 0, 1500);

                // Assert that Methodologies that were scheduled to go out with other Releases remain unaffected
                var unrelatedMethodology = context.MethodologyVersions.Single(m => m.Id == methodologyScheduledWithAnotherRelease.Id);
                Assert.True(unrelatedMethodology.ScheduledForPublishingWithRelease);
                Assert.Equal(methodologyScheduledWithAnotherRelease.ScheduledWithReleaseId,
                    unrelatedMethodology.ScheduledWithReleaseId);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseId = Guid.NewGuid(),
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                },
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest
            {
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

            releaseCacheService.Setup(s => s.UpdateRelease(releaseId,
                publication.Slug,
                publication.Releases[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(contentDbContext: context,
                    releaseCacheService: releaseCacheService.Object);

                var result = await service
                    .UpdateReleasePublished(
                        releaseId,
                        request
                    );

                VerifyAllMocks(releaseCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .SingleAsync(r => r.Id == releaseId);

                Assert.Equal(request.Published, saved.Published);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished_LatestReleaseInPublication()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseId = releaseId,
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                },
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest
            {
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

            releaseCacheService.Setup(s => s.UpdateRelease(releaseId,
                publication.Slug,
                publication.Releases[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseId));

            // As the release is the latest for the publication the separate cache entry for the publication's latest
            // release should also be updated
            releaseCacheService.Setup(s => s.UpdateRelease(releaseId,
                publication.Slug,
                null
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(contentDbContext: context,
                    releaseCacheService: releaseCacheService.Object);

                var result = await service
                    .UpdateReleasePublished(
                        releaseId,
                        request
                    );

                VerifyAllMocks(releaseCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .SingleAsync(r => r.Id == releaseId);

                Assert.Equal(request.Published, saved.Published);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished_ReleaseNotPublished()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseId = Guid.NewGuid(),
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        Slug = "release-slug",
                        Published = null
                    }
                },
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest
            {
                Published = DateTime.UtcNow.AddDays(-1)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(context);

                var result = await service
                    .UpdateReleasePublished(
                        releaseId,
                        request
                    );

                result.AssertBadRequest(ReleaseNotPublished);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished_FutureDate()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseId = Guid.NewGuid(),
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                },
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest
            {
                Published = DateTime.UtcNow.AddDays(1)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(context);

                var result = await service
                    .UpdateReleasePublished(
                        releaseId,
                        request
                    );

                result.AssertBadRequest(ReleasePublishedCannotBeFutureDate);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished_ConvertsPublishedFromLocalToUniversalTimezone()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseId = Guid.NewGuid(),
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                },
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest
            {
                Published = DateTime.Parse("2022-08-08T09:30:00.0000000+01:00") // DateTimeKind: Local
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

            releaseCacheService.Setup(s => s.UpdateRelease(releaseId,
                publication.Slug,
                publication.Releases[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(contentDbContext: context,
                    releaseCacheService: releaseCacheService.Object);

                var result = await service
                    .UpdateReleasePublished(
                        releaseId,
                        request
                    );

                VerifyAllMocks(releaseCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .SingleAsync(r => r.Id == releaseId);

                // The published date retrieved from the database should always be represented in UTC
                // because of the conversion setup in the database context config.
                Assert.Equal(DateTimeKind.Utc, saved.Published!.Value.Kind);

                // Make sure the request date was converted to UTC before it was updated on the release
                Assert.Equal(DateTime.Parse("2022-08-08T08:30:00Z", styles: DateTimeStyles.AdjustToUniversal), saved.Published);
            }
        }

        private ReleaseService BuildReleaseService(
            ContentDbContext contentDbContext,
            StatisticsDbContext? statisticsDbContext = null,
            IReleaseRepository? releaseRepository = null,
            IReleaseCacheService? releaseCacheService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            ISubjectRepository? subjectRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseDataFileService? releaseDataFileService = null,
            IDataImportService? dataImportService = null,
            IFootnoteService? footnoteService = null,
            IFootnoteRepository? footnoteRepository = null,
            IDataBlockService? dataBlockService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IBlobCacheService? cacheService = null)
        {
            var userService = AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            return new ReleaseService(
                contentDbContext,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService.Object,
                releaseRepository ?? Mock.Of<IReleaseRepository>(Strict),
                releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                dataImportService ?? Mock.Of<IDataImportService>(Strict),
                footnoteService ?? Mock.Of<IFootnoteService>(Strict),
                footnoteRepository ?? Mock.Of<IFootnoteRepository>(Strict),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
                new SequentialGuidGenerator(),
                cacheService ?? Mock.Of<IBlobCacheService>(Strict)
            );
        }
    }
}
