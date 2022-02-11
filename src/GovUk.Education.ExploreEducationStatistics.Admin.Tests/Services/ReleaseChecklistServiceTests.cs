using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseChecklistServiceTests
    {
        [Fact]
        public async Task GetChecklist_AllErrors()
        {
            var publication = new Publication();

            var originalRelease = new Release
            {
                Publication = publication,
                Version = 0,
                Created = DateTime.UtcNow.AddMonths(-2),
                Updates = new List<Update>
                {
                    new()
                    {
                        Reason = "Original release note",
                        Created = DateTime.UtcNow.AddMonths(-2).AddDays(1),
                    }
                },
            };
            var release = new Release
            {
                Publication = publication,
                PreviousVersion = originalRelease,
                Version = 1,
                Created = DateTime.UtcNow.AddMonths(-1),
                Updates = new List<Update>
                {
                    new()
                    {
                        Reason = "Original release note",
                        Created = DateTime.UtcNow.AddMonths(-2).AddDays(1),
                    }
                },
            };

            var releaseContentSection1 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Type = ContentSectionType.Generic,
                    Content = new List<ContentBlock>()
                }
            };

            var releaseContentSection2 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Type = ContentSectionType.Generic,
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Body = "<p>Test</p>"
                        },
                        new DataBlock(),
                        new HtmlBlock
                        {
                            Body = ""
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    releaseContentSection1,
                    releaseContentSection2,
                    originalRelease);
                await context.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
            var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                methodologyVersionRepository
                    .Setup(mock => mock.GetLatestVersionByPublication(release.PublicationId))
                    .ReturnsAsync(new List<MethodologyVersion>());

                releaseDataFileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                releaseDataFileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(
                        new List<File>
                        {
                            new File()
                        }
                    );

                dataImportService
                    .Setup(s => s.HasIncompleteImports(release.Id))
                    .ReturnsAsync(true);

                dataGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    dataImportService: dataImportService.Object,
                    dataGuidanceService: dataGuidanceService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object
                );

                var result = await service.GetChecklist(release.Id);

                var checklist = result.AssertRight();

                Assert.False(checklist.Valid);

                Assert.Equal(6, checklist.Errors.Count);

                Assert.Equal(DataFileImportsMustBeCompleted, checklist.Errors[0].Code);
                Assert.Equal(DataFileReplacementsMustBeCompleted, checklist.Errors[1].Code);
                Assert.Equal(PublicDataGuidanceRequired, checklist.Errors[2].Code);
                Assert.Equal(ReleaseNoteRequired, checklist.Errors[3].Code);
                Assert.Equal(EmptyContentSectionExists, checklist.Errors[4].Code);
                Assert.Equal(GenericSectionsContainEmptyHtmlBlock, checklist.Errors[5].Code);
            }

            MockUtils.VerifyAllMocks(dataImportService,
                dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_AllWarningsWithNoDataFiles()
        {
            var release = new Release
            {
                Publication = new Publication()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                methodologyVersionRepository
                    .Setup(mock => mock.GetLatestVersionByPublication(release.PublicationId))
                    .ReturnsAsync(new List<MethodologyVersion>());

                releaseDataFileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                releaseDataFileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                dataGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    dataGuidanceService: dataGuidanceService.Object
                );

                var result = await service.GetChecklist(release.Id);

                var checklist = result.AssertRight();

                Assert.False(checklist.Valid);

                Assert.Equal(4, checklist.Warnings.Count);
                Assert.Equal(NoMethodology, checklist.Warnings[0].Code);
                Assert.Equal(NoNextReleaseDate, checklist.Warnings[1].Code);
                Assert.Equal(NoDataFiles, checklist.Warnings[2].Code);
                Assert.Equal(NoPublicPreReleaseAccessList, checklist.Warnings[3].Code);
            }

            MockUtils.VerifyAllMocks(dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_AllWarningsWithDataFiles()
        {
            var release = new Release
            {
                Publication = new Publication()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };
                var otherSubject = new Subject
                {
                    Id = Guid.NewGuid(),
                };

                methodologyVersionRepository
                    .Setup(mock => mock.GetLatestVersionByPublication(release.PublicationId))
                    .ReturnsAsync(new List<MethodologyVersion>());

                releaseDataFileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(
                        new List<File>
                        {
                            new File
                            {
                                Id = Guid.NewGuid(),
                                Filename = "test-file-1.csv",
                                Type = FileType.Data,
                                SubjectId = subject.Id
                            },
                            new File
                            {
                                Id = Guid.NewGuid(),
                                Filename = "test-file-2.csv",
                                Type = FileType.Data,
                                SubjectId = otherSubject.Id
                            }
                        }
                    );

                dataGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

                footnoteRepository
                    .Setup(r => r.GetSubjectsWithNoFootnotes(release.Id))
                    .ReturnsAsync(
                        new List<Subject>
                        {
                            subject,
                        }
                    );

                releaseDataFileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                dataBlockService
                    .Setup(s => s.List(release.Id))
                    .ReturnsAsync(
                        new List<DataBlockSummaryViewModel>
                        {
                            new DataBlockSummaryViewModel(),
                            new DataBlockSummaryViewModel(),
                        }
                    );

                var service = BuildReleaseChecklistService(
                    context,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    dataGuidanceService: dataGuidanceService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    dataBlockService: dataBlockService.Object
                );
                var result = await service.GetChecklist(release.Id);

                var checklist = result.AssertRight();

                Assert.False(checklist.Valid);

                Assert.Equal(5, checklist.Warnings.Count);
                Assert.Equal(NoMethodology, checklist.Warnings[0].Code);
                Assert.Equal(NoNextReleaseDate, checklist.Warnings[1].Code);

                var noFootnotesWarning = Assert.IsType<NoFootnotesOnSubjectsWarning>(checklist.Warnings[2]);
                Assert.Equal(NoFootnotesOnSubjects, noFootnotesWarning.Code);
                Assert.Equal(1, noFootnotesWarning.TotalSubjects);

                Assert.Equal(NoTableHighlights, checklist.Warnings[3].Code);
                Assert.Equal(NoPublicPreReleaseAccessList, checklist.Warnings[4].Code);
            }

            MockUtils.VerifyAllMocks(dataBlockService,
                footnoteRepository,
                dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_AllWarningsWithUnapprovedMethodology()
        {
            var release = new Release
            {
                Publication = new Publication()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                methodologyVersionRepository
                    .Setup(mock => mock.GetLatestVersionByPublication(release.PublicationId))
                    .ReturnsAsync(AsList(methodologyVersion));

                releaseDataFileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                releaseDataFileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                dataGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    dataGuidanceService: dataGuidanceService.Object
                );

                var result = await service.GetChecklist(release.Id);

                var checklist = result.AssertRight();

                Assert.False(checklist.Valid);

                Assert.Equal(4, checklist.Warnings.Count);

                var methodologyMustBeApprovedError =
                    Assert.IsType<MethodologyNotApprovedWarning>(checklist.Warnings[0]);
                Assert.Equal(MethodologyNotApproved, methodologyMustBeApprovedError.Code);
                Assert.Equal(methodologyVersion.Id, methodologyMustBeApprovedError.MethodologyId);

                Assert.Equal(NoNextReleaseDate, checklist.Warnings[1].Code);
                Assert.Equal(NoDataFiles, checklist.Warnings[2].Code);
                Assert.Equal(NoPublicPreReleaseAccessList, checklist.Warnings[3].Code);
            }

            MockUtils.VerifyAllMocks(dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_FullyValid()
        {
            var publication = new Publication();

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved
            };

            var originalRelease = new Release
            {
                Publication = publication,
                Version = 0,
                Created = DateTime.UtcNow.AddMonths(-2),
            };
            var release = new Release
            {
                Publication = publication,
                PreviousVersion = originalRelease,
                Version = 1,
                Created = DateTime.UtcNow.AddMonths(-1),
                DataGuidance = "Test guidance",
                PreReleaseAccessList = "Test access list",
                NextReleaseDate = new PartialDate
                {
                    Month = "12",
                    Year = "2021"
                },
                Updates = new List<Update>
                {
                    new()
                    {
                        Reason = "Test reason 2",
                        // To avoid checklist error, this amendment requires an Update
                        // created after release.Created
                        Created = DateTime.UtcNow,
                    }
                },
            };

            var releaseContentSection1 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Type = ContentSectionType.Generic,
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Body = "<p>test</p>"
                        }
                    }
                }
            };

            var releaseContentSection2 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Type = ContentSectionType.Generic,
                    Content = new List<ContentBlock>
                    {
                        new DataBlock()
                    }
                }
            };

            var releaseContentSection3 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Type = ContentSectionType.Headlines,
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Body = ""
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    releaseContentSection1,
                    releaseContentSection2,
                    releaseContentSection3,
                    originalRelease);
                await context.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>();
            var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };

                methodologyVersionRepository
                    .Setup(mock => mock.GetLatestVersionByPublication(release.PublicationId))
                    .ReturnsAsync(ListOf(methodologyVersion));

                releaseDataFileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(
                        new List<File>
                        {
                            new File
                            {
                                Id = Guid.NewGuid(),
                                Filename = "test-file-1.csv",
                                Type = FileType.Data,
                                SubjectId = subject.Id,
                            },
                        }
                    );

                releaseDataFileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                dataGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(Unit.Instance);

                footnoteRepository
                    .Setup(r => r.GetSubjectsWithNoFootnotes(release.Id))
                    .ReturnsAsync(new List<Subject>());

                dataBlockService
                    .Setup(s => s.List(release.Id))
                    .ReturnsAsync(
                        new List<DataBlockSummaryViewModel>
                        {
                            new DataBlockSummaryViewModel
                            {
                                HighlightName = "Test highlight name"
                            },
                        }
                    );

                var service = BuildReleaseChecklistService(
                    context,
                    dataGuidanceService: dataGuidanceService.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    dataBlockService: dataBlockService.Object
                );
                var result = await service.GetChecklist(release.Id);

                var checklist = result.AssertRight();

                Assert.Empty(checklist.Errors);
                Assert.Empty(checklist.Warnings);
                Assert.True(checklist.Valid);
            }

            MockUtils.VerifyAllMocks(dataBlockService,
                footnoteRepository,
                dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_NotFound()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildReleaseChecklistService(context);
                var result = await service.GetChecklist(Guid.NewGuid());
                result.AssertNotFound();
            }
        }

        private static ReleaseChecklistService BuildReleaseChecklistService(
            ContentDbContext contentDbContext,
            IDataImportService dataImportService = null,
            IUserService userService = null,
            IDataGuidanceService dataGuidanceService = null,
            IReleaseDataFileRepository releaseDataFileRepository = null,
            IMethodologyVersionRepository methodologyVersionRepository = null,
            IFootnoteRepository footnoteRepository = null,
            IDataBlockService dataBlockService = null)
        {
            return new(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                dataImportService ?? new Mock<IDataImportService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                dataGuidanceService ?? new Mock<IDataGuidanceService>().Object,
                releaseDataFileRepository ?? new Mock<IReleaseDataFileRepository>().Object,
                methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object,
                footnoteRepository ?? new Mock<IFootnoteRepository>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object
            );
        }
    }
}
