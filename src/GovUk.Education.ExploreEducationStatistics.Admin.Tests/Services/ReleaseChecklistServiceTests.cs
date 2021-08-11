using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
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

            var methodology = new Methodology
            {
                Id = Guid.NewGuid()
            };

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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    releaseContentSection1,
                    releaseContentSection2,
                    originalRelease);
                await context.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
            var metaGuidanceService = new Mock<IMetaGuidanceService>(MockBehavior.Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                methodologyRepository
                    .Setup(mock => mock.GetLatestByPublication(release.PublicationId))
                    .ReturnsAsync(new List<Methodology>
                    {
                        methodology
                    });
                
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

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicMetaGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    dataImportService: dataImportService.Object,
                    metaGuidanceService: metaGuidanceService.Object,
                    methodologyRepository: methodologyRepository.Object
                );

                var checklist = await service.GetChecklist(release.Id);

                Assert.True(checklist.IsRight);

                Assert.False(checklist.Right.Valid);

                Assert.Equal(7, checklist.Right.Errors.Count);

                Assert.Equal(DataFileImportsMustBeCompleted, checklist.Right.Errors[0].Code);
                Assert.Equal(DataFileReplacementsMustBeCompleted, checklist.Right.Errors[1].Code);

                var methodologyMustBeApprovedError =
                    Assert.IsType<MethodologyMustBeApprovedError>(checklist.Right.Errors[2]);
                Assert.Equal(MethodologyMustBeApproved, methodologyMustBeApprovedError.Code);
                Assert.Equal(methodology.Id, methodologyMustBeApprovedError.MethodologyId);

                Assert.Equal(PublicMetaGuidanceRequired, checklist.Right.Errors[3].Code);
                Assert.Equal(ReleaseNoteRequired, checklist.Right.Errors[4].Code);
                Assert.Equal(EmptyContentSectionExists, checklist.Right.Errors[5].Code);
                Assert.Equal(GenericSectionsContainEmptyHtmlBlock, checklist.Right.Errors[6].Code);
            }

            MockUtils.VerifyAllMocks(dataImportService,
                metaGuidanceService,
                methodologyRepository,
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            var metaGuidanceService = new Mock<IMetaGuidanceService>(MockBehavior.Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                methodologyRepository
                    .Setup(mock => mock.GetLatestByPublication(release.PublicationId))
                    .ReturnsAsync(new List<Methodology>());

                releaseDataFileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                releaseDataFileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicMetaGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    methodologyRepository: methodologyRepository.Object,
                    metaGuidanceService: metaGuidanceService.Object
                );

                var checklist = await service.GetChecklist(release.Id);

                Assert.True(checklist.IsRight);

                Assert.False(checklist.Right.Valid);

                Assert.Equal(4, checklist.Right.Warnings.Count);
                Assert.Equal(NoMethodology, checklist.Right.Warnings[0].Code);
                Assert.Equal(NoNextReleaseDate, checklist.Right.Warnings[1].Code);
                Assert.Equal(NoDataFiles, checklist.Right.Warnings[2].Code);
                Assert.Equal(NoPublicPreReleaseAccessList, checklist.Right.Warnings[3].Code);
            }

            MockUtils.VerifyAllMocks(metaGuidanceService,
                methodologyRepository,
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var metaGuidanceService = new Mock<IMetaGuidanceService>(MockBehavior.Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };
                var otherSubject = new Subject
                {
                    Id = Guid.NewGuid(),
                };

                methodologyRepository
                    .Setup(mock => mock.GetLatestByPublication(release.PublicationId))
                    .ReturnsAsync(new List<Methodology>());

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

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicMetaGuidanceRequired));

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
                    methodologyRepository: methodologyRepository.Object,
                    metaGuidanceService: metaGuidanceService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    dataBlockService: dataBlockService.Object
                );
                var checklist = await service.GetChecklist(release.Id);

                Assert.True(checklist.IsRight);

                Assert.False(checklist.Right.Valid);

                Assert.Equal(5, checklist.Right.Warnings.Count);
                Assert.Equal(NoMethodology, checklist.Right.Warnings[0].Code);
                Assert.Equal(NoNextReleaseDate, checklist.Right.Warnings[1].Code);

                var noFootnotesWarning = Assert.IsType<NoFootnotesOnSubjectsWarning>(checklist.Right.Warnings[2]);
                Assert.Equal(NoFootnotesOnSubjects, noFootnotesWarning.Code);
                Assert.Equal(1, noFootnotesWarning.TotalSubjects);

                Assert.Equal(NoTableHighlights, checklist.Right.Warnings[3].Code);
                Assert.Equal(NoPublicPreReleaseAccessList, checklist.Right.Warnings[4].Code);
            }

            MockUtils.VerifyAllMocks(dataBlockService,
                footnoteRepository,
                metaGuidanceService,
                methodologyRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_FullyValid()
        {
            var publication = new Publication();

            var methodology = new Methodology
            {
                Status = Approved
            };

            var originalRelease = new Release
            {
                Publication = publication,
                Version = 0,
                Created = DateTime.UtcNow.AddDays(-14),
            };
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId,
                Publication = publication,
                PreviousVersion = originalRelease,
                Version = 1,
                Created = DateTime.UtcNow.AddDays(-7),
                MetaGuidance = "Test meta guidance",
                PreReleaseAccessList = "Test access list",
                NextReleaseDate = new PartialDate
                {
                    Month = "12",
                    Year = "2021"
                },
                Updates = new List<Update>
                {
                    new Update
                    {
                        Reason = "Test reason 1",
                        ReleaseId = releaseId,
                        Created = DateTime.UtcNow.AddDays(-13),
                    },
                    new Update
                    {
                        Reason = "Test reason 2",
                        ReleaseId = releaseId,
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

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
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
            var metaGuidanceService = new Mock<IMetaGuidanceService>(MockBehavior.Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };

                methodologyRepository
                    .Setup(mock => mock.GetLatestByPublication(release.PublicationId))
                    .ReturnsAsync(new List<Methodology>
                    {
                        methodology
                    });

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

                metaGuidanceService
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
                    metaGuidanceService: metaGuidanceService.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    methodologyRepository: methodologyRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    dataBlockService: dataBlockService.Object
                );
                var checklist = await service.GetChecklist(release.Id);

                Assert.True(checklist.IsRight);

                Assert.Empty(checklist.Right.Errors);
                Assert.Empty(checklist.Right.Warnings);
                Assert.True(checklist.Right.Valid);
            }

            MockUtils.VerifyAllMocks(dataBlockService,
                footnoteRepository,
                metaGuidanceService,
                methodologyRepository,
                releaseDataFileRepository);
        }

        [Fact]
        public async Task GetChecklist_NotFound()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var service = BuildReleaseChecklistService(context);
                var checklist = await service.GetChecklist(Guid.NewGuid());

                Assert.True(checklist.IsLeft);

                Assert.IsType<NotFoundResult>(checklist.Left);
            }
        }

        private static ReleaseChecklistService BuildReleaseChecklistService(
            ContentDbContext contentDbContext,
            IDataImportService dataImportService = null,
            IUserService userService = null,
            IMetaGuidanceService metaGuidanceService = null,
            IReleaseDataFileRepository releaseDataFileRepository = null,
            IMethodologyRepository methodologyRepository = null,
            IFootnoteRepository footnoteRepository = null,
            IDataBlockService dataBlockService = null)
        {
            return new ReleaseChecklistService(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                dataImportService ?? new Mock<IDataImportService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                metaGuidanceService ?? new Mock<IMetaGuidanceService>().Object,
                releaseDataFileRepository ?? new Mock<IReleaseDataFileRepository>().Object,
                methodologyRepository ?? new Mock<IMethodologyRepository>().Object,
                footnoteRepository ?? new Mock<IFootnoteRepository>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object
            );
        }
    }
}
