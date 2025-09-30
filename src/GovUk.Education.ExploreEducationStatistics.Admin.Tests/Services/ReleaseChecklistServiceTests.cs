#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseChecklistServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData(DataSetVersionStatus.Mapping)]
    [InlineData(DataSetVersionStatus.Finalising)]
    public async Task GetChecklist_AllErrors(DataSetVersionStatus mappingStatus)
    {
        Release release = _dataFixture.DefaultRelease()
            .WithPublication(_dataFixture.DefaultPublication());

        var originalReleaseVersion = new ReleaseVersion
        {
            Release = release,
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

        var releaseVersion = new ReleaseVersion
        {
            Release = release,
            PreviousVersion = originalReleaseVersion,
            Version = 1,
            Created = DateTime.UtcNow.AddMonths(-1),
            GenericContent =
                new List<ContentSection>
                {
                    new()
                    {
                        Type = ContentSectionType.Generic,
                        Content = new List<ContentBlock>()
                    },
                    new()
                    {
                        Type = ContentSectionType.Generic,
                        Content =
                            new List<ContentBlock>
                            {
                                new HtmlBlock { Body = "<p>Test</p>" },
                                new DataBlock(),
                                new HtmlBlock { Body = "" }
                            }
                    }
                },
            RelatedDashboardsSection =
                new ContentSection
                {
                    Type = ContentSectionType.RelatedDashboards,
                    Content = new List<ContentBlock> { new HtmlBlock { Body = "" } }
                },
            SummarySection =
                new ContentSection
                {
                    Type = ContentSectionType.ReleaseSummary,
                    Content = new List<ContentBlock> { new HtmlBlock { Body = "" } }
                },
            Updates = new List<Update>
            {
                new()
                {
                    Reason = "Original release note",
                    Created = DateTime.UtcNow.AddMonths(-2).AddDays(1),
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(releaseVersion);
            await context.SaveChangesAsync();
        }

        var dataImportService = new Mock<IDataImportService>(MockBehavior.Strict);
        var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
        var dataSetVersionService = new Mock<IDataSetVersionService>(MockBehavior.Strict);

        await using (var context = InMemoryContentDbContext(contextId))
        {
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestVersionByPublication(releaseVersion.Release.PublicationId))
                .ReturnsAsync(new List<MethodologyVersion>());

            releaseDataFileRepository
                .Setup(r => r.ListDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            releaseDataFileRepository
                .Setup(r => r.ListReplacementDataFiles(releaseVersion.Id))
                .ReturnsAsync(
                    new List<File> { new() }
                );

            dataImportService
                .Setup(s => s.HasIncompleteImports(releaseVersion.Id))
                .ReturnsAsync(true);

            dataGuidanceService
                .Setup(s => s.ValidateForReleaseChecklist(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

            List<DataSetVersionStatusSummary> dataSetVersionStatusSummaries =
            [
                new(
                    Id: Guid.NewGuid(),
                    Title: "Data set 1",
                    Status: DataSetVersionStatus.Cancelled),
                new(
                    Id: Guid.NewGuid(),
                    Title: "Data set 2",
                    Status: mappingStatus),
                new(
                    Id: Guid.NewGuid(),
                    Title: "Data set 3",
                    Status: DataSetVersionStatus.Processing),
                new(
                    Id: Guid.NewGuid(),
                    Title: "Data set 4",
                    Status: DataSetVersionStatus.Failed)
            ];

            dataSetVersionService
                .Setup(s => s.GetStatusesForReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(dataSetVersionStatusSummaries);

            var service = BuildReleaseChecklistService(
                context,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                dataImportService: dataImportService.Object,
                dataGuidanceService: dataGuidanceService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                dataSetVersionService: dataSetVersionService.Object
            );

            var result = await service.GetChecklist(releaseVersion.Id);

            MockUtils.VerifyAllMocks(dataImportService,
                dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository,
                dataSetVersionService);

            var checklist = result.AssertRight();

            Assert.False(checklist.Valid);

            var errors = checklist.Errors
                .Select(error => error.Code)
                .ToList();

            Assert.Equal(13, errors.Count);
            Assert.Equal(DataFileImportsMustBeCompleted, errors[0]);
            Assert.Equal(DataFileReplacementsMustBeCompleted, errors[1]);
            Assert.Equal(PublicDataGuidanceRequired, errors[2]);
            Assert.Equal(ReleaseNoteRequired, errors[3]);
            Assert.Equal(SummarySectionContainsEmptyHtmlBlock, errors[4]);
            Assert.Equal(EmptyContentSectionExists, errors[5]);
            Assert.Equal(GenericSectionsContainEmptyHtmlBlock, errors[6]);
            Assert.Equal(ReleaseMustContainKeyStatOrNonEmptyHeadlineBlock, errors[7]);
            Assert.Equal(RelatedDashboardsSectionContainsEmptyHtmlBlock, errors[8]);
            Assert.Equal(PublicApiDataSetImportsMustBeCompleted, errors[9]);
            Assert.Equal(PublicApiDataSetCancellationsMustBeResolved, errors[10]);
            Assert.Equal(PublicApiDataSetFailuresMustBeResolved, errors[11]);
            Assert.Equal(PublicApiDataSetMappingsMustBeCompleted, errors[12]);
        }
    }

    [Fact]
    public async Task GetChecklist_AllWarningsWithNoDataFiles()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithNextReleaseDate(null)
            .WithPreReleaseAccessList(string.Empty);

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
        var dataSetVersionService = new Mock<IDataSetVersionService>(MockBehavior.Strict);

        await using (var context = InMemoryContentDbContext(contextId))
        {
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestVersionByPublication(releaseVersion.Release.PublicationId))
                .ReturnsAsync(new List<MethodologyVersion>());

            releaseDataFileRepository
                .Setup(r => r.ListDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            releaseDataFileRepository
                .Setup(r => r.ListReplacementDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            dataGuidanceService
                .Setup(s => s.ValidateForReleaseChecklist(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

            dataSetVersionService
                .Setup(s => s.GetStatusesForReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync([]);

            var service = BuildReleaseChecklistService(
                context,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                dataGuidanceService: dataGuidanceService.Object,
                dataSetVersionService: dataSetVersionService.Object
            );

            var result = await service.GetChecklist(releaseVersion.Id);

            MockUtils.VerifyAllMocks(dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository,
                dataSetVersionService);

            var checklist = result.AssertRight();

            Assert.False(checklist.Valid);

            Assert.Equal(4, checklist.Warnings.Count);
            Assert.Equal(NoMethodology, checklist.Warnings[0].Code);
            Assert.Equal(NoNextReleaseDate, checklist.Warnings[1].Code);
            Assert.Equal(NoDataFiles, checklist.Warnings[2].Code);
            Assert.Equal(NoPublicPreReleaseAccessList, checklist.Warnings[3].Code);
        }
    }

    [Fact]
    public async Task GetChecklist_AllWarningsWithDataFiles()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithNextReleaseDate(null)
            .WithPreReleaseAccessList(string.Empty);

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
        var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
        var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
        var dataSetVersionService = new Mock<IDataSetVersionService>(MockBehavior.Strict);

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            var otherSubject = new Subject { Id = Guid.NewGuid() };

            methodologyVersionRepository
                .Setup(mock => mock.GetLatestVersionByPublication(releaseVersion.Release.PublicationId))
                .ReturnsAsync(new List<MethodologyVersion>());

            releaseDataFileRepository
                .Setup(r => r.ListDataFiles(releaseVersion.Id))
                .ReturnsAsync(
                    new List<File>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Filename = "test-file-1.csv",
                            Type = FileType.Data,
                            SubjectId = subject.Id
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Filename = "test-file-2.csv",
                            Type = FileType.Data,
                            SubjectId = otherSubject.Id
                        }
                    }
                );

            dataGuidanceService
                .Setup(s => s.ValidateForReleaseChecklist(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

            footnoteRepository
                .Setup(r => r.GetSubjectsWithNoFootnotes(releaseVersion.Id))
                .ReturnsAsync(
                    new List<Subject>
                    {
                        subject,
                    }
                );

            releaseDataFileRepository
                .Setup(r => r.ListReplacementDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            dataBlockService
                .Setup(s => s.ListDataBlocks(releaseVersion.Id))
                .ReturnsAsync(
                    new List<DataBlock>
                    {
                        new(),
                        new(),
                    }
                );

            dataSetVersionService
                .Setup(s => s.GetStatusesForReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync([]);

            var service = BuildReleaseChecklistService(
                context,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                dataGuidanceService: dataGuidanceService.Object,
                footnoteRepository: footnoteRepository.Object,
                dataBlockService: dataBlockService.Object,
                dataSetVersionService: dataSetVersionService.Object
            );

            var result = await service.GetChecklist(releaseVersion.Id);

            MockUtils.VerifyAllMocks(dataBlockService,
                footnoteRepository,
                dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository,
                dataSetVersionService);

            var checklist = result.AssertRight();

            Assert.False(checklist.Valid);

            Assert.Equal(5, checklist.Warnings.Count);
            Assert.Equal(NoMethodology, checklist.Warnings[0].Code);
            Assert.Equal(NoNextReleaseDate, checklist.Warnings[1].Code);

            var noFootnotesWarning = Assert.IsType<NoFootnotesOnSubjectsWarning>(checklist.Warnings[2]);
            Assert.Equal(NoFootnotesOnSubjects, noFootnotesWarning.Code);
            Assert.Equal(1, noFootnotesWarning.TotalSubjects);

            Assert.Equal(NoFeaturedTables, checklist.Warnings[3].Code);
            Assert.Equal(NoPublicPreReleaseAccessList, checklist.Warnings[4].Code);
        }
    }

    [Fact]
    public async Task GetChecklist_AllWarningsWithUnapprovedMethodology()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithNextReleaseDate(null)
            .WithPreReleaseAccessList(string.Empty);

        var methodologyVersion = new MethodologyVersion { Status = Draft };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
        var dataSetVersionService = new Mock<IDataSetVersionService>(MockBehavior.Strict);

        await using (var context = InMemoryContentDbContext(contextId))
        {
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestVersionByPublication(releaseVersion.Release.PublicationId))
                .ReturnsAsync(AsList(methodologyVersion));

            releaseDataFileRepository
                .Setup(r => r.ListDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            releaseDataFileRepository
                .Setup(r => r.ListReplacementDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            dataGuidanceService
                .Setup(s => s.ValidateForReleaseChecklist(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(ValidationActionResult(PublicDataGuidanceRequired));

            dataSetVersionService
                .Setup(s => s.GetStatusesForReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync([]);

            var service = BuildReleaseChecklistService(
                context,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                dataGuidanceService: dataGuidanceService.Object,
                dataSetVersionService: dataSetVersionService.Object
            );

            var result = await service.GetChecklist(releaseVersion.Id);

            MockUtils.VerifyAllMocks(dataGuidanceService,
                methodologyVersionRepository,
                releaseDataFileRepository,
                dataGuidanceService,
                dataSetVersionService);

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
    }

    [Fact]
    public async Task GetChecklist_FullyValid()
    {
        Release release = _dataFixture.DefaultRelease()
            .WithPublication(_dataFixture.DefaultPublication());

        var methodologyVersion = new MethodologyVersion { Status = Approved };

        var originalReleaseVersion = new ReleaseVersion
        {
            Release = release,
            Version = 0,
            Created = DateTime.UtcNow.AddMonths(-2),
        };

        var dataBlockId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion
        {
            Release = release,
            PreviousVersion = originalReleaseVersion,
            Version = 1,
            Created = DateTime.UtcNow.AddMonths(-1),
            DataGuidance = "Test guidance",
            PreReleaseAccessList = "Test access list",
            GenericContent =
                new List<ContentSection>
                {
                    new()
                    {
                        Type = ContentSectionType.Generic,
                        Content = new List<ContentBlock> { new HtmlBlock { Body = "<p>test</p>" } }
                    },
                    new()
                    {
                        Type = ContentSectionType.Generic,
                        Content = new List<ContentBlock> { new DataBlock { Id = dataBlockId } }
                    }
                },
            HeadlinesSection =
                new ContentSection
                {
                    Type = ContentSectionType.Headlines,
                    Content = new List<ContentBlock> { new HtmlBlock { Body = "Not empty" } }
                },
            RelatedDashboardsSection =
                new ContentSection
                {
                    Type = ContentSectionType.RelatedDashboards,
                    Content = new List<ContentBlock> { new HtmlBlock { Body = "Not empty" } }
                },
            SummarySection =
                new ContentSection
                {
                    Type = ContentSectionType.ReleaseSummary,
                    Content = new List<ContentBlock> { new HtmlBlock { Body = "Not empty" } }
                },
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

        var featuredTable = new FeaturedTable
        {
            DataBlockId = dataBlockId,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.ReleaseVersions.AddRange(originalReleaseVersion, releaseVersion);
            context.FeaturedTables.AddRange(featuredTable);
            await context.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(MockBehavior.Strict);
        var footnoteRepository = new Mock<IFootnoteRepository>();
        var dataGuidanceService = new Mock<IDataGuidanceService>(MockBehavior.Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
        var dataSetVersionService = new Mock<IDataSetVersionService>(MockBehavior.Strict);

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var subject = new Subject { Id = Guid.NewGuid() };

            methodologyVersionRepository
                .Setup(mock => mock.GetLatestVersionByPublication(releaseVersion.Release.PublicationId))
                .ReturnsAsync(ListOf(methodologyVersion));

            releaseDataFileRepository
                .Setup(r => r.ListDataFiles(releaseVersion.Id))
                .ReturnsAsync(
                    new List<File>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Filename = "test-file-1.csv",
                            Type = FileType.Data,
                            SubjectId = subject.Id,
                        },
                    }
                );

            releaseDataFileRepository
                .Setup(r => r.ListReplacementDataFiles(releaseVersion.Id))
                .ReturnsAsync(new List<File>());

            dataGuidanceService
                .Setup(s => s.ValidateForReleaseChecklist(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(Unit.Instance);

            footnoteRepository
                .Setup(r => r.GetSubjectsWithNoFootnotes(releaseVersion.Id))
                .ReturnsAsync(new List<Subject>());

            dataBlockService
                .Setup(s => s.ListDataBlocks(releaseVersion.Id))
                .ReturnsAsync(
                    new List<DataBlock> { new() { Id = dataBlockId } }
                );

            dataSetVersionService
                .Setup(s => s.GetStatusesForReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(new[]
                    {
                        DataSetVersionStatus.Draft, DataSetVersionStatus.Withdrawn,
                        DataSetVersionStatus.Published, DataSetVersionStatus.Deprecated
                    }
                    .Select(status => new DataSetVersionStatusSummary(
                        Id: Guid.NewGuid(),
                        Title: "",
                        Status: status))
                    .ToList());

            var service = BuildReleaseChecklistService(
                context,
                dataGuidanceService: dataGuidanceService.Object,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                footnoteRepository: footnoteRepository.Object,
                dataBlockService: dataBlockService.Object,
                dataSetVersionService: dataSetVersionService.Object
            );
            var result = await service.GetChecklist(releaseVersion.Id);

            var checklist = result.AssertRight();

            Assert.Empty(checklist.Errors);
            Assert.Empty(checklist.Warnings);
            Assert.True(checklist.Valid);
        }

        MockUtils.VerifyAllMocks(dataBlockService,
            footnoteRepository,
            dataGuidanceService,
            methodologyVersionRepository,
            releaseDataFileRepository,
            dataSetVersionService);
    }

    [Fact]
    public async Task GetChecklist_NotFound()
    {
        await using var context = InMemoryContentDbContext();
        var service = BuildReleaseChecklistService(context);

        var result = await service.GetChecklist(releaseVersionId: Guid.NewGuid());

        result.AssertNotFound();
    }

    private static ReleaseChecklistService BuildReleaseChecklistService(
        ContentDbContext contentDbContext,
        IDataImportService? dataImportService = null,
        IUserService? userService = null,
        IDataGuidanceService? dataGuidanceService = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null,
        IMethodologyVersionRepository? methodologyVersionRepository = null,
        IFootnoteRepository? footnoteRepository = null,
        IDataBlockService? dataBlockService = null,
        IDataSetVersionService? dataSetVersionService = null)
    {
        return new(
            contentDbContext,
            dataImportService ?? new Mock<IDataImportService>().Object,
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            dataGuidanceService ?? new Mock<IDataGuidanceService>().Object,
            releaseDataFileRepository ?? new Mock<IReleaseDataFileRepository>().Object,
            methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object,
            footnoteRepository ?? new Mock<IFootnoteRepository>().Object,
            dataBlockService ?? new Mock<IDataBlockService>().Object,
            dataSetVersionService ?? new Mock<IDataSetVersionService>().Object
        );
    }
}
