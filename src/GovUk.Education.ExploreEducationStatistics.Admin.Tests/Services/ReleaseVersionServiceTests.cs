#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
using StatsReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class ReleaseVersionServiceTests
{
    private readonly DataFixture _dataFixture = new();
    private static readonly User User = new() { Id = Guid.NewGuid() };

    public class CreateReleaseTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task ReleaseTypeExperimentalStatistics_ReturnsValidationActionResult()
        {
            var releaseCreateRequest = new ReleaseCreateRequest
            {
                Type = ReleaseType.ExperimentalStatistics,
            };

            var releaseVersionService = BuildReleaseVersionService(Mock.Of<ContentDbContext>());

            var result = await releaseVersionService.CreateRelease(releaseCreateRequest);

            result.AssertBadRequest(ReleaseTypeInvalid);
        }

        [Fact]
        public async Task NoTemplate()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId;

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = (await releaseVersionService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = publication.Id,
                        Year = 2018,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics,
                        Label = "initial"
                    }
                )).AssertRight();
                newReleaseVersionId = result.Id;

                Assert.Equal("2018-19-initial", result.Slug);
                Assert.Equal("Academic year 2018/19 initial", result.Title);
                Assert.Equal("2018/19", result.YearTitle);
                Assert.Equal("initial", result.Label);
                Assert.Equal(0, result.Version);
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
                var actualReleaseVersion = await context.ReleaseVersions
                    .Include(rv => rv.Release)
                    .SingleAsync(rv => rv.Id == newReleaseVersionId);

                var actualRelease = actualReleaseVersion.Release;

                Assert.Equal(publication.Id, actualRelease.PublicationId);
                Assert.Equal(2018, actualRelease.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actualRelease.TimePeriodCoverage);
                Assert.Equal("2018-19-initial", actualRelease.Slug);

                Assert.Equal(2018, actualReleaseVersion.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actualReleaseVersion.TimePeriodCoverage);
                Assert.Equal("2018-19-initial", actualReleaseVersion.Slug);
                Assert.Equal(ReleaseType.OfficialStatistics, actualReleaseVersion.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, actualReleaseVersion.ApprovalStatus);
                Assert.Equal(0, actualReleaseVersion.Version);

                Assert.Null(actualReleaseVersion.PreviousVersionId);
                Assert.Null(actualReleaseVersion.PublishScheduled);
                Assert.Null(actualReleaseVersion.Published);
                Assert.Null(actualReleaseVersion.NextReleaseDate);
                Assert.Null(actualReleaseVersion.NotifiedOn);
                Assert.False(actualReleaseVersion.NotifySubscribers);
                Assert.False(actualReleaseVersion.UpdatePublishedDate);
            }
        }

        [Fact]
        public async Task WithTemplate()
        {
            var dataBlock1 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 1",
                Order = 2,
                Comments =
                [
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
                ]
            };

            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 2"
            };

            var templateReleaseVersion = new ReleaseVersion
            {
                Content = ListOf(new ContentSection
                {
                    Id = Guid.NewGuid(),
                    Caption = "Template caption index 0",
                    Heading = "Template heading index 0",
                    Type = ContentSectionType.Generic,
                    Order = 1,
                    Content =
                    [
                        new HtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Body = "<div></div>",
                            Order = 1,
                            Comments =
                            [
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
                            ]
                        },
                        dataBlock1,
                        dataBlock2
                    ]
                })
            };

            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease()
                    .WithVersions([templateReleaseVersion])
                    .Generate(1));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId;

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = publication.Id,
                        TemplateReleaseId = templateReleaseVersion.Id,
                        Year = 2018,
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Type = ReleaseType.OfficialStatistics
                    }
                );

                var newReleaseVersion = result.AssertRight();
                newReleaseVersionId = newReleaseVersion.Id;
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // Do an in depth check of the saved release version
                var newReleaseVersion = await context
                    .ReleaseVersions
                    .Include(releaseVersion => releaseVersion.Content)
                    .ThenInclude(section => section.Content)
                    .SingleAsync(rv => rv.Id == newReleaseVersionId);

                var contentSections = newReleaseVersion.GenericContent.ToList();
                Assert.Single(contentSections);
                Assert.Equal("Template caption index 0", contentSections[0].Caption);
                Assert.Equal("Template heading index 0", contentSections[0].Heading);
                Assert.Single(contentSections);
                Assert.Equal(1, contentSections[0].Order);

                // Content should not be copied when created from a template.
                Assert.Empty(contentSections[0].Content);
                Assert.Empty(contentSections[0].Content.AsReadOnly());
                Assert.Equal(ContentSectionType.ReleaseSummary, newReleaseVersion.SummarySection.Type);
                Assert.Equal(ContentSectionType.Headlines, newReleaseVersion.HeadlinesSection.Type);
                Assert.Equal(ContentSectionType.KeyStatisticsSecondary,
                    newReleaseVersion.KeyStatisticsSecondarySection.Type);

                // Data Blocks should not be copied when created from a template.
                Assert.Equal(2, context.DataBlocks.Count());
                Assert.Empty(context
                    .DataBlocks
                    .Where(dataBlock => dataBlock.ReleaseVersionId == newReleaseVersionId));
            }
        }
    }

    public class GetDeleteDataFilePlanTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            Subject subject = _dataFixture
                .DefaultSubject();

            File file = _dataFixture
                .DefaultFile()
                .WithSubjectId(subject.Id)
                .WithType(FileType.Data);

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();

                statisticsDbContext.Subject.Add(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);

            var deleteDataBlockPlan = new DeleteDataBlockPlanViewModel();
            dataBlockService.Setup(service =>
                    service.GetDeletePlan(releaseVersion.Id, It.Is<Subject>(s => s.Id == subject.Id)))
                .ReturnsAsync(deleteDataBlockPlan);

            var footnote = new Footnote { Id = Guid.NewGuid() };
            footnoteRepository.Setup(service => service.GetFootnotes(releaseVersion.Id, subject.Id))
                .ReturnsAsync([footnote]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object);

                var result = await releaseVersionService.GetDeleteDataFilePlan(
                    releaseVersionId: releaseVersion.Id,
                    fileId: file.Id);

                VerifyAllMocks(dataBlockService,
                    footnoteRepository);

                var deleteDataFilePlan = result.AssertRight();

                Assert.Equal(releaseVersion.Id, deleteDataFilePlan.ReleaseId);
                Assert.Equal(subject.Id, deleteDataFilePlan.SubjectId);
                Assert.Equal(deleteDataBlockPlan, deleteDataFilePlan.DeleteDataBlockPlan);
                Assert.Equal([footnote.Id], deleteDataFilePlan.FootnoteIds);
                Assert.Null(deleteDataFilePlan.DeleteApiDataSetVersionPlan);
                Assert.True(deleteDataFilePlan.Valid);
            }
        }

        [Fact]
        public async Task FileIsLinkedToPublicApiDataSet_InvalidPlan()
        {
            DataSet dataSet = _dataFixture
                .DefaultDataSet();

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet);

            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            Subject subject = _dataFixture
                .DefaultSubject();

            File file = _dataFixture
                .DefaultFile()
                .WithSubjectId(subject.Id)
                .WithType(FileType.Data);

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file)
                .WithPublicApiDataSetId(dataSet.Id)
                .WithPublicApiDataSetVersion(dataSetVersion.SemVersion());

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();

                statisticsDbContext.Subject.Add(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);

            dataSetVersionService.Setup(service => service.GetDataSetVersion(
                    releaseFile.PublicApiDataSetId!.Value,
                    releaseFile.PublicApiDataSetVersion!,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSetVersion);

            var deleteDataBlockPlan = new DeleteDataBlockPlanViewModel();
            dataBlockService.Setup(service =>
                    service.GetDeletePlan(releaseVersion.Id, It.Is<Subject>(s => s.Id == subject.Id)))
                .ReturnsAsync(deleteDataBlockPlan);

            var footnote = new Footnote { Id = Guid.NewGuid() };
            footnoteRepository.Setup(service => service.GetFootnotes(releaseVersion.Id, subject.Id))
                .ReturnsAsync([footnote]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataSetVersionService: dataSetVersionService.Object,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object);

                var result = await releaseVersionService.GetDeleteDataFilePlan(
                    releaseVersionId: releaseVersion.Id,
                    fileId: file.Id);

                VerifyAllMocks(dataSetVersionService,
                    dataBlockService,
                    footnoteRepository);

                var deleteDataFilePlan = result.AssertRight();

                Assert.Equal(releaseVersion.Id, deleteDataFilePlan.ReleaseId);
                Assert.Equal(subject.Id, deleteDataFilePlan.SubjectId);
                Assert.Equal(deleteDataBlockPlan, deleteDataFilePlan.DeleteDataBlockPlan);
                Assert.Equal([footnote.Id], deleteDataFilePlan.FootnoteIds);
                Assert.NotNull(deleteDataFilePlan.DeleteApiDataSetVersionPlan);
                Assert.Equal(dataSet.Id, deleteDataFilePlan.DeleteApiDataSetVersionPlan.DataSetId);
                Assert.Equal(dataSet.Title, deleteDataFilePlan.DeleteApiDataSetVersionPlan.DataSetTitle);
                Assert.Equal(dataSetVersion.Id, deleteDataFilePlan.DeleteApiDataSetVersionPlan.Id);
                Assert.Equal(dataSetVersion.PublicVersion, deleteDataFilePlan.DeleteApiDataSetVersionPlan.Version);
                Assert.Equal(dataSetVersion.Status, deleteDataFilePlan.DeleteApiDataSetVersionPlan.Status);
                Assert.False(deleteDataFilePlan.DeleteApiDataSetVersionPlan.Valid);
                Assert.False(deleteDataFilePlan.Valid);
            }
        }
    }

    public class RemoveDataFilesTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            Subject subject = _dataFixture
                .DefaultSubject();

            File file = _dataFixture
                .DefaultFile()
                .WithSubjectId(subject.Id)
                .WithType(FileType.Data);

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file);

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();

                statisticsDbContext.Subject.Add(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var cacheService = new Mock<IBlobCacheService>(Strict);
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);

            cacheService
                .Setup(service =>
                    service.DeleteItemAsync(new PrivateSubjectMetaCacheKey(releaseVersion.Id, subject.Id)))
                .Returns(Task.CompletedTask);

            dataBlockService.Setup(service => service.GetDeletePlan(releaseVersion.Id,
                    It.Is<Subject>(s => s.Id == subject.Id)))
                .ReturnsAsync(new DeleteDataBlockPlanViewModel());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlanViewModel>()))
                .ReturnsAsync(Unit.Instance);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport { Status = DataImportStatus.COMPLETE });

            footnoteRepository.Setup(service => service.GetFootnotes(releaseVersion.Id, subject.Id))
                .ReturnsAsync([]);

            releaseDataFileService.Setup(service => service.Delete(releaseVersion.Id, file.Id, false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository
                .Setup(service => service.DeleteReleaseSubject(releaseVersion.Id, subject.Id, true))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context,
                    statisticsDbContext,
                    cacheService: cacheService.Object,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseVersionService.RemoveDataFiles(releaseVersionId: releaseVersion.Id,
                    fileId: file.Id);

                VerifyAllMocks(cacheService,
                    dataBlockService,
                    dataImportService,
                    footnoteRepository,
                    releaseDataFileService,
                    releaseSubjectRepository
                );

                result.AssertRight();
            }
        }

        [Fact]
        public async Task FileImporting()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            Subject subject = _dataFixture
                .DefaultSubject();

            File file = _dataFixture
                .DefaultFile()
                .WithSubjectId(subject.Id)
                .WithType(FileType.Data);

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport { Status = DataImportStatus.STAGE_1 });

            await using var context = InMemoryApplicationDbContext(contentDbContextId);
            var releaseVersionService = BuildReleaseVersionService(
                context,
                dataImportService: dataImportService.Object);

            var result = await releaseVersionService.RemoveDataFiles(
                releaseVersionId: releaseVersion.Id,
                fileId: file.Id);

            VerifyAllMocks(dataImportService);

            result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
        }

        [Fact]
        public async Task ReplacementExists()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            Subject subject = _dataFixture
                .DefaultSubject();

            Subject replacementSubject = _dataFixture
                .DefaultSubject();

            File file = _dataFixture
                .DefaultFile()
                .WithSubjectId(subject.Id)
                .WithType(FileType.Data);

            File replacementFile = _dataFixture
                .DefaultFile()
                .WithSubjectId(replacementSubject.Id)
                .WithType(FileType.Data)
                .WithReplacing(file);

            file.ReplacedBy = replacementFile;

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file);

            ReleaseFile replacementReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(replacementFile);

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(releaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();

                statisticsDbContext.Subject.AddRange(subject, replacementSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var cacheService = new Mock<IBlobCacheService>(Strict);
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var dataImportService = new Mock<IDataImportService>(Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);

            cacheService.Setup(service =>
                    service.DeleteItemAsync(new PrivateSubjectMetaCacheKey(releaseVersion.Id, subject.Id)))
                .Returns(Task.CompletedTask);
            cacheService.Setup(service =>
                    service.DeleteItemAsync(
                        new PrivateSubjectMetaCacheKey(releaseVersion.Id, replacementSubject.Id)))
                .Returns(Task.CompletedTask);

            dataBlockService.Setup(service =>
                    service.GetDeletePlan(releaseVersion.Id,
                        It.Is<Subject>(s =>
                            new[] { subject.Id, replacementSubject.Id }.Contains(s.Id))))
                .ReturnsAsync(new DeleteDataBlockPlanViewModel());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlanViewModel>()))
                .ReturnsAsync(Unit.Instance);

            dataImportService.Setup(service => service.GetImport(It.IsIn(file.Id, replacementFile.Id)))
                .ReturnsAsync(new DataImport { Status = DataImportStatus.COMPLETE });

            footnoteRepository.Setup(service =>
                    service.GetFootnotes(releaseVersion.Id, It.IsIn(subject.Id, replacementSubject.Id)))
                .ReturnsAsync([]);

            releaseDataFileService
                .Setup(service => service.Delete(releaseVersion.Id, It.IsIn(file.Id, replacementFile.Id), false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service =>
                    service.DeleteReleaseSubject(releaseVersion.Id,
                        It.IsIn(subject.Id, replacementSubject.Id),
                        true))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    context,
                    statisticsDbContext,
                    cacheService: cacheService.Object,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseVersionService.RemoveDataFiles(
                    releaseVersionId: releaseVersion.Id,
                    fileId: file.Id);

                VerifyAllMocks(
                    cacheService,
                    dataBlockService,
                    dataImportService,
                    footnoteRepository,
                    releaseDataFileService,
                    releaseSubjectRepository);

                dataBlockService.Verify(
                    mock => mock.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlanViewModel>()),
                    Times.Exactly(2));

                releaseDataFileService.Verify(
                    mock => mock.Delete(
                        releaseVersion.Id,
                        It.IsIn(file.Id, replacementFile.Id),
                        false
                    ),
                    Times.Exactly(2)
                );

                releaseSubjectRepository.Verify(
                    mock => mock.DeleteReleaseSubject(releaseVersion.Id,
                        It.IsIn(subject.Id, replacementSubject.Id),
                        true),
                    Times.Exactly(2));

                result.AssertRight();
            }
        }

        [Fact]
        public async Task ReplacementFileImporting()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            Subject subject = _dataFixture
                .DefaultSubject();

            Subject replacementSubject = _dataFixture
                .DefaultSubject();

            File file = _dataFixture
                .DefaultFile()
                .WithSubjectId(subject.Id)
                .WithType(FileType.Data);

            File replacementFile = _dataFixture
                .DefaultFile()
                .WithSubjectId(replacementSubject.Id)
                .WithType(FileType.Data)
                .WithReplacing(file);

            file.ReplacedBy = replacementFile;

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file);

            ReleaseFile replacementReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(replacementFile);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(releaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport { Status = DataImportStatus.COMPLETE });
            dataImportService.Setup(service => service.GetImport(replacementFile.Id))
                .ReturnsAsync(new DataImport { Status = DataImportStatus.STAGE_1 });

            await using var context = InMemoryApplicationDbContext(contentDbContextId);
            var releaseVersionService = BuildReleaseVersionService(
                context,
                dataImportService: dataImportService.Object);

            var result = await releaseVersionService.RemoveDataFiles(
                releaseVersionId: releaseVersion.Id,
                fileId: file.Id);

            VerifyAllMocks(dataImportService);

            result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
        }

        [Fact]
        public async Task FileIsLinkedToPublicApiDataSet_ValidationProblem()
        {
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion();

            File file = _dataFixture
                .DefaultFile()
                .WithType(FileType.Data);

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file)
                .WithPublicApiDataSetId(Guid.NewGuid())
                .WithPublicApiDataSetVersion(1, 0);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataImportService = new Mock<IDataImportService>(Strict);

            dataImportService.Setup(service => service.GetImport(file.Id))
                .ReturnsAsync(new DataImport { Status = DataImportStatus.COMPLETE });

            await using var context = InMemoryApplicationDbContext(contentDbContextId);
            var releaseVersionService = BuildReleaseVersionService(
                context,
                dataImportService: dataImportService.Object);

            var result = await releaseVersionService.RemoveDataFiles(
                releaseVersionId: releaseVersion.Id,
                fileId: file.Id);

            VerifyAllMocks(dataImportService);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            var errorDetail = validationProblem.AssertHasError(
                expectedPath: null,
                expectedCode: ValidationMessages.CannotDeleteApiDataSetReleaseFile.Code);

            var apiDataSetErrorDetail = Assert.IsType<ApiDataSetErrorDetail>(errorDetail.Detail);

            Assert.Equal(releaseFile.PublicApiDataSetId, apiDataSetErrorDetail.DataSetId);
        }
    }

    public class UpdateReleaseVersionTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);

            const string newLabel = "initial";
            var newReleaseSlug = $"{release.Slug}-{newLabel}";
            var newReleaseTitle = $"{release.Title} {newLabel}";

            dataSetVersionService.Setup(service => service.UpdateVersionsForReleaseVersion(
                    releaseVersion.Id,
                    newReleaseSlug,
                    newReleaseTitle,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var updatedType = EnumUtil.GetEnums<ReleaseType>()
                .Except([releaseVersion.Type, ReleaseType.ExperimentalStatistics])
                .First();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: context,
                    dataSetVersionService: dataSetVersionService.Object);

                var result = await releaseVersionService
                    .UpdateReleaseVersion(
                        releaseVersion.Id,
                        new ReleaseVersionUpdateRequest
                        {
                            Type = updatedType,
                            Year = release.Year,
                            TimePeriodCoverage = release.TimePeriodCoverage,
                            PreReleaseAccessList = "New access list",
                            Label = newLabel
                        }
                    );

                VerifyAllMocks(dataSetVersionService);

                var viewModel = result.AssertRight();

                Assert.Equal(releaseVersion.Release.Publication.Id, viewModel.PublicationId);
                Assert.Equal(releaseVersion.NextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(updatedType, viewModel.Type);
                Assert.Equal(release.Year, viewModel.Year);
                Assert.Equal(release.YearTitle, viewModel.YearTitle);
                Assert.Equal(release.TimePeriodCoverage, viewModel.TimePeriodCoverage);
                Assert.Equal("New access list", viewModel.PreReleaseAccessList);
                Assert.Equal(newReleaseSlug, viewModel.Slug);
                Assert.Equal(newReleaseTitle, viewModel.Title);
                Assert.Equal(newLabel, viewModel.Label);
                Assert.Equal(releaseVersion.Version, viewModel.Version);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var actualReleaseVersion = await context.ReleaseVersions
                    .Include(rv => rv.Release)
                    .Include(rv => rv.ReleaseStatuses)
                    .SingleAsync(rv => rv.Id == releaseVersion.Id);

                var actualRelease = actualReleaseVersion.Release;

                Assert.Equal(publication.Id, actualRelease.PublicationId);
                Assert.Equal(release.Year, actualRelease.Year);
                Assert.Equal(release.TimePeriodCoverage, actualRelease.TimePeriodCoverage);
                Assert.Equal(newReleaseSlug, actualRelease.Slug);

                Assert.Equal(release.Year, actualReleaseVersion.Year);
                Assert.Equal(release.TimePeriodCoverage, actualReleaseVersion.TimePeriodCoverage);
                Assert.Equal(newReleaseSlug, actualReleaseVersion.Slug);
                Assert.Equal(releaseVersion.NextReleaseDate, actualReleaseVersion.NextReleaseDate);
                Assert.Equal(updatedType, actualReleaseVersion.Type);
                Assert.Equal("New access list", actualReleaseVersion.PreReleaseAccessList);

                Assert.Empty(actualReleaseVersion.ReleaseStatuses);
            }
        }

        [Fact]
        public async Task FailsNonUniqueSlug()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var (release, otherRelease) = _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                .WithPublication(publication)
                .Generate(2)
                .ToTuple2();

            var releaseVersion = release.Versions.Single();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService
                    .UpdateReleaseVersion(
                        releaseVersion.Id,
                        new ReleaseVersionUpdateRequest
                        {
                            Year = otherRelease.Year,
                            TimePeriodCoverage = otherRelease.TimePeriodCoverage,
                            Type = releaseVersion.Type,
                            PreReleaseAccessList = releaseVersion.PreReleaseAccessList
                        }
                    );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task ReleaseTypeExperimentalStatistics_ReturnsValidationActionResult()
        {
            var releaseUpdateRequest = new ReleaseVersionUpdateRequest
            {
                Type = ReleaseType.ExperimentalStatistics,
            };

            var releaseVersionService = BuildReleaseVersionService(Mock.Of<ContentDbContext>());

            var result = await releaseVersionService.UpdateReleaseVersion(It.IsAny<Guid>(), releaseUpdateRequest);

            result.AssertBadRequest(ReleaseTypeInvalid);
        }
    }

    public class GetReleaseTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ =>
                [
                    _dataFixture.DefaultRelease()
                        .WithVersions(_ =>
                        [
                            _dataFixture.DefaultReleaseVersion()
                                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                                .WithPublished(DateTime.UtcNow)
                                .WithReleaseStatuses(_dataFixture.DefaultReleaseStatus()
                                    .Generate(2))
                        ])
                ]);

            var releaseVersion = publication.Releases[0].Versions[0];

            var nonPreReleaseUserRole = new UserReleaseRole
            {
                Role = ReleaseRole.Contributor,
                ReleaseVersion = releaseVersion
            };

            var nonPreReleaseUserInvite = new UserReleaseInvite
            {
                Role = ReleaseRole.Contributor,
                ReleaseVersion = releaseVersion
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.ReleaseVersions.AddAsync(releaseVersion);
                await context.UserReleaseRoles.AddAsync(nonPreReleaseUserRole);
                await context.UserReleaseInvites.AddAsync(nonPreReleaseUserInvite);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetRelease(releaseVersion.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(releaseVersion.Id, viewModel.Id);
                Assert.Equal(releaseVersion.Release.Title, viewModel.Title);
                Assert.Equal(releaseVersion.Release.Year, viewModel.Year);
                Assert.Equal(releaseVersion.Release.YearTitle, viewModel.YearTitle);
                Assert.Equal(releaseVersion.Release.Label, viewModel.Label);
                Assert.Equal(releaseVersion.Version, viewModel.Version);
                Assert.Equal(releaseVersion.Release.Slug, viewModel.Slug);
                Assert.Equal(releaseVersion.Release.Publication.Id, viewModel.PublicationId);
                Assert.Equal(releaseVersion.Release.Publication.Title, viewModel.PublicationTitle);
                Assert.Equal(releaseVersion.Release.Publication.Slug, viewModel.PublicationSlug);
                Assert.Equal(releaseVersion.ApprovalStatus, viewModel.ApprovalStatus);
                Assert.Equal(releaseVersion.LatestInternalReleaseNote, viewModel.LatestInternalReleaseNote);
                Assert.Equal(releaseVersion.PublishScheduled, viewModel.PublishScheduled);
                Assert.Equal(releaseVersion.Published, viewModel.Published);
                Assert.Equal(releaseVersion.PreReleaseAccessList, viewModel.PreReleaseAccessList);
                Assert.Equal(releaseVersion.NextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(releaseVersion.Type, viewModel.Type);
                Assert.Equal(releaseVersion.Release.TimePeriodCoverage, viewModel.TimePeriodCoverage);
                Assert.Equal(releaseVersion.NotifySubscribers, viewModel.NotifySubscribers);
                Assert.Equal(releaseVersion.UpdatePublishedDate, viewModel.UpdatePublishedDate);

                Assert.Null(viewModel.PreviousVersionId);
                Assert.True(viewModel.LatestRelease);
                Assert.True(viewModel.Live);
                Assert.False(viewModel.Amendment);
                Assert.False(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task WithPreReleaseUsers()
        {
            UserReleaseRole preReleaseUserRole = _dataFixture.DefaultUserReleaseRole()
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication())));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.UserReleaseRoles.AddAsync(preReleaseUserRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetRelease(preReleaseUserRole.ReleaseVersionId);

                var viewModel = result.AssertRight();
                Assert.True(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task WithPreReleaseInvites()
        {
            UserReleaseInvite preReleaseUserInvite = _dataFixture.DefaultUserReleaseInvite()
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication())));

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.UserReleaseInvites.AddAsync(preReleaseUserInvite);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetRelease(preReleaseUserInvite.ReleaseVersionId);

                var viewModel = result.AssertRight();
                Assert.True(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task NotLatestPublishedRelease()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 2)]);

            var notLatestReleaseVersion = publication.Releases[0].Versions[0];

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);
                var result = await releaseVersionService.GetRelease(notLatestReleaseVersion.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(notLatestReleaseVersion.Id, viewModel.Id);
                Assert.False(viewModel.LatestRelease);
            }
        }

        [Fact]
        public async Task NoLatestInternalReleaseNote()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetRelease(publication.Releases[0].Versions[0].Id);

                var releaseViewModel = result.AssertRight();
                Assert.Null(releaseViewModel.LatestInternalReleaseNote);
            }
        }
    }

    public class GetLatestPublishedReleaseTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetLatestPublishedRelease(publication.Id);
                var latestIdTitleViewModel = result.AssertRight();

                Assert.NotNull(latestIdTitleViewModel);
                Assert.Equal(publication.Releases[0].Versions[0].Id, latestIdTitleViewModel.Id);
                Assert.Equal(publication.Releases[0].Title, latestIdTitleViewModel.Title);
            }
        }

        [Fact]
        public async Task NoPublishedRelease()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetLatestPublishedRelease(publication.Id);
                result.AssertNotFound();
            }
        }
    }

    public class GetDeleteReleaseVersionPlanTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var releaseBeingDeleted = new ReleaseVersion { Id = Guid.NewGuid() };

            // This is just another unrelated Release that should not be affected.
            var releaseNotBeingDeleted = new ReleaseVersion { Id = Guid.NewGuid() };

            var methodology1ScheduledWithRelease1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseVersionId = releaseBeingDeleted.Id,
                AlternativeTitle = "Methodology 1 with alternative title",
                Methodology = new Methodology { OwningPublicationTitle = "Methodology 1 owned Publication title" }
            };

            var methodology2ScheduledWithRelease1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseVersionId = releaseBeingDeleted.Id,
                Methodology = new Methodology { OwningPublicationTitle = "Methodology 2 with owned Publication title" }
            };

            var methodologyScheduledWithRelease2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseVersionId = releaseNotBeingDeleted.Id
            };

            var methodologyNotScheduled = new MethodologyVersion { Id = Guid.NewGuid() };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.AddRange(releaseBeingDeleted, releaseNotBeingDeleted);
                context.MethodologyVersions.AddRange(
                    methodology1ScheduledWithRelease1,
                    methodology2ScheduledWithRelease1,
                    methodologyScheduledWithRelease2,
                    methodologyNotScheduled);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(context);

                var result = await releaseVersionService.GetDeleteReleaseVersionPlan(releaseBeingDeleted.Id);

                var plan = result.AssertRight();

                // Assert that only the 2 Methodologies that were scheduled with the Release being deleted are flagged
                // up in the Plan.
                Assert.Equal(2, plan.ScheduledMethodologies.Count);
                var methodology1 =
                    plan.ScheduledMethodologies.Single(m => m.Id == methodology1ScheduledWithRelease1.Id);
                var methodology2 =
                    plan.ScheduledMethodologies.Single(m => m.Id == methodology2ScheduledWithRelease1.Id);

                Assert.Equal("Methodology 1 with alternative title", methodology1.Title);
                Assert.Equal("Methodology 2 with owned Publication title", methodology2.Title);
            }
        }
    }

    public class DeleteReleaseVersionTests : ReleaseVersionServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Success(bool isAmendment)
        {
            // Arrange
            var release = _dataFixture.DefaultRelease().Generate();

            var publication = new Publication
            {
                ReleaseSeries =
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ReleaseId = release.Id
                    }
                ],
                Theme = new Theme()
            };

            var releaseVersion = new ReleaseVersion
            {
                Id = release.Id,
                Publication = publication,
                Version = isAmendment ? 0 : 1,
                ReleaseId = release.Id
            };

            var statisticsReleaseVersion = new StatsReleaseVersion
            {
                Id = releaseVersion.Id,
                PublicationId = releaseVersion.Publication.Id
            };

            // This Methodology is scheduled to go out with the Release being deleted.
            var methodologyScheduledWithRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                ScheduledWithReleaseVersionId = releaseVersion.Id,
                Methodology =
                    new Methodology { OwningPublicationTitle = "Methodology scheduled with this Release" },
            };

            // This Methodology has nothing to do with the Release being deleted.
            var methodologyScheduledWithAnotherRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                ScheduledWithReleaseVersionId = Guid.NewGuid(),
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology scheduled with another Release",
                },
            };

            var userReleaseRole = new UserReleaseRole
            {
                UserId = User.Id,
                ReleaseVersion = releaseVersion
            };

            var userReleaseInvite = new UserReleaseInvite { ReleaseVersion = releaseVersion };

            var anotherRelease = new ReleaseVersion
            {
                Publication = publication,
                Version = 0
            };

            var anotherUserReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                ReleaseVersion = anotherRelease
            };

            var anotherUserReleaseInvite = new UserReleaseInvite
            {
                Id = Guid.NewGuid(),
                ReleaseVersion = anotherRelease
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(release);
                context.ReleaseVersions.AddRange(releaseVersion, anotherRelease);
                context.UserReleaseRoles.AddRange(userReleaseRole, anotherUserReleaseRole);
                context.UserReleaseInvites.AddRange(userReleaseInvite, anotherUserReleaseInvite);
                context.MethodologyVersions.AddRange(methodologyScheduledWithRelease,
                    methodologyScheduledWithAnotherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                context.ReleaseVersion.AddRange(statisticsReleaseVersion);
                await context.SaveChangesAsync();
            }

            var releaseDataFilesService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);
            var processorClient = new Mock<IProcessorClient>(Strict);

            var forceDeleteRelatedData = false;

            releaseDataFilesService.Setup(mock =>
                mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData)).ReturnsAsync(Unit.Instance);

            releaseFileService.Setup(mock =>
                mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData)).ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(mock =>
                    mock.DeleteAllReleaseSubjects(releaseVersion.Id, !forceDeleteRelatedData))
                .Returns(Task.CompletedTask);

            cacheService
                .Setup(mock => mock.DeleteCacheFolderAsync(
                    ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersion.Id))))
                .Returns(Task.CompletedTask);

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    forceDeleteRelatedData,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    releaseDataFileService: releaseDataFilesService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    cacheService: cacheService.Object,
                    processorClient: processorClient.Object);

                // Act
                var result = await releaseVersionService.DeleteReleaseVersion(releaseVersion.Id);

                // Assert
                releaseDataFilesService.Verify(mock =>
                        mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData),
                    Times.Once);

                releaseFileService.Verify(mock =>
                        mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData),
                    Times.Once);

                releaseSubjectRepository.Verify(mock =>
                        mock.DeleteAllReleaseSubjects(releaseVersion.Id, !forceDeleteRelatedData),
                    Times.Once);

                VerifyAllMocks(cacheService,
                    releaseDataFilesService,
                    releaseFileService,
                    processorClient
                );

                result.AssertRight();

                if (isAmendment)
                {
                    // assert that hard-deleted entities no longer exist
                    var hardDeletedRelease = await contentDbContext
                        .ReleaseVersions
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(rv => rv.Id == releaseVersion.Id);

                    Assert.Null(hardDeletedRelease);

                    var hardDeletedReleaseRole = await contentDbContext
                        .UserReleaseRoles
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(r => r.Id == userReleaseRole.Id);

                    Assert.Null(hardDeletedReleaseRole);

                    var hardDeletedReleaseInvite = await contentDbContext
                        .UserReleaseInvites
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(r => r.Id == userReleaseInvite.Id);

                    Assert.Null(hardDeletedReleaseInvite);

                    // assert that other entities were not accidentally hard-deleted
                    var retrievedAnotherReleaseRole = await contentDbContext
                        .UserReleaseRoles
                        .IgnoreQueryFilters()
                        .FirstAsync(r => r.Id == anotherUserReleaseRole.Id);

                    Assert.False(retrievedAnotherReleaseRole.SoftDeleted);

                    var retrievedAnotherReleaseInvite = await contentDbContext
                        .UserReleaseInvites
                        .IgnoreQueryFilters()
                        .FirstAsync(r => r.Id == anotherUserReleaseInvite.Id);

                    Assert.False(retrievedAnotherReleaseInvite.SoftDeleted);
                }
                else
                {
                    // assert that soft-deleted entities are no longer discoverable by default
                    var softDeletedRelease = await contentDbContext
                        .ReleaseVersions
                        .FirstOrDefaultAsync(rv => rv.Id == releaseVersion.Id);

                    Assert.Null(softDeletedRelease);

                    var softDeletedReleaseRole = await contentDbContext
                        .UserReleaseRoles
                        .FirstOrDefaultAsync(r => r.Id == userReleaseRole.Id);

                    Assert.Null(softDeletedReleaseRole);

                    var softDeletedReleaseInvite = await contentDbContext
                        .UserReleaseInvites
                        .FirstOrDefaultAsync(r => r.Id == userReleaseInvite.Id);

                    Assert.Null(softDeletedReleaseInvite);

                    // assert that soft-deleted entities do not appear via references from other entities by default
                    var publicationWithoutDeletedRelease = await contentDbContext
                        .Publications
                        .Include(p => p.ReleaseVersions)
                        .AsNoTracking()
                        .FirstAsync(p => p.Id == publication.Id);

                    Assert.Single(publicationWithoutDeletedRelease.ReleaseVersions);
                    Assert.Equal(anotherRelease.Id, publicationWithoutDeletedRelease.ReleaseVersions[0].Id);

                    // assert that soft-deleted entities have had their soft-deleted flag set to true
                    var updatedRelease = await contentDbContext
                        .ReleaseVersions
                        .IgnoreQueryFilters()
                        .FirstAsync(rv => rv.Id == releaseVersion.Id);

                    Assert.True(updatedRelease.SoftDeleted);

                    var updatedReleaseRole = await contentDbContext
                        .UserReleaseRoles
                        .IgnoreQueryFilters()
                        .FirstAsync(r => r.Id == userReleaseRole.Id);

                    Assert.True(updatedReleaseRole.SoftDeleted);

                    var updatedReleaseInvite = await contentDbContext
                        .UserReleaseInvites
                        .IgnoreQueryFilters()
                        .FirstAsync(r => r.Id == userReleaseInvite.Id);

                    Assert.True(updatedReleaseInvite.SoftDeleted);

                    // assert that soft-deleted entities appear via references from other entities when explicitly searched for
                    var publicationWithDeletedRelease = await contentDbContext
                        .Publications
                        .Include(p => p.ReleaseVersions)
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .FirstAsync(p => p.Id == publication.Id);

                    Assert.Equal(2, publicationWithDeletedRelease.ReleaseVersions.Count);
                    Assert.Equal(updatedRelease.Id, publicationWithDeletedRelease.ReleaseVersions[0].Id);
                    Assert.Equal(anotherRelease.Id, publicationWithDeletedRelease.ReleaseVersions[1].Id);
                    Assert.True(publicationWithDeletedRelease.ReleaseVersions[0].SoftDeleted);
                    Assert.False(publicationWithDeletedRelease.ReleaseVersions[1].SoftDeleted);

                    // assert that other entities were not accidentally soft-deleted
                    var retrievedAnotherReleaseRole = await contentDbContext
                        .UserReleaseRoles
                        .FirstAsync(r => r.Id == anotherUserReleaseRole.Id);

                    Assert.False(retrievedAnotherReleaseRole.SoftDeleted);

                    var retrievedAnotherReleaseInvite = await contentDbContext
                        .UserReleaseInvites
                        .FirstAsync(r => r.Id == anotherUserReleaseInvite.Id);

                    Assert.False(retrievedAnotherReleaseInvite.SoftDeleted);
                }

                // Assert that Methodologies that were scheduled to go out with this Release are no longer scheduled
                // to do so
                var retrievedMethodologyVersion =
                    await contentDbContext.MethodologyVersions.SingleAsync(m =>
                        m.Id == methodologyScheduledWithRelease.Id);
                Assert.True(retrievedMethodologyVersion.ScheduledForPublishingImmediately);
                Assert.Null(retrievedMethodologyVersion.ScheduledWithReleaseVersionId);
                Assert.Equal(MethodologyApprovalStatus.Draft, retrievedMethodologyVersion.Status);
                Assert.InRange(DateTime.UtcNow
                        .Subtract(retrievedMethodologyVersion.Updated!.Value).Milliseconds,
                    0,
                    1500);

                // Assert that Methodologies that were scheduled to go out with other Releases remain unaffected
                var unrelatedMethodology =
                    await contentDbContext.MethodologyVersions.SingleAsync(
                        m => m.Id == methodologyScheduledWithAnotherRelease.Id);
                Assert.True(unrelatedMethodology.ScheduledForPublishingWithRelease);
                Assert.Equal(methodologyScheduledWithAnotherRelease.ScheduledWithReleaseVersionId,
                    unrelatedMethodology.ScheduledWithReleaseVersionId);

                // We don't expect the Statistics ReleaseVersion to be deleted at this point, whether the
                // Content ReleaseVersion is a draft or not. This needs to remain until the stored procedure
                // that removes soft-deleted Subjects related to this ReleaseVersion is run. 
                Assert.NotNull(await statisticsDbContext
                    .ReleaseVersion
                    .SingleOrDefaultAsync(rv => rv.Id == statisticsReleaseVersion.Id));
            }
        }

        [Fact]
        public async Task ProcessorReturns400_Returns400()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Theme = new Theme() }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var processorClient = new Mock<IProcessorClient>(Strict);

            var forceDeletePublicApiData = false;

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    forceDeletePublicApiData,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BadRequestObjectResult(new ValidationProblemViewModel
                {
                    Errors = new ErrorViewModel[]
                    {
                        new()
                        {
                            Path = "error path",
                            Code = "error code"
                        }
                    }
                }));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    context,
                    processorClient: processorClient.Object);

                var result = await releaseVersionService.DeleteReleaseVersion(releaseVersion.Id);

                VerifyAllMocks(processorClient);

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                validationProblem.AssertHasError(
                    expectedPath: "error path",
                    expectedCode: "error code");
            }
        }

        [Fact]
        public async Task ProcessorThrows_Throws()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Theme = new Theme() }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var processorClient = new Mock<IProcessorClient>(Strict);

            var forceDeletePublicApiData = false;

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    forceDeletePublicApiData,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    context,
                    processorClient: processorClient.Object);

                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                    await releaseVersionService.DeleteReleaseVersion(releaseVersion.Id));

                VerifyAllMocks(processorClient);
            }
        }
    }

    public class DeleteTestReleaseVersionTests : ReleaseVersionServiceTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Success(bool softDeleted)
        {
            // Arrange
            var release = _dataFixture.DefaultRelease().Generate();

            var publication = new Publication
            {
                ReleaseSeries =
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ReleaseId = release.Id
                    }
                ],
                Theme = new Theme()
            };

            var releaseVersion = new ReleaseVersion
            {
                Id = release.Id,
                Publication = publication,
                Version = 1,
                ReleaseId = release.Id,
                ApprovalStatus = ReleaseApprovalStatus.Approved,
                SoftDeleted = softDeleted
            };

            var statisticsReleaseVersion = new StatsReleaseVersion
            {
                Id = releaseVersion.Id,
                PublicationId = releaseVersion.Publication.Id
            };

            // This Methodology is scheduled to go out with the Release being deleted.
            var methodologyScheduledWithRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                ScheduledWithReleaseVersionId = releaseVersion.Id,
                Methodology =
                    new Methodology { OwningPublicationTitle = "Methodology scheduled with this Release" },
            };

            // This Methodology has nothing to do with the Release being deleted.
            var methodologyScheduledWithAnotherRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                ScheduledWithReleaseVersionId = Guid.NewGuid(),
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology scheduled with another Release",
                },
            };

            var userReleaseRole = new UserReleaseRole
            {
                UserId = User.Id,
                ReleaseVersion = releaseVersion
            };

            var userReleaseInvite = new UserReleaseInvite { ReleaseVersion = releaseVersion };

            var anotherRelease = new ReleaseVersion
            {
                Publication = publication,
                Version = 0
            };

            var anotherUserReleaseRole = new UserReleaseRole
            {
                Id = Guid.NewGuid(),
                ReleaseVersion = anotherRelease
            };

            var anotherUserReleaseInvite = new UserReleaseInvite
            {
                Id = Guid.NewGuid(),
                ReleaseVersion = anotherRelease
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(release);
                context.ReleaseVersions.AddRange(releaseVersion, anotherRelease);
                context.UserReleaseRoles.AddRange(userReleaseRole, anotherUserReleaseRole);
                context.UserReleaseInvites.AddRange(userReleaseInvite, anotherUserReleaseInvite);
                context.MethodologyVersions.AddRange(methodologyScheduledWithRelease,
                    methodologyScheduledWithAnotherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                context.ReleaseVersion.AddRange(statisticsReleaseVersion);
                await context.SaveChangesAsync();
            }

            var releaseDataFilesService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);
            var processorClient = new Mock<IProcessorClient>(Strict);

            var forceDeleteRelatedData = true;

            releaseDataFilesService.Setup(mock =>
                mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData)).ReturnsAsync(Unit.Instance);

            releaseFileService.Setup(mock =>
                mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData)).ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(mock =>
                    mock.DeleteAllReleaseSubjects(releaseVersion.Id, !forceDeleteRelatedData))
                .Returns(Task.CompletedTask);

            releasePublishingStatusRepository.Setup(mock =>
                    mock.RemovePublisherReleaseStatuses(new List<Guid> { releaseVersion.Id }))
                .Returns(Task.CompletedTask);

            cacheService
                .Setup(mock => mock.DeleteCacheFolderAsync(
                    ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersion.Id))))
                .Returns(Task.CompletedTask);

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    forceDeleteRelatedData,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    releaseDataFileService: releaseDataFilesService.Object,
                    releaseFileService: releaseFileService.Object,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    cacheService: cacheService.Object,
                    processorClient: processorClient.Object);

                // Act
                var result = await releaseVersionService.DeleteTestReleaseVersion(releaseVersion.Id);

                // Assert
                releaseDataFilesService.Verify(mock =>
                        mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData),
                    Times.Once);

                releaseFileService.Verify(mock =>
                        mock.DeleteAll(releaseVersion.Id, forceDeleteRelatedData),
                    Times.Once);

                releaseSubjectRepository.Verify(mock =>
                        mock.DeleteAllReleaseSubjects(releaseVersion.Id, !forceDeleteRelatedData),
                    Times.Once);

                VerifyAllMocks(
                    cacheService,
                    releaseDataFilesService,
                    releaseFileService,
                    processorClient,
                    releasePublishingStatusRepository
                );

                result.AssertRight();

                // assert that hard-deleted entities no longer exist
                var hardDeletedRelease = await contentDbContext
                    .ReleaseVersions
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(rv => rv.Id == releaseVersion.Id);

                Assert.Null(hardDeletedRelease);

                var hardDeletedReleaseRole = await contentDbContext
                    .UserReleaseRoles
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Id == userReleaseRole.Id);

                Assert.Null(hardDeletedReleaseRole);

                var hardDeletedReleaseInvite = await contentDbContext
                    .UserReleaseInvites
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Id == userReleaseInvite.Id);

                Assert.Null(hardDeletedReleaseInvite);

                // assert that other entities were not accidentally hard-deleted
                var retrievedAnotherReleaseRole = await contentDbContext
                    .UserReleaseRoles
                    .IgnoreQueryFilters()
                    .FirstAsync(r => r.Id == anotherUserReleaseRole.Id);

                Assert.False(retrievedAnotherReleaseRole.SoftDeleted);

                var retrievedAnotherReleaseInvite = await contentDbContext
                    .UserReleaseInvites
                    .IgnoreQueryFilters()
                    .FirstAsync(r => r.Id == anotherUserReleaseInvite.Id);

                Assert.False(retrievedAnotherReleaseInvite.SoftDeleted);

                // Assert that Methodologies that were scheduled to go out with this Release are no longer scheduled
                // to do so
                var retrievedMethodologyVersion =
                    await contentDbContext.MethodologyVersions.SingleAsync(m =>
                        m.Id == methodologyScheduledWithRelease.Id);
                Assert.True(retrievedMethodologyVersion.ScheduledForPublishingImmediately);
                Assert.Null(retrievedMethodologyVersion.ScheduledWithReleaseVersionId);
                Assert.Equal(MethodologyApprovalStatus.Draft, retrievedMethodologyVersion.Status);
                Assert.InRange(DateTime.UtcNow
                        .Subtract(retrievedMethodologyVersion.Updated!.Value).Milliseconds,
                    0,
                    1500);

                // Assert that Methodologies that were scheduled to go out with other Releases remain unaffected
                var unrelatedMethodology =
                    await contentDbContext.MethodologyVersions.SingleAsync(
                        m => m.Id == methodologyScheduledWithAnotherRelease.Id);
                Assert.True(unrelatedMethodology.ScheduledForPublishingWithRelease);
                Assert.Equal(methodologyScheduledWithAnotherRelease.ScheduledWithReleaseVersionId,
                    unrelatedMethodology.ScheduledWithReleaseVersionId);

                // We expect the Statistics ReleaseVersion to be deleted immediately for test ReleaseVersions,
                // as they do not use Subjects large enough to warrant using the stored procedure that cleans up
                // soft-deleted Subjects.
                Assert.Null(await statisticsDbContext
                    .ReleaseVersion
                    .SingleOrDefaultAsync(rv => rv.Id == statisticsReleaseVersion.Id));
            }
        }

        [Fact]
        public async Task ProcessorReturns400_Returns400()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Theme = new Theme() }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var processorClient = new Mock<IProcessorClient>(Strict);

            var forceDeletePublicApiData = true;

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    forceDeletePublicApiData,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BadRequestObjectResult(new ValidationProblemViewModel
                {
                    Errors = new ErrorViewModel[]
                    {
                        new()
                        {
                            Path = "error path",
                            Code = "error code"
                        }
                    }
                }));

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            await using var contentDbContext = InMemoryApplicationDbContext(contextId);
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    processorClient: processorClient.Object);

                var result = await releaseVersionService.DeleteTestReleaseVersion(releaseVersion.Id);

                VerifyAllMocks(processorClient);

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                validationProblem.AssertHasError(
                    expectedPath: "error path",
                    expectedCode: "error code");
            }
        }

        [Fact]
        public async Task ProcessorThrows_Throws()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Theme = new Theme() }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var processorClient = new Mock<IProcessorClient>(Strict);

            var forceDeletePublicApiData = true;

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    forceDeletePublicApiData,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            await using var contentDbContext = InMemoryApplicationDbContext(contextId);
            {
                var releaseVersionService = BuildReleaseVersionService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    processorClient: processorClient.Object);

                await Assert.ThrowsAsync<HttpRequestException>(async () =>
                    await releaseVersionService.DeleteTestReleaseVersion(releaseVersion.Id));

                VerifyAllMocks(processorClient);
            }
        }
    }

    public class UpdateReleasePublishedTests : ReleaseVersionServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions =
                [
                    new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                ],
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest { Published = DateTime.UtcNow.AddDays(-1) };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

            releaseCacheService.Setup(s => s.UpdateRelease(releaseVersionId,
                publication.Slug,
                publication.ReleaseVersions[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseVersionId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(contentDbContext: context,
                    releaseCacheService: releaseCacheService.Object);

                var result = await service
                    .UpdateReleasePublished(
                        releaseVersionId,
                        request
                    );

                VerifyAllMocks(releaseCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.ReleaseVersions
                    .SingleAsync(rv => rv.Id == releaseVersionId);

                Assert.Equal(request.Published, saved.Published);
            }
        }

        [Fact]
        public async Task LatestReleaseInPublication()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = releaseVersionId,
                ReleaseVersions =
                [
                    new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                ],
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest { Published = DateTime.UtcNow.AddDays(-1) };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

            releaseCacheService.Setup(s => s.UpdateRelease(releaseVersionId,
                publication.Slug,
                publication.ReleaseVersions[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseVersionId));

            // As the release is the latest for the publication the separate cache entry for the publication's latest
            // release should also be updated
            releaseCacheService.Setup(s => s.UpdateRelease(releaseVersionId,
                publication.Slug,
                null
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseVersionId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(contentDbContext: context,
                    releaseCacheService: releaseCacheService.Object);

                var result = await service
                    .UpdateReleasePublished(
                        releaseVersionId,
                        request
                    );

                VerifyAllMocks(releaseCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.ReleaseVersions
                    .SingleAsync(rv => rv.Id == releaseVersionId);

                Assert.Equal(request.Published, saved.Published);
            }
        }

        [Fact]
        public async Task ReleaseNotPublished()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions =
                [
                    new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Slug = "release-slug",
                        Published = null
                    }
                ],
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest { Published = DateTime.UtcNow.AddDays(-1) };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(context);

                var result = await service
                    .UpdateReleasePublished(
                        releaseVersionId,
                        request
                    );

                result.AssertBadRequest(ValidationErrorMessages.ReleaseNotPublished);
            }
        }

        [Fact]
        public async Task FutureDate()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions =
                [
                    new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                ],
                Slug = "publication-slug"
            };

            var request = new ReleasePublishedUpdateRequest { Published = DateTime.UtcNow.AddDays(1) };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(context);

                var result = await service
                    .UpdateReleasePublished(
                        releaseVersionId,
                        request
                    );

                result.AssertBadRequest(ReleasePublishedCannotBeFutureDate);
            }
        }

        [Fact]
        public async Task ConvertsPublishedFromLocalToUniversalTimezone()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions =
                [
                    new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Slug = "release-slug",
                        Published = DateTime.UtcNow
                    }
                ],
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

            releaseCacheService.Setup(s => s.UpdateRelease(releaseVersionId,
                publication.Slug,
                publication.ReleaseVersions[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseVersionId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(contentDbContext: context,
                    releaseCacheService: releaseCacheService.Object);

                var result = await service
                    .UpdateReleasePublished(
                        releaseVersionId,
                        request
                    );

                VerifyAllMocks(releaseCacheService);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.ReleaseVersions
                    .SingleAsync(rv => rv.Id == releaseVersionId);

                // The published date retrieved from the database should always be represented in UTC
                // because of the conversion setup in the database context config.
                Assert.Equal(DateTimeKind.Utc, saved.Published!.Value.Kind);

                // Make sure the request date was converted to UTC before it was updated on the release
                Assert.Equal(DateTime.Parse("2022-08-08T08:30:00Z", styles: DateTimeStyles.AdjustToUniversal),
                    saved.Published);
            }
        }
    }

    public class ListUsersReleasesForApprovalTests : ReleaseVersionServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task UserHasApproverRoleOnReleaseVersion()
        {
            var contextId = Guid.NewGuid().ToString();

            var otherUser = new User();

            var publications = _fixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                            .Generate(1)),
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.HigherLevelReview)
                            .Generate(1)),
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                            .Generate(1))
                ))
                .GenerateList(4);

            var contributorReleaseRolesForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithRole(ReleaseRole.Contributor)
                .WithReleaseVersions(publications[0].Releases.SelectMany(r => r.Versions))
                .GenerateList();

            var approverReleaseRolesForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithRole(ReleaseRole.Approver)
                .WithReleaseVersions(publications[1].Releases.SelectMany(r => r.Versions))
                .GenerateList();

            var prereleaseReleaseRolesForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithRole(ReleaseRole.PrereleaseViewer)
                .WithReleaseVersions(publications[2].Releases.SelectMany(r => r.Versions))
                .GenerateList();

            var approverReleaseRolesForOtherUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(otherUser)
                .WithRole(ReleaseRole.Approver)
                .WithReleaseVersions(publications.SelectMany(publication =>
                    publication.Releases.SelectMany(r => r.Versions)))
                .GenerateList();

            var higherReviewReleaseVersionWithApproverRoleForUser = publications[1].Releases[1].Versions.Single();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publications);
                context.UserReleaseRoles.AddRange(contributorReleaseRolesForUser);
                context.UserReleaseRoles.AddRange(approverReleaseRolesForUser);
                context.UserReleaseRoles.AddRange(prereleaseReleaseRolesForUser);
                context.UserReleaseRoles.AddRange(approverReleaseRolesForOtherUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(context);

                var result = await service.ListUsersReleasesForApproval();

                var viewModels = result.AssertRight();

                // Assert that the only Release returned for this user is the Release where they have a direct
                // Approver role on, and it is in Higher Review.
                var viewModel = Assert.Single(viewModels);
                Assert.Equal(higherReviewReleaseVersionWithApproverRoleForUser.Id, viewModel.Id);

                // Assert that we have a fully populated ReleaseSummaryViewModel, including details from the owning
                // Publication.
                Assert.Equal(
                    higherReviewReleaseVersionWithApproverRoleForUser.Publication.Title,
                    viewModel.Publication!.Title);
            }
        }

        [Fact]
        public async Task UserHasApproverRoleOnPublications()
        {
            var contextId = Guid.NewGuid().ToString();

            var otherUser = new User();

            var publications = _fixture
                .DefaultPublication()
                .WithReleases(_ => ListOf<Release>(
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                            .Generate(1)),
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.HigherLevelReview)
                            .Generate(1)),
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                            .Generate(1)),
                    _fixture.DefaultRelease()
                        .WithVersions(_ => _fixture
                            .DefaultReleaseVersion()
                            .WithApprovalStatus(ReleaseApprovalStatus.HigherLevelReview)
                            .Generate(1))
                ))
                .GenerateList(3);

            var ownerPublicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithRole(PublicationRole.Owner)
                .WithPublication(publications[0])
                .Generate();

            var approverPublicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithRole(PublicationRole.Approver)
                .WithPublication(publications[1])
                .Generate();

            var ownerPublicationRolesForOtherUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(otherUser)
                .WithRole(PublicationRole.Owner)
                .WithPublications(publications)
                .GenerateList();

            var approverPublicationRolesForOtherUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(otherUser)
                .WithRole(PublicationRole.Approver)
                .WithPublications(publications)
                .GenerateList();

            var releaseVersion1WithApproverRoleForUser = publications[1].Releases[1].Versions.Single();
            var releaseVersion2WithApproverRoleForUser = publications[1].Releases[3].Versions.Single();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publications);
                context.UserPublicationRoles.AddRange(ownerPublicationRoleForUser, approverPublicationRoleForUser);
                context.UserPublicationRoles.AddRange(ownerPublicationRolesForOtherUser);
                context.UserPublicationRoles.AddRange(approverPublicationRolesForOtherUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(context);

                var result = await service.ListUsersReleasesForApproval();

                var viewModels = result.AssertRight();

                // Assert that the only Releases returned for this user are the Releases where they have Approver
                // role on the overarching Publication and the Releases are in Higher Review.
                Assert.Equal(2, viewModels.Count);
                Assert.Equal(releaseVersion1WithApproverRoleForUser.Id, viewModels[0].Id);
                Assert.Equal(releaseVersion2WithApproverRoleForUser.Id, viewModels[1].Id);
            }
        }

        [Fact]
        public async Task UserIsPublicationAndReleaseApprover_NoDuplication()
        {
            var contextId = Guid.NewGuid().ToString();

            Publication publication = _fixture
                .DefaultPublication()
                .WithReleases(_ => _fixture.DefaultRelease()
                    .WithVersions(_ => _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate(1));

            var approverReleaseRolesForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithRole(ReleaseRole.Approver)
                .WithReleaseVersions(publication.Releases.Single().Versions)
                .GenerateList();

            var approverPublicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithRole(PublicationRole.Approver)
                .WithPublication(publication)
                .Generate();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publication);
                context.UserReleaseRoles.AddRange(approverReleaseRolesForUser);
                context.UserPublicationRoles.AddRange(approverPublicationRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseVersionService(context);

                var result = await service.ListUsersReleasesForApproval();

                var viewModels = result.AssertRight();

                // Assert that the Release only appears once despite the user having approval directly via the
                // Release itself AND via the overarching Publication.
                Assert.Single(viewModels);
                Assert.Equal(publication.ReleaseVersions[0].Id, viewModels[0].Id);
            }
        }
    }

    private static ReleaseVersionService BuildReleaseVersionService(
        ContentDbContext contentDbContext,
        StatisticsDbContext? statisticsDbContext = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IReleaseCacheService? releaseCacheService = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IReleaseFileService? releaseFileService = null,
        IReleaseDataFileService? releaseDataFileService = null,
        IDataImportService? dataImportService = null,
        IFootnoteRepository? footnoteRepository = null,
        IDataBlockService? dataBlockService = null,
        IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
        IReleaseSubjectRepository? releaseSubjectRepository = null,
        IDataSetVersionService? dataSetVersionService = null,
        IProcessorClient? processorClient = null,
        IBlobCacheService? cacheService = null)
    {
        var userService = AlwaysTrueUserService();

        userService
            .Setup(s => s.GetUserId())
            .Returns(User.Id);

        return new ReleaseVersionService(
            contentDbContext,
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
            AdminMapper(),
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService.Object,
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
            releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
            releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
            releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
            releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
            dataImportService ?? Mock.Of<IDataImportService>(Strict),
            footnoteRepository ?? Mock.Of<IFootnoteRepository>(Strict),
            dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
            releasePublishingStatusRepository ?? Mock.Of<IReleasePublishingStatusRepository>(Strict),
            releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
            dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
            processorClient ?? Mock.Of<IProcessorClient>(Strict),
            new SequentialGuidGenerator(),
            cacheService ?? Mock.Of<IBlobCacheService>(Strict)
        );
    }
}