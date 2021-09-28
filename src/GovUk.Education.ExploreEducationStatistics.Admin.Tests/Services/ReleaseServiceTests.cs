#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
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
                Title = "Publication"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(
                    new ReleaseType
                    {
                        Title = "Ad Hoc",
                    }
                );
                await context.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = releaseService.CreateRelease(
                    new ReleaseCreateViewModel
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
        public async Task CreateReleaseWithTemplate()
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

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(
                    new ReleaseType
                    {
                        Id = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8"), Title = "Ad Hoc",
                    }
                );
                await context.AddAsync(
                    new Publication
                    {
                        Id = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        Title = "Publication",
                        Releases = new List<Release>
                        {
                            new() // Template release
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
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = releaseService.CreateRelease(
                    new ReleaseCreateViewModel
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
        public async Task LatestReleaseCorrectlyReported()
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
                await context.AddAsync(publication);
                await context.AddRangeAsync(
                    new List<Release>
                    {
                        notLatestRelease, latestRelease
                    }
                );
                await context.SaveChangesAsync();
            }

            // Note that we use different contexts for each method call - this is to avoid misleadingly optimistic
            // loading of the entity graph as we go.
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);
                var notLatest = (await releaseService.GetRelease(notLatestRelease.Id)).Right;

                Assert.Equal(notLatestRelease.Id, notLatest.Id);
                Assert.False(notLatest.LatestRelease);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);
                var latest = (await releaseService.GetRelease(latestRelease.Id)).Right;

                Assert.Equal(latestRelease.Id, latest.Id);
                Assert.True(latest.LatestRelease);
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

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var releaseDataFileService = new Mock<IReleaseDataFileService>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

            dataBlockService.Setup(service => service.GetDeletePlan(release.Id, subject))
                .ReturnsAsync(new DeleteDataBlockPlan());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlan>()))
                .Returns(Task.CompletedTask);

            dataImportService.Setup(service => service.GetStatus(file.Id))
                .ReturnsAsync(DataImportStatus.COMPLETE);

            subjectRepository.Setup(service => service.Get(subject.Id)).ReturnsAsync(subject);

            releaseDataFileService.Setup(service => service.Delete(release.Id, file.Id, false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service => service.SoftDeleteReleaseSubject(release.Id, subject.Id))
                .Returns(Task.CompletedTask);

                await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);
                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                dataBlockService.Verify(mock =>
                    mock.GetDeletePlan(release.Id, subject), Times.Once());
                dataBlockService.Verify(
                    mock => mock.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlan>()),
                    Times.Once());

                releaseDataFileService.Verify(mock =>
                    mock.Delete(release.Id, file.Id, false), Times.Once());

                dataImportService.Verify(
                    mock => mock.GetStatus(file.Id),
                    Times.Once());

                releaseSubjectRepository.Verify(
                    mock => mock.SoftDeleteReleaseSubject(release.Id, subject.Id),
                    Times.Once());

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

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var fileStorageService = new Mock<IReleaseFileService>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

            dataImportService.Setup(service =>
                    service.GetStatus(file.Id))
                .ReturnsAsync(DataImportStatus.STAGE_1);

            subjectRepository.Setup(service => service.Get(subject.Id)).ReturnsAsync(subject);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseFileService: fileStorageService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                dataImportService.Verify(
                    mock => mock.GetStatus(file.Id),
                    Times.Once());

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

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var releaseDataFileService = new Mock<IReleaseDataFileService>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

            dataBlockService.Setup(service =>
                    service.GetDeletePlan(release.Id, It.IsIn(subject, replacementSubject)))
                .ReturnsAsync(new DeleteDataBlockPlan());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlan>()))
                .Returns(Task.CompletedTask);

            dataImportService.Setup(service =>
                    service.GetStatus(It.IsIn(file.Id, replacementFile.Id)))
                .ReturnsAsync(DataImportStatus.COMPLETE);

            subjectRepository.Setup(service => service.Get(subject.Id)).ReturnsAsync(subject);
            subjectRepository.Setup(service => service.Get(replacementSubject.Id)).ReturnsAsync(replacementSubject);

            releaseDataFileService
                .Setup(service => service.Delete(release.Id, It.IsIn(file.Id, replacementFile.Id), false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service =>
                    service.SoftDeleteReleaseSubject(release.Id, It.IsIn(subject.Id, replacementSubject.Id)))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                dataBlockService.Verify(
                    mock => mock.GetDeletePlan(release.Id, It.IsIn(subject, replacementSubject)),
                    Times.Exactly(2));
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

                dataImportService.Verify(
                    mock => mock.GetStatus(It.IsIn(file.Id, replacementFile.Id)), Times.Exactly(2));

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

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var fileStorageService = new Mock<IReleaseFileService>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

            dataImportService.Setup(service => service.GetStatus(file.Id))
                .ReturnsAsync(DataImportStatus.COMPLETE);
            dataImportService.Setup(service => service.GetStatus(replacementFile.Id))
                .ReturnsAsync(DataImportStatus.STAGE_1);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseFileService: fileStorageService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(release.Id, file.Id);

                dataImportService.Verify(
                    mock => mock.GetStatus(It.IsIn(file.Id, replacementFile.Id)), Times.Exactly(2));

                result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
            }
        }

        [Fact]
        public async Task UpdateRelease()
        {
            var releaseId = Guid.NewGuid();

            var adHocReleaseType = new ReleaseType {Title = "Ad Hoc"};
            var officialStatisticsReleaseType = new ReleaseType {Title = "Official Statistics"};

            var release = new Release
            {
                Id = releaseId,
                Type = adHocReleaseType,
                Publication = new Publication(),
                ReleaseName = "2030",
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate {Day = "15", Month = "6", Year = "2039"},
                PreReleaseAccessList = "Old access list",
                Version = 0,
                PreviousVersionId = releaseId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release,
                    adHocReleaseType, officialStatisticsReleaseType);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new ReleaseUpdateViewModel
                        {
                            TypeId = officialStatisticsReleaseType.Id,
                            ReleaseName = "2035",
                            TimePeriodCoverage = TimeIdentifier.March,
                            PreReleaseAccessList = "New access list",
                        }
                    );

                var viewModel = result.AssertRight();

                Assert.Equal(release.Publication.Id, viewModel.PublicationId);
                Assert.Equal(release.NextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(officialStatisticsReleaseType, viewModel.Type);
                Assert.Equal("2035", viewModel.ReleaseName);
                Assert.Equal(TimeIdentifier.March, viewModel.TimePeriodCoverage);
                Assert.Equal("New access list", viewModel.PreReleaseAccessList);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.Releases
                    .Include(r => r.ReleaseStatuses)
                    .FirstAsync(r => r.Id == releaseId);

                Assert.Equal(release.Publication.Id, saved.PublicationId);
                Assert.Equal(release.NextReleaseDate, saved.NextReleaseDate);
                Assert.Equal(officialStatisticsReleaseType.Id, saved.TypeId);
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
            var releaseType = new ReleaseType {Title = "Ad Hoc"};

            var publication = new Publication();

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
                await context.AddRangeAsync(releaseType, release, otherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService
                    .UpdateRelease(
                        releaseId,
                        new ReleaseUpdateViewModel
                        {
                            TypeId = releaseType.Id,
                            ReleaseName = "2035",
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
            var adHocReleaseType = new ReleaseType
            {
                Title = "Ad Hoc"
            };

            var releaseId = Guid.NewGuid();
            var nextReleaseDate = new PartialDate {Day = "1", Month = "1", Year = "2040"};

            var release = new Release
            {
                Id = releaseId,
                Type = adHocReleaseType,
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
                       InternalReleaseNote = "Release note null Created date",
                       Created = null
                    },
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
                }
            };

            var publication = new Publication
            {
                Id = new Guid("f7da23e2-304a-4b47-a8f5-dba28a554de9"),
                Title = "Test publication",
                Slug = "test-publication",
                Releases = new List<Release>
                {
                    release
                }
            };

            var publication2 = new Publication
            {
                Releases = new List<Release>
                {
                    new Release
                    {
                        ReleaseStatuses = new List<ReleaseStatus>
                        {
                            new ReleaseStatus
                            {
                                InternalReleaseNote = "Different release",
                                Created = DateTime.UtcNow
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, publication2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetRelease(releaseId);

                var viewModel = result.AssertRight();

                Assert.Equal("2035", viewModel.ReleaseName);
                Assert.Equal("2035-1", viewModel.Slug);
                Assert.Equal(publication.Id, viewModel.PublicationId);
                Assert.Equal("Test publication", viewModel.PublicationTitle);
                Assert.Equal("test-publication", viewModel.PublicationSlug);
                Assert.Equal("Latest release note - 1 day ago", viewModel.LatestInternalReleaseNote);
                Assert.Equal(DateTime.Parse("2020-06-29T01:00:00.00"), viewModel.PublishScheduled);
                Assert.Equal(DateTime.Parse("2020-06-29T02:00:00.00Z"), viewModel.Published);
                Assert.Equal("Test access list", viewModel.PreReleaseAccessList);
                Assert.Equal(nextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(adHocReleaseType, viewModel.Type);
                Assert.Equal(TimeIdentifier.January, viewModel.TimePeriodCoverage);
                Assert.Equal("2035", viewModel.YearTitle);

                Assert.Null(viewModel.PreviousVersionId);
                Assert.True(viewModel.LatestRelease);
                Assert.True(viewModel.Live);
                Assert.False(viewModel.Amendment);
            }
        }

        [Fact]
        async Task GetRelease_NoLatestInternalReleaseNote()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.CalendarYear
            };

            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    release
                }
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
                await context.AddAsync(
                    new UserReleaseRole
                    {
                        UserId = _userId,
                        ReleaseId = notLatestRelease.Id
                    }
                );

                await context.AddAsync(publication);
                await context.AddRangeAsync(
                    new List<Release>
                    {
                        notLatestRelease, latestReleaseV0, latestReleaseV1, latestReleaseV2Deleted
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var latest = releaseService.GetLatestPublishedRelease(publication.Id).Result.Right;

                Assert.NotNull(latest);
                Assert.Equal(latestReleaseV1.Id, latest!.Id);
                Assert.Equal("June 2036", latest.Title);
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

            var releaseDataFilesService = new Mock<IReleaseDataFileService>(MockBehavior.Strict);
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            releaseDataFilesService.Setup(mock =>
                mock.DeleteAll(release.Id, false)).ReturnsAsync(Unit.Instance);

            releaseFileService.Setup(mock =>
                mock.DeleteAll(release.Id, false)).ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context,
                    releaseDataFileService: releaseDataFilesService.Object,
                    releaseFileService: releaseFileService.Object);

                var result = await releaseService.DeleteRelease(release.Id);

                releaseDataFilesService.Verify(mock =>
                    mock.DeleteAll(release.Id, false), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.DeleteAll(release.Id, false), Times.Once);

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

        private ReleaseService BuildReleaseService(
            ContentDbContext contentDbContext,
            StatisticsDbContext? statisticsDbContext = null,
            IReleaseRepository? releaseRepository = null,
            IReleaseFileRepository? releaseFileRepository = null,
            ISubjectRepository? subjectRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseDataFileService? releaseDataFileService = null,
            IDataImportService? dataImportService = null,
            IFootnoteService? footnoteService = null,
            IDataBlockService? dataBlockService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null)
        {
            var userService = MockUtils.AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(_userId);

            return new ReleaseService(
                contentDbContext,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService.Object,
                releaseRepository ?? new Mock<IReleaseRepository>().Object,
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                subjectRepository ?? new Mock<ISubjectRepository>().Object,
                releaseDataFileService ?? new Mock<IReleaseDataFileService>().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                dataImportService ?? new Mock<IDataImportService>().Object,
                footnoteService ?? new Mock<IFootnoteService>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object,
                releaseSubjectRepository ?? new Mock<IReleaseSubjectRepository>().Object,
                new SequentialGuidGenerator()
            );
        }
    }
}
