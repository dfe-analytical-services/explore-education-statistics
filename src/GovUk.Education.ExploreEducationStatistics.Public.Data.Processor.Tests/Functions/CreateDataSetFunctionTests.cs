using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using FileType = GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class CreateDataSetFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public CreateDataSetFunction Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<CreateDataSetFunction>();
    }
}

[CollectionDefinition(nameof(CreateDataSetFunctionTestsFixture))]
public class CreateDataSetFunctionTestsCollection : ICollectionFixture<CreateDataSetFunctionTestsFixture>;

[Collection(nameof(CreateDataSetFunctionTestsFixture))]
public abstract class CreateDataSetFunctionTests(CreateDataSetFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class CreateDataSetTests(CreateDataSetFunctionTestsFixture fixture) : CreateDataSetFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var subjectId = Guid.NewGuid();

            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            var release = publication.Releases.Single();

            var (releaseFile, releaseMetaFile) = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(release.Versions.Single())
                .WithFiles([
                    DataFixture.DefaultFile(FileType.Data).WithSubjectId(subjectId),
                    DataFixture.DefaultFile(FileType.Metadata).WithSubjectId(subjectId),
                ])
                .GenerateTuple2();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile, releaseMetaFile);
                });

            var durableTaskClientMock = new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient");

            ProcessDataSetVersionContext? processInitialDataSetVersionContext = null;
            StartOrchestrationOptions? startOrchestrationOptions = null;
            durableTaskClientMock
                .Setup(client =>
                    client.ScheduleNewOrchestrationInstanceAsync(
                        nameof(ProcessInitialDataSetVersionOrchestration.ProcessInitialDataSetVersion),
                        It.IsAny<ProcessDataSetVersionContext>(),
                        It.IsAny<StartOrchestrationOptions>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    (TaskName _, object _, StartOrchestrationOptions? options, CancellationToken _) =>
                        options?.InstanceId ?? Guid.NewGuid().ToString()
                )
                .Callback<TaskName, object, StartOrchestrationOptions?, CancellationToken>(
                    (_, input, options, _) =>
                    {
                        processInitialDataSetVersionContext = Assert.IsAssignableFrom<ProcessDataSetVersionContext>(
                            input
                        );
                        startOrchestrationOptions = options;
                    }
                );

            var result = await CreateDataSet(releaseFileId: releaseFile.Id, durableTaskClientMock.Object);

            VerifyAllMocks(durableTaskClientMock);

            var responseViewModel = result.AssertOkObjectResult<ProcessDataSetVersionResponseViewModel>();

            // Assert a single data set was created
            var dataSet = Assert.Single(
                await fixture
                    .GetPublicDataDbContext()
                    .DataSets.Include(ds => ds.Versions)
                        .ThenInclude(dsv => dsv.Imports)
                    .Where(ds => ds.PublicationId == publication.Id)
                    .ToListAsync()
            );

            Assert.Equal(DataSetStatus.Draft, dataSet.Status);
            Assert.Equal(releaseFile.Name, dataSet.Title);
            Assert.Equal(releaseFile.Summary, dataSet.Summary);
            Assert.Equal(publication.Id, dataSet.PublicationId);
            Assert.Null(dataSet.LatestLiveVersion);

            // Assert the data set has a single version and that it is the latest draft version
            var dataSetVersion = Assert.Single(dataSet.Versions);
            Assert.Equal(dataSetVersion, dataSet.LatestDraftVersion);

            Assert.Equal(dataSet.Id, dataSetVersion.DataSetId);
            Assert.Equal(DataSetVersionStatus.Processing, dataSetVersion.Status);
            Assert.Empty(dataSetVersion.Notes);
            Assert.Equal(1, dataSetVersion.VersionMajor);
            Assert.Equal(0, dataSetVersion.VersionMinor);

            Assert.Equal(releaseFile.File.DataSetFileId, dataSetVersion.Release.DataSetFileId);
            Assert.Equal(releaseFile.Id, dataSetVersion.Release.ReleaseFileId);
            Assert.Equal(release.Slug, dataSetVersion.Release.Slug);
            Assert.Equal(release.Title, dataSetVersion.Release.Title);

            // Assert a single import was created
            var dataSetVersionImport = Assert.Single(dataSetVersion.Imports);

            Assert.Equal(dataSetVersion.Id, dataSetVersionImport.DataSetVersionId);
            Assert.NotEqual(Guid.Empty, dataSetVersionImport.InstanceId);
            Assert.Equal(DataSetVersionImportStage.Pending, dataSetVersionImport.Stage);

            // Assert the response view model values match the created data set version and import
            Assert.Equal(dataSet.Id, responseViewModel.DataSetId);
            Assert.Equal(dataSetVersion.Id, responseViewModel.DataSetVersionId);
            Assert.Equal(dataSetVersionImport.InstanceId, responseViewModel.InstanceId);

            // Assert the processing orchestrator was scheduled with the correct arguments
            Assert.NotNull(processInitialDataSetVersionContext);
            Assert.NotNull(startOrchestrationOptions);
            Assert.Equal(
                new ProcessDataSetVersionContext { DataSetVersionId = dataSetVersion.Id },
                processInitialDataSetVersionContext
            );
            Assert.Equal(
                new StartOrchestrationOptions { InstanceId = dataSetVersionImport.InstanceId.ToString() },
                startOrchestrationOptions
            );
        }

        [Fact]
        public async Task ReleaseFileIdIsEmpty_ReturnsValidationProblem()
        {
            var result = await CreateDataSet(releaseFileId: Guid.Empty);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst());
        }

        [Fact]
        public async Task ReleaseFileIdIsNotFound_ReturnsValidationProblem()
        {
            var releaseFileId = Guid.NewGuid();

            var result = await CreateDataSet(releaseFileId: releaseFileId);

            result.AssertNotFoundWithValidationProblem<ReleaseFile, Guid>(
                expectedId: releaseFileId,
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst()
            );
        }

        [Fact]
        public async Task ReleaseFileIdHasDataSetVersion_ReturnsValidationProblem()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases.Single().Versions.Single())
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            DataSet dataSet = DataFixture.DefaultDataSet();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithDataSet(dataSet);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });
            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.SaveChanges();

                    context.DataSetVersions.Add(dataSetVersion);
                });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileHasApiDataSetVersion.Code
            );
        }

        [Fact]
        public async Task ReleaseVersionNotDraft_ReturnsValidationProblem()
        {
            var subjectId = Guid.NewGuid();

            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 1)]);

            var (releaseFile, releaseMetaFile) = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases.Single().Versions.Single())
                .WithFiles([
                    DataFixture.DefaultFile(FileType.Data).WithSubjectId(subjectId),
                    DataFixture.DefaultFile(FileType.Metadata).WithSubjectId(subjectId),
                ])
                .GenerateTuple2();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile, releaseMetaFile);
                });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileReleaseVersionNotDraft.Code
            );
        }

        [Fact]
        public async Task ReleaseFileTypeNotData_ReturnsValidationProblem()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases.Single().Versions.Single())
                .WithFile(DataFixture.DefaultFile(FileType.Ancillary));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.FileTypeNotData.Code
            );
        }

        [Fact]
        public async Task ReleaseFileHasNoMetaFile_ReturnsValidationProblem()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases([DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases.Single().Versions.Single())
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            var result = await CreateDataSet(releaseFileId: releaseFile.Id);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
                expectedCode: ValidationMessages.NoMetadataFile.Code
            );
        }

        private async Task<IActionResult> CreateDataSet(Guid releaseFileId, DurableTaskClient? durableTaskClient = null)
        {
            return await fixture.Function.CreateDataSet(
                new DataSetCreateRequest { ReleaseFileId = releaseFileId },
                durableTaskClient ?? new Mock<DurableTaskClient>(MockBehavior.Strict, "TestClient").Object,
                CancellationToken.None
            );
        }
    }
}
