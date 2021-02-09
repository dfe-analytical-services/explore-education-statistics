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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
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
            var publication = new Publication
            {
                Methodology = new Methodology()
            };
            var originalRelease = new Release
            {
                Publication = publication,
                Version = 0
            };
            var release = new Release
            {
                Publication = publication,
                PreviousVersion = originalRelease,
                Version = 1,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release, originalRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var fileRepository = new Mock<IFileRepository>();

                fileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                fileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(
                        new List<File>
                        {
                            new File()
                        }
                    );

                var dataImportService = new Mock<IDataImportService>();
                var metaGuidanceService = new Mock<IMetaGuidanceService>();

                dataImportService
                    .Setup(s => s.HasIncompleteImports(release.Id))
                    .ReturnsAsync(true);

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicMetaGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    fileRepository: fileRepository.Object,
                    dataImportService: dataImportService.Object,
                    metaGuidanceService: metaGuidanceService.Object
                );

                var checklist = await service.GetChecklist(release.Id);

                Assert.True(checklist.IsRight);

                Assert.False(checklist.Right.Valid);

                Assert.Equal(5, checklist.Right.Errors.Count);

                Assert.Equal(DataFileImportsMustBeCompleted, checklist.Right.Errors[0].Code);
                Assert.Equal(DataFileReplacementsMustBeCompleted, checklist.Right.Errors[1].Code);

                var methodologyMustBeApprovedError =
                    Assert.IsType<MethodologyMustBeApprovedError>(checklist.Right.Errors[2]);
                Assert.Equal(MethodologyMustBeApproved, methodologyMustBeApprovedError.Code);
                Assert.Equal(publication.MethodologyId, methodologyMustBeApprovedError.MethodologyId);

                Assert.Equal(PublicMetaGuidanceRequired, checklist.Right.Errors[3].Code);
                Assert.Equal(ReleaseNoteRequired, checklist.Right.Errors[4].Code);
            }
        }

        [Fact]
        public async Task GetChecklist_AllWarningsWithNoDataFiles()
        {
            var publication = new Publication();
            var release = new Release
            {
                Publication = publication,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var fileRepository = new Mock<IFileRepository>();

                fileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                fileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                var metaGuidanceService = new Mock<IMetaGuidanceService>();

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicMetaGuidanceRequired));

                var service = BuildReleaseChecklistService(
                    context,
                    fileRepository: fileRepository.Object,
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
        }

        [Fact]
        public async Task GetChecklist_AllWarningsWithDataFiles()
        {
            var publication = new Publication();
            var release = new Release
            {
                Publication = publication,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

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

                var fileRepository = new Mock<IFileRepository>();

                fileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(
                        new List<File>
                        {
                            new File
                            {
                                Id = Guid.NewGuid(),
                                Filename = "test-file-1.csv",
                                Type = FileType.Data,
                                Release = release,
                                ReleaseId = release.Id,
                                SubjectId = subject.Id
                            },
                            new File
                            {
                                Id = Guid.NewGuid(),
                                Filename = "test-file-2.csv",
                                Type = FileType.Data,
                                Release = release,
                                ReleaseId = release.Id,
                                SubjectId = otherSubject.Id
                            }
                        }
                    );

                var metaGuidanceService = new Mock<IMetaGuidanceService>();

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(ValidationActionResult(PublicMetaGuidanceRequired));

                var footnoteRepository = new Mock<IFootnoteRepository>();

                footnoteRepository
                    .Setup(r => r.GetSubjectsWithNoFootnotes(release.Id))
                    .ReturnsAsync(
                        new List<Subject>
                        {
                            subject,
                        }
                    );

                fileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                var dataBlockService = new Mock<IDataBlockService>();

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
                    fileRepository: fileRepository.Object,
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
        }

        [Fact]
        public async Task GetChecklist_FullyValid()
        {
            var publication = new Publication
            {
                Methodology = new Methodology
                {
                    Status = MethodologyStatus.Approved
                }
            };

            var originalRelease = new Release
            {
                Publication = publication,
                Version = 0
            };
            var release = new Release
            {
                Publication = publication,
                PreviousVersion = originalRelease,
                Version = 1,
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
                        Release = originalRelease
                    },
                    new Update
                    {
                        Reason = "Test reason 2"
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(release, originalRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = ContentDbUtils.InMemoryContentDbContext(contextId))
            {
                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };

                var fileRepository = new Mock<IFileRepository>();

                fileRepository
                    .Setup(r => r.ListDataFiles(release.Id))
                    .ReturnsAsync(
                        new List<File>
                        {
                            new File
                            {
                                Id = Guid.NewGuid(),
                                Filename = "test-file-1.csv",
                                Type = FileType.Data,
                                Release = release,
                                ReleaseId = release.Id,
                                SubjectId = subject.Id,
                            },
                        }
                    );

                fileRepository
                    .Setup(r => r.ListReplacementDataFiles(release.Id))
                    .ReturnsAsync(new List<File>());

                var metaGuidanceService = new Mock<IMetaGuidanceService>();

                metaGuidanceService
                    .Setup(s => s.Validate(release.Id))
                    .ReturnsAsync(Unit.Instance);

                var footnoteRepository = new Mock<IFootnoteRepository>();

                footnoteRepository
                    .Setup(r => r.GetSubjectsWithNoFootnotes(release.Id))
                    .ReturnsAsync(new List<Subject>());

                var dataBlockService = new Mock<IDataBlockService>();

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
                    fileRepository: fileRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    dataBlockService: dataBlockService.Object
                );
                var checklist = await service.GetChecklist(release.Id);

                Assert.True(checklist.IsRight);

                Assert.Empty(checklist.Right.Errors);
                Assert.Empty(checklist.Right.Warnings);
                Assert.True(checklist.Right.Valid);
            }
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
            IFileRepository fileRepository = null,
            IFootnoteRepository footnoteRepository = null,
            IDataBlockService dataBlockService = null)
        {
            return new ReleaseChecklistService(
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                dataImportService ?? new Mock<IDataImportService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                metaGuidanceService ?? new Mock<IMetaGuidanceService>().Object,
                fileRepository ?? new Mock<IFileRepository>().Object,
                footnoteRepository ?? new Mock<IFootnoteRepository>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object
            );
        }
    }
}
