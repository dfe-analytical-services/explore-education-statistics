#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
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
using System.Net.Http;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {
        private readonly DataFixture _dataFixture = new();
        private static readonly User User = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task CreateRelease_ReleaseTypeExperimentalStatistics_ReturnsValidationActionResult()
        {
            var releaseCreateRequest = new ReleaseCreateRequest
            {
                Type = ReleaseType.ExperimentalStatistics,
            };

            var releaseService = BuildReleaseService(Mock.Of<ContentDbContext>());

            var result = await releaseService.CreateRelease(releaseCreateRequest);

            result.AssertBadRequest(ReleaseTypeInvalid);
        }

        [Fact]
        public async Task UpdateReleaseVersion_ReleaseTypeExperimentalStatistics_ReturnsValidationActionResult()
        {
            var releaseUpdateRequest = new ReleaseUpdateRequest
            {
                Type = ReleaseType.ExperimentalStatistics,
            };

            var releaseService = BuildReleaseService(Mock.Of<ContentDbContext>());

            var result = await releaseService.UpdateReleaseVersion(It.IsAny<Guid>(), releaseUpdateRequest);

            result.AssertBadRequest(ReleaseTypeInvalid);
        }

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
                var actual = await context
                    .ReleaseVersions
                    .SingleAsync(rv => rv.PublicationId == publication.Id);

                Assert.Equal(2018, actual.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actual.TimePeriodCoverage);
                Assert.Equal(ReleaseType.OfficialStatistics, actual.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, actual.ApprovalStatus);
                Assert.Equal(0, actual.Version);
                Assert.NotEqual(Guid.Empty, actual.ReleaseId);

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
            var templateReleaseId = Guid.NewGuid();

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
                },
                ReleaseVersionId = templateReleaseId
            };

            var dataBlock2 = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Data Block 2",
                ReleaseVersionId = templateReleaseId
            };

            var templateRelease = new ReleaseVersion
            {
                Id = templateReleaseId,
                ReleaseName = "2018",
                Content = ListOf(new ContentSection
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
                            Body = "<div></div>",
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
                }),
                Version = 0
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(
                    new Publication
                    {
                        Id = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        Title = "Publication",
                        ReleaseVersions = ListOf(templateRelease)
                    }
                );
                await context.ContentBlocks.AddRangeAsync(dataBlock1, dataBlock2);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId;

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.CreateRelease(
                    new ReleaseCreateRequest
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = templateReleaseId,
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
                var newReleaseVersion = context
                    .ReleaseVersions
                    .Include(releaseVersion => releaseVersion.Content)
                    .ThenInclude(section => section.Content)
                    .Single(rv => rv.Id == newReleaseVersionId);

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
                Assert.Equal(ContentSectionType.KeyStatisticsSecondary, newReleaseVersion.KeyStatisticsSecondarySection.Type);

                // Data Blocks should not be copied when created from a template.
                Assert.Equal(2, context.DataBlocks.Count());
                Assert.Empty(context
                    .DataBlocks
                    .Where(dataBlock => dataBlock.ReleaseVersionId == newReleaseVersionId));
            }
        }

        [Fact]
        public async Task GetDeleteDataFilePlan()
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
            dataBlockService.Setup(service => service.GetDeletePlan(releaseVersion.Id, It.Is<Subject>(s => s.Id == subject.Id)))
                .ReturnsAsync(deleteDataBlockPlan);

            var footnote = new Footnote { Id = Guid.NewGuid() };
            footnoteRepository.Setup(service => service.GetFootnotes(releaseVersion.Id, subject.Id))
                .ReturnsAsync([footnote]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object);

                var result = await releaseService.GetDeleteDataFilePlan(
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
        public async Task GetDeleteDataFilePlan_FileIsLinkedToPublicApiDataSet_InvalidPlan()
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
            dataBlockService.Setup(service => service.GetDeletePlan(releaseVersion.Id, It.Is<Subject>(s => s.Id == subject.Id)))
                .ReturnsAsync(deleteDataBlockPlan);

            var footnote = new Footnote { Id = Guid.NewGuid() };
            footnoteRepository.Setup(service => service.GetFootnotes(releaseVersion.Id, subject.Id))
                .ReturnsAsync([footnote]);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataSetVersionService: dataSetVersionService.Object,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object);

                var result = await releaseService.GetDeleteDataFilePlan(
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
                Assert.Equal(dataSetVersion.Version, deleteDataFilePlan.DeleteApiDataSetVersionPlan.Version);
                Assert.Equal(dataSetVersion.Status, deleteDataFilePlan.DeleteApiDataSetVersionPlan.Status);
                Assert.False(deleteDataFilePlan.DeleteApiDataSetVersionPlan.Valid);
                Assert.False(deleteDataFilePlan.Valid);
            }
        }

        [Fact]
        public async Task RemoveDataFiles()
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
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.COMPLETE
                });

            footnoteRepository.Setup(service => service.GetFootnotes(releaseVersion.Id, subject.Id))
                .ReturnsAsync(new List<Footnote>());

            releaseDataFileService.Setup(service => service.Delete(releaseVersion.Id, file.Id, false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service => service.DeleteReleaseSubject(releaseVersion.Id, subject.Id, true))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context,
                    statisticsDbContext,
                    cacheService: cacheService.Object,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(releaseVersionId: releaseVersion.Id,
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
        public async Task RemoveDataFiles_FileImporting()
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
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.STAGE_1
                });

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataImportService: dataImportService.Object);

                var result = await releaseService.RemoveDataFiles(releaseVersionId: releaseVersion.Id,
                    fileId: file.Id);

                VerifyAllMocks(dataImportService);

                result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
            }
        }

        [Fact]
        public async Task RemoveDataFiles_ReplacementExists()
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
                    service.DeleteItemAsync(new PrivateSubjectMetaCacheKey(releaseVersion.Id, replacementSubject.Id)))
                .Returns(Task.CompletedTask);

            dataBlockService.Setup(service =>
                    service.GetDeletePlan(releaseVersion.Id, It.Is<Subject>(s =>
                        new[] { subject.Id, replacementSubject.Id }.Contains(s.Id))))
                .ReturnsAsync(new DeleteDataBlockPlanViewModel());

            dataBlockService.Setup(service => service.DeleteDataBlocks(It.IsAny<DeleteDataBlockPlanViewModel>()))
                .ReturnsAsync(Unit.Instance);

            dataImportService.Setup(service => service.GetImport(It.IsIn(file.Id, replacementFile.Id)))
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.COMPLETE
                });

            footnoteRepository.Setup(service =>
                    service.GetFootnotes(releaseVersion.Id, It.IsIn(subject.Id, replacementSubject.Id)))
                .ReturnsAsync(new List<Footnote>());

            releaseDataFileService
                .Setup(service => service.Delete(releaseVersion.Id, It.IsIn(file.Id, replacementFile.Id), false))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(service =>
                    service.DeleteReleaseSubject(releaseVersion.Id, It.IsIn(subject.Id, replacementSubject.Id), true))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
                    context,
                    statisticsDbContext,
                    cacheService: cacheService.Object,
                    dataBlockService: dataBlockService.Object,
                    dataImportService: dataImportService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var result = await releaseService.RemoveDataFiles(
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
                        It.IsIn(subject.Id, replacementSubject.Id), true),
                    Times.Exactly(2));

                result.AssertRight();
            }
        }

        [Fact]
        public async Task RemoveDataFiles_ReplacementFileImporting()
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

                var result = await releaseService.RemoveDataFiles(releaseVersionId: releaseVersion.Id,
                    fileId: file.Id);

                VerifyAllMocks(dataImportService);

                result.AssertBadRequest(CannotRemoveDataFilesUntilImportComplete);
            }
        }

        [Fact]
        public async Task RemoveDataFiles_FileIsLinkedToPublicApiDataSet_ValidationProblem()
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
                .ReturnsAsync(new DataImport
                {
                    Status = DataImportStatus.COMPLETE
                });

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseService = BuildReleaseService(context,
                    dataImportService: dataImportService.Object);

                var result = await releaseService.RemoveDataFiles(releaseVersionId: releaseVersion.Id,
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

        [Fact]
        public async Task UpdateReleaseVersion()
        {
            var releaseVersion = new ReleaseVersion
            {
                Type = ReleaseType.AdHocStatistics,
                Publication = new Publication(),
                ReleaseName = "2030",
                PublishScheduled = DateTime.UtcNow,
                NextReleaseDate = new PartialDate {Day = "15", Month = "6", Year = "2039"},
                PreReleaseAccessList = "Old access list",
                Version = 0
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.AddRange(releaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService
                    .UpdateReleaseVersion(
                        releaseVersion.Id,
                        new ReleaseUpdateRequest
                        {
                            Type = ReleaseType.OfficialStatistics,
                            Year = 2035,
                            TimePeriodCoverage = TimeIdentifier.March,
                            PreReleaseAccessList = "New access list",
                        }
                    );

                var viewModel = result.AssertRight();

                Assert.Equal(releaseVersion.Publication.Id, viewModel.PublicationId);
                Assert.Equal(releaseVersion.NextReleaseDate, viewModel.NextReleaseDate);
                Assert.Equal(ReleaseType.OfficialStatistics, viewModel.Type);
                Assert.Equal(2035, viewModel.Year);
                Assert.Equal("2035", viewModel.YearTitle);
                Assert.Equal(TimeIdentifier.March, viewModel.TimePeriodCoverage);
                Assert.Equal("New access list", viewModel.PreReleaseAccessList);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var saved = await context.ReleaseVersions
                    .Include(rv => rv.ReleaseStatuses)
                    .FirstAsync(rv => rv.Id == releaseVersion.Id);

                Assert.Equal(releaseVersion.Publication.Id, saved.PublicationId);
                Assert.Equal(releaseVersion.NextReleaseDate, saved.NextReleaseDate);
                Assert.Equal(ReleaseType.OfficialStatistics, saved.Type);
                Assert.Equal("2035-march", saved.Slug);
                Assert.Equal("2035", saved.ReleaseName);
                Assert.Equal(TimeIdentifier.March, saved.TimePeriodCoverage);
                Assert.Equal("New access list", saved.PreReleaseAccessList);

                Assert.Empty(saved.ReleaseStatuses);
            }
        }

        [Fact]
        public async Task UpdateReleaseVersion_FailsNonUniqueSlug()
        {
            var publication = new Publication();

            var releaseVersion = new ReleaseVersion
            {
                Type = ReleaseType.AdHocStatistics,
                Publication = publication,
                ReleaseName = "2030",
                Slug = "2030",
                PublishScheduled = DateTime.UtcNow,
                Version = 0
            };

            var otherRelease = new ReleaseVersion
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
                context.ReleaseVersions.AddRange(releaseVersion, otherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService
                    .UpdateReleaseVersion(
                        releaseVersion.Id,
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

            var releaseVersion = new ReleaseVersion
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
                Slug = "test-publication",
                LatestPublishedReleaseVersion = releaseVersion,
                ReleaseVersions =
                {
                    releaseVersion
                }
            };

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
                await context.Publications.AddAsync(publication);
                await context.UserReleaseRoles.AddAsync(nonPreReleaseUserRole);
                await context.UserReleaseInvites.AddAsync(nonPreReleaseUserInvite);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetRelease(releaseVersion.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(releaseVersion.Id, viewModel.Id);
                Assert.Equal("January 2035", viewModel.Title);
                Assert.Equal(2035, viewModel.Year);
                Assert.Equal("2035", viewModel.YearTitle);
                Assert.Equal("2035-1", viewModel.Slug);
                Assert.Equal(publication.Id, viewModel.PublicationId);
                Assert.Equal("Test publication", viewModel.PublicationTitle);
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
            var releaseVersion = new ReleaseVersion
            {
                TimePeriodCoverage = TimeIdentifier.January,
                ReleaseName = "2035",
            };

            var publication = new Publication
            {
                ReleaseVersions =
                {
                    releaseVersion
                }
            };

            var preReleaseUserRole = new UserReleaseRole
            {
                Role = ReleaseRole.PrereleaseViewer,
                ReleaseVersion = releaseVersion
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

                var result = await releaseService.GetRelease(releaseVersion.Id);

                var viewModel = result.AssertRight();
                Assert.True(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task GetRelease_WithPreReleaseInvites()
        {
            var releaseVersion = new ReleaseVersion
            {
                TimePeriodCoverage = TimeIdentifier.January,
                ReleaseName = "2035",
            };

            var publication = new Publication
            {
                ReleaseVersions =
                {
                    releaseVersion
                }
            };

            var preReleaseUserInvite = new UserReleaseInvite
            {
                Role = ReleaseRole.PrereleaseViewer,
                ReleaseVersion = releaseVersion
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

                var result = await releaseService.GetRelease(releaseVersion.Id);

                var viewModel = result.AssertRight();
                Assert.True(viewModel.PreReleaseUsersOrInvitesAdded);
            }
        }

        [Fact]
        public async Task GetRelease_NotLatestPublishedRelease()
        {
            var publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    ReleaseName = "2022",
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    Published = DateTime.UtcNow
                }
            };

            var notLatestReleaseVersion = new ReleaseVersion
            {
                Publication = publication,
                ReleaseName = "2021",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                Published = DateTime.UtcNow
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseVersions.Add(notLatestReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context);
                var result = await releaseService.GetRelease(notLatestReleaseVersion.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(notLatestReleaseVersion.Id, viewModel.Id);
                Assert.False(viewModel.LatestRelease);
            }
        }

        [Fact]
        public async Task GetRelease_NoLatestInternalReleaseNote()
        {
            var releaseVersion = new ReleaseVersion
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.CalendarYear
            };

            var publication = new Publication
            {
                ReleaseVersions = new List<ReleaseVersion>
                {
                    releaseVersion
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

                var result = await releaseService.GetRelease(releaseVersion.Id);

                var releaseViewModel = result.AssertRight();
                Assert.Null(releaseViewModel.LatestInternalReleaseNote);
            }
        }

        [Fact]
        public async Task GetLatestPublishedRelease()
        {
            var publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion
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
                Assert.Equal(publication.LatestPublishedReleaseVersionId, latestIdTitleViewModel.Id);
                Assert.Equal("Calendar year 2022", latestIdTitleViewModel.Title);
            }
        }

        [Fact]
        public async Task GetLatestPublishedRelease_NoPublishedRelease()
        {
            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = null
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
        public async Task GetDeleteReleaseVersionPlan()
        {
            var releaseBeingDeleted = new ReleaseVersion
            {
                Id = Guid.NewGuid()
            };

            // This is just another unrelated Release that should not be affected.
            var releaseNotBeingDeleted = new ReleaseVersion
            {
                Id = Guid.NewGuid()
            };

            var methodology1ScheduledWithRelease1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseVersionId = releaseBeingDeleted.Id,
                AlternativeTitle = "Methodology 1 with alternative title",
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology 1 owned Publication title"
                }
            };

            var methodology2ScheduledWithRelease1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseVersionId = releaseBeingDeleted.Id,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology 2 with owned Publication title"
                }
            };

            var methodologyScheduledWithRelease2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                ScheduledWithReleaseVersionId = releaseNotBeingDeleted.Id
            };

            var methodologyNotScheduled = new MethodologyVersion
            {
                Id = Guid.NewGuid()
            };

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
                var releaseService = BuildReleaseService(context);

                var result = await releaseService.GetDeleteReleaseVersionPlan(releaseBeingDeleted.Id);

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
        public async Task DeleteReleaseVersion()
        {
            var publication = new Publication();

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Version = 0,
            };

            // This Methodology is scheduled to go out with the Release being deleted.
            var methodologyScheduledWithRelease = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                ScheduledWithReleaseVersionId = releaseVersion.Id,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Methodology scheduled with this Release"
                },
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

            var userReleaseInvite = new UserReleaseInvite
            {
                ReleaseVersion = releaseVersion
            };

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
                context.ReleaseVersions.AddRange(releaseVersion, anotherRelease);
                context.UserReleaseRoles.AddRange(userReleaseRole, anotherUserReleaseRole);
                context.UserReleaseInvites.AddRange(userReleaseInvite, anotherUserReleaseInvite);
                context.MethodologyVersions.AddRange(methodologyScheduledWithRelease, methodologyScheduledWithAnotherRelease);
                await context.SaveChangesAsync();
            }

            var releaseDataFilesService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var cacheService = new Mock<IBlobCacheService>(Strict);
            var processorClient = new Mock<IProcessorClient>(Strict);

            releaseDataFilesService.Setup(mock =>
                mock.DeleteAll(releaseVersion.Id, false)).ReturnsAsync(Unit.Instance);

            releaseFileService.Setup(mock =>
                mock.DeleteAll(releaseVersion.Id, false)).ReturnsAsync(Unit.Instance);

            releaseSubjectRepository.Setup(mock =>
                mock.DeleteAllReleaseSubjects(releaseVersion.Id, true)).Returns(Task.CompletedTask);

            cacheService
                .Setup(mock => mock.DeleteCacheFolderAsync(
                    ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersion.Id))))
                .Returns(Task.CompletedTask);

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(context,
                    releaseDataFileService: releaseDataFilesService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    cacheService: cacheService.Object,
                    processorClient: processorClient.Object);

                var result = await releaseService.DeleteReleaseVersion(releaseVersion.Id);

                releaseDataFilesService.Verify(mock =>
                    mock.DeleteAll(releaseVersion.Id, false), Times.Once);

                releaseFileService.Verify(mock =>
                    mock.DeleteAll(releaseVersion.Id, false), Times.Once);

                VerifyAllMocks(cacheService,
                    releaseDataFilesService,
                    releaseFileService,
                    processorClient
                );

                result.AssertRight();

                // assert that soft-deleted entities are no longer discoverable by default
                var unableToFindDeletedRelease = context
                    .ReleaseVersions
                    .FirstOrDefault(rv => rv.Id == releaseVersion.Id);

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
                    .Include(p => p.ReleaseVersions)
                    .AsNoTracking()
                    .First(p => p.Id == publication.Id);

                Assert.Single(publicationWithoutDeletedRelease.ReleaseVersions);
                Assert.Equal(anotherRelease.Id, publicationWithoutDeletedRelease.ReleaseVersions[0].Id);

                // assert that soft-deleted entities have had their soft-deleted flag set to true
                var updatedRelease = context
                    .ReleaseVersions
                    .IgnoreQueryFilters()
                    .First(rv => rv.Id == releaseVersion.Id);

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
                    .Include(p => p.ReleaseVersions)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .First(p => p.Id == publication.Id);

                Assert.Equal(2, publicationWithDeletedRelease.ReleaseVersions.Count);
                Assert.Equal(updatedRelease.Id, publicationWithDeletedRelease.ReleaseVersions[0].Id);
                Assert.Equal(anotherRelease.Id, publicationWithDeletedRelease.ReleaseVersions[1].Id);
                Assert.True(publicationWithDeletedRelease.ReleaseVersions[0].SoftDeleted);
                Assert.False(publicationWithDeletedRelease.ReleaseVersions[1].SoftDeleted);

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
                var retrievedMethodologyVersion = context.MethodologyVersions.Single(m => m.Id == methodologyScheduledWithRelease.Id);
                Assert.True(retrievedMethodologyVersion.ScheduledForPublishingImmediately);
                Assert.Null(retrievedMethodologyVersion.ScheduledWithReleaseVersionId);
                Assert.Equal(MethodologyApprovalStatus.Draft, retrievedMethodologyVersion.Status);
                Assert.InRange(DateTime.UtcNow
                    .Subtract(retrievedMethodologyVersion.Updated!.Value).Milliseconds, 0, 1500);

                // Assert that Methodologies that were scheduled to go out with other Releases remain unaffected
                var unrelatedMethodology = context.MethodologyVersions.Single(m => m.Id == methodologyScheduledWithAnotherRelease.Id);
                Assert.True(unrelatedMethodology.ScheduledForPublishingWithRelease);
                Assert.Equal(methodologyScheduledWithAnotherRelease.ScheduledWithReleaseVersionId,
                    unrelatedMethodology.ScheduledWithReleaseVersionId);
            }
        }

        [Fact]
        public async Task DeleteReleaseVersion_ProcessorReturns400_Returns400()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var processorClient = new Mock<IProcessorClient>(Strict);

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BadRequestObjectResult(new ValidationProblemViewModel
                    {
                        Errors = new ErrorViewModel[]
                        {
                            new() {
                                Path ="error path",
                                Code = "error code"
                            }
                        }
                    }));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
                    context,
                    processorClient: processorClient.Object);

                var result = await releaseService.DeleteReleaseVersion(releaseVersion.Id);

                VerifyAllMocks(processorClient);

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                validationProblem.AssertHasError(
                    expectedPath: "error path",
                    expectedCode: "error code");
            }
        }

        [Fact]
        public async Task DeleteReleaseVersion_ProcessorThrows_Throws()
        {
            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.ReleaseVersions.Add(releaseVersion);
                await context.SaveChangesAsync();
            }

            var processorClient = new Mock<IProcessorClient>(Strict);

            processorClient.Setup(mock => mock.BulkDeleteDataSetVersions(
                    releaseVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildReleaseService(
                    context,
                    processorClient: processorClient.Object);

                await Assert.ThrowsAsync<HttpRequestException>(async () => await releaseService.DeleteReleaseVersion(releaseVersion.Id));

                VerifyAllMocks(processorClient);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new()
                    {
                        Id = releaseVersionId,
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

            releaseCacheService.Setup(s => s.UpdateRelease(releaseVersionId,
                publication.Slug,
                publication.ReleaseVersions[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseVersionId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(contentDbContext: context,
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
        public async Task UpdateReleasePublished_LatestReleaseInPublication()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = releaseVersionId,
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new()
                    {
                        Id = releaseVersionId,
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
                var service = BuildReleaseService(contentDbContext: context,
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
        public async Task UpdateReleasePublished_ReleaseNotPublished()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new()
                    {
                        Id = releaseVersionId,
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
                        releaseVersionId,
                        request
                    );

                result.AssertBadRequest(ReleaseNotPublished);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished_FutureDate()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new()
                    {
                        Id = releaseVersionId,
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
                        releaseVersionId,
                        request
                    );

                result.AssertBadRequest(ReleasePublishedCannotBeFutureDate);
            }
        }

        [Fact]
        public async Task UpdateReleasePublished_ConvertsPublishedFromLocalToUniversalTimezone()
        {
            var releaseVersionId = Guid.NewGuid();

            var publication = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid(),
                ReleaseVersions = new List<ReleaseVersion>
                {
                    new()
                    {
                        Id = releaseVersionId,
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

            releaseCacheService.Setup(s => s.UpdateRelease(releaseVersionId,
                publication.Slug,
                publication.ReleaseVersions[0].Slug
            )).ReturnsAsync(new ReleaseCacheViewModel(releaseVersionId));

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var service = BuildReleaseService(contentDbContext: context,
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
                Assert.Equal(DateTime.Parse("2022-08-08T08:30:00Z", styles: DateTimeStyles.AdjustToUniversal), saved.Published);
            }
        }

        public class ListUsersReleasesForApproval
        {
            private readonly DataFixture _fixture = new();

            [Fact]
            public async Task UserHasApproverRoleOnRelease()
            {
                var contextId = Guid.NewGuid().ToString();

                var otherUser = new User();

                var publications = _fixture
                    .DefaultPublication()
                    .WithReleaseVersions(_ => _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatuses(ListOf(
                            ReleaseApprovalStatus.Draft,
                            ReleaseApprovalStatus.HigherLevelReview,
                            ReleaseApprovalStatus.Approved))
                        .GenerateList())
                    .GenerateList(4);

                var contributorReleaseRolesForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRole(ReleaseRole.Contributor)
                    .WithReleaseVersions(publications[0].ReleaseVersions)
                    .GenerateList();

                var approverReleaseRolesForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRole(ReleaseRole.Approver)
                    .WithReleaseVersions(publications[1].ReleaseVersions)
                    .GenerateList();

                var prereleaseReleaseRolesForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRole(ReleaseRole.PrereleaseViewer)
                    .WithReleaseVersions(publications[2].ReleaseVersions)
                    .GenerateList();

                var approverReleaseRolesForOtherUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(otherUser)
                    .WithRole(ReleaseRole.Approver)
                    .WithReleaseVersions(publications.SelectMany(publication => publication.ReleaseVersions))
                    .GenerateList();

                var higherReviewReleaseWithApproverRoleForUser = publications[1].ReleaseVersions[1];

                await using (var context = InMemoryApplicationDbContext(contextId))
                {
                    await context.Publications.AddRangeAsync(publications);
                    await context.UserReleaseRoles.AddRangeAsync(contributorReleaseRolesForUser);
                    await context.UserReleaseRoles.AddRangeAsync(approverReleaseRolesForUser);
                    await context.UserReleaseRoles.AddRangeAsync(prereleaseReleaseRolesForUser);
                    await context.UserReleaseRoles.AddRangeAsync(approverReleaseRolesForOtherUser);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryApplicationDbContext(contextId))
                {
                    var service = BuildReleaseService(context);

                    var result = await service.ListUsersReleasesForApproval();

                    var viewModels = result.AssertRight();

                    // Assert that the only Release returned for this user is the Release where they have a direct
                    // Approver role on and it is in Higher Review.
                    var viewModel = Assert.Single(viewModels);
                    Assert.Equal(higherReviewReleaseWithApproverRoleForUser.Id, viewModel.Id);

                    // Assert that we have a fully populated ReleaseSummaryViewModel, including details from the owning
                    // Publication.
                    Assert.Equal(
                        higherReviewReleaseWithApproverRoleForUser.Publication.Title,
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
                    .WithReleaseVersions(_ => _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatuses(ListOf(
                            ReleaseApprovalStatus.Draft,
                            ReleaseApprovalStatus.HigherLevelReview,
                            ReleaseApprovalStatus.Approved,
                            ReleaseApprovalStatus.HigherLevelReview))
                        .GenerateList())
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

                var release1WithApproverRoleForUser = publications[1].ReleaseVersions[1];
                var release2WithApproverRoleForUser = publications[1].ReleaseVersions[3];

                await using (var context = InMemoryApplicationDbContext(contextId))
                {
                    await context.Publications.AddRangeAsync(publications);
                    await context.UserPublicationRoles.AddRangeAsync(
                        ownerPublicationRoleForUser,
                        approverPublicationRoleForUser);
                    await context.UserPublicationRoles.AddRangeAsync(ownerPublicationRolesForOtherUser);
                    await context.UserPublicationRoles.AddRangeAsync(approverPublicationRolesForOtherUser);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryApplicationDbContext(contextId))
                {
                    var service = BuildReleaseService(context);

                    var result = await service.ListUsersReleasesForApproval();

                    var viewModels = result.AssertRight();

                    // Assert that the only Releases returned for this user are the Releases where they have Approver
                    // role on the overarching Publication and the Releases are in Higher Review.
                    Assert.Equal(2, viewModels.Count);
                    Assert.Equal(release1WithApproverRoleForUser.Id, viewModels[0].Id);
                    Assert.Equal(release2WithApproverRoleForUser.Id, viewModels[1].Id);
                }
            }

            [Fact]
            public async Task UserIsPublicationAndReleaseApprover_NoDuplication()
            {
                var contextId = Guid.NewGuid().ToString();

                var publication = _fixture
                    .DefaultPublication()
                    .WithReleaseVersions(_ => _fixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate();

                var approverReleaseRolesForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRole(ReleaseRole.Approver)
                    .WithReleaseVersions(publication.ReleaseVersions)
                    .GenerateList();

                var approverPublicationRoleForUser = _fixture
                    .DefaultUserPublicationRole()
                    .WithUser(User)
                    .WithRole(PublicationRole.Approver)
                    .WithPublication(publication)
                    .Generate();

                await using (var context = InMemoryApplicationDbContext(contextId))
                {
                    await context.Publications.AddRangeAsync(publication);
                    await context.UserReleaseRoles.AddRangeAsync(approverReleaseRolesForUser);
                    await context.UserPublicationRoles.AddRangeAsync(approverPublicationRoleForUser);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryApplicationDbContext(contextId))
                {
                    var service = BuildReleaseService(context);

                    var result = await service.ListUsersReleasesForApproval();

                    var viewModels = result.AssertRight();

                    // Assert that the Release only appears once despite the user having approval directly via the
                    // Release itself AND via the overarching Publication.
                    Assert.Single(viewModels);
                    Assert.Equal(publication.ReleaseVersions[0].Id, viewModels[0].Id);
                }
            }
        }

        private static ReleaseService BuildReleaseService(
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
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IDataSetVersionService? dataSetVersionService = null,
            IProcessorClient? processorClient = null,
            IBlobCacheService? cacheService = null)
        {
            var userService = AlwaysTrueUserService();

            userService
                .Setup(s => s.GetUserId())
                .Returns(User.Id);

            return new ReleaseService(
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
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
                dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
                processorClient ?? Mock.Of<IProcessorClient>(Strict),
                new SequentialGuidGenerator(),
                cacheService ?? Mock.Of<IBlobCacheService>(Strict)
            );
        }
    }
}
