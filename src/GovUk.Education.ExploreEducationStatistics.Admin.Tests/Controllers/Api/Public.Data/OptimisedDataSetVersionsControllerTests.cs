#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Xunit.Abstractions;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public class OptimisedDataSetVersionsControllerTestsFixture : OptimisedHttpClientWithPsqlFixture;

[CollectionDefinition(nameof(OptimisedDataSetVersionsControllerTestsFixture))]
public class OptimisedDataSetVersionsControllerTestsCollection
    : ICollectionFixture<OptimisedDataSetVersionsControllerTestsFixture>;

[Collection(nameof(OptimisedDataSetVersionsControllerTestsFixture))]
public abstract class OptimisedDataSetVersionsControllerTests(
    OptimisedDataSetVersionsControllerTestsFixture fixture,
    ITestOutputHelper output
) : IAsyncLifetime
{
    private const string BaseUrl = "api/public-data/data-set-versions";
    private readonly DataFixture _dataFixture = new(new Random().Next());

    public async Task InitializeAsync()
    {
        // var sw = new Stopwatch();
        // sw.Start();
        //
        // await fixture.ClearTestData();
        //
        // output.WriteLine($"Clear up test data {sw.ElapsedMilliseconds}");
        // sw.Restart();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public class OptimisedGetVersionsTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.AvailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task OnlyPreviouslyPublishedVersionsReturned(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await _fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 10,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<
                PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>
            >();

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(10, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(1, viewModel.Paging.TotalResults);

            var liveVersion = Assert.Single(viewModel.Results);
            Assert.Equal(currentDataSetVersion.Id, liveVersion.Id);
            Assert.Equal(currentDataSetVersion.PublicVersion, liveVersion.Version);
            Assert.Equal(currentDataSetVersion.Status, liveVersion.Status);
            Assert.Equal(currentDataSetVersion.VersionType, liveVersion.Type);

            Assert.Equal(releaseFile.ReleaseVersion.Id, liveVersion.ReleaseVersion.Id);
            Assert.Equal(
                releaseFile.ReleaseVersion.Release.Title,
                liveVersion.ReleaseVersion.Title
            );

            Assert.Equal(releaseFile.File.DataSetFileId, liveVersion.File.Id);
            Assert.Equal(releaseFile.Name, liveVersion.File.Title);

            liveVersion.Published.AssertEqual(currentDataSetVersion.Published!.Value);
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DraftVersionsNotReturned(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 10,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<
                PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>
            >();

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(10, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(0, viewModel.Paging.TotalResults);

            Assert.Empty(viewModel.Results);
        }

        [Theory]
        [InlineData(1, 1, 0)]
        [InlineData(1, 2, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(1, 2, 9)]
        [InlineData(2, 2, 9)]
        [InlineData(2, 2, 2)]
        public async Task ResultsArePaginatedCorrectly(
            int page,
            int pageSize,
            int numberOfPublishedDataSetVersions
        )
        {
            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .ForInstance(s =>
                    s.Set(
                        rf => rf.ReleaseVersion,
                        () =>
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture
                                        .DefaultRelease()
                                        .WithPublication(_dataFixture.DefaultPublication())
                                )
                    )
                )
                .ForInstance(s =>
                    s.Set(rf => rf.File, () => _dataFixture.DefaultFile(FileType.Data))
                )
                .GenerateList(numberOfPublishedDataSetVersions);

            await _fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            var now = DateTimeOffset.UtcNow.AddDays(-numberOfPublishedDataSetVersions);
            var dataSetVersions = releaseFiles
                .Select(
                    (rf, index) =>
                        _dataFixture
                            .DefaultDataSetVersion(
                                filters: 1,
                                indicators: 1,
                                locations: 1,
                                timePeriods: 3
                            )
                            .WithVersionNumber(major: 1, minor: index, patch: 1)
                            .WithStatusPublished()
                            .WithPublished(now.AddDays(index))
                            .WithDataSet(dataSet)
                            .WithRelease(
                                _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(rf.Id)
                            )
                            .Generate()
                )
                .ToList();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersions);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: page,
                pageSize: pageSize,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<
                PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>
            >();

            var pagedDataSetVersionIds = dataSetVersions
                .OrderByDescending(dsv => dsv.Published)
                .Select(dsv => dsv.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.NotNull(viewModel);
            Assert.Equal(page, viewModel.Paging.Page);
            Assert.Equal(pageSize, viewModel.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSetVersions, viewModel.Paging.TotalResults);
            Assert.Equal(pagedDataSetVersionIds.Count, viewModel.Results.Count);
            Assert.All(viewModel.Results, dsv => Assert.Contains(dsv.Id, pagedDataSetVersionIds));
        }

        [Fact]
        public async Task VersionsForDifferentDataSetNotReturned()
        {
            ReleaseFile targetReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            ReleaseFile otherReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await _fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(targetReleaseFile, otherReleaseFile);
                });

            DataSet targetDataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            DataSet otherDataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(targetDataSet, otherDataSet));

            DataSetVersion targetDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(targetDataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(targetReleaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion otherDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(otherDataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(otherReleaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(targetDataSetVersion, otherDataSetVersion);
                    context.DataSets.UpdateRange(targetDataSet, otherDataSet);
                });

            var response = await ListLiveVersions(
                dataSetId: targetDataSet.Id,
                page: 1,
                pageSize: 10,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<
                PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>
            >();

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(10, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(1, viewModel.Paging.TotalResults);

            var liveVersion = Assert.Single(viewModel.Results);
            Assert.Equal(targetDataSetVersion.Id, liveVersion.Id);
            Assert.Equal(targetDataSetVersion.PublicVersion, liveVersion.Version);
            Assert.Equal(targetDataSetVersion.Status, liveVersion.Status);
            Assert.Equal(targetDataSetVersion.VersionType, liveVersion.Type);

            Assert.Equal(targetReleaseFile.ReleaseVersion.Id, liveVersion.ReleaseVersion.Id);
            Assert.Equal(
                targetReleaseFile.ReleaseVersion.Release.Title,
                liveVersion.ReleaseVersion.Title
            );

            Assert.Equal(targetReleaseFile.File.DataSetFileId, liveVersion.File.Id);
            Assert.Equal(targetReleaseFile.Name, liveVersion.File.Title);

            liveVersion.Published.AssertEqual(targetDataSetVersion.Published!.Value);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var response = await ListLiveVersions(
                dataSetId: Guid.NewGuid(),
                page: 1,
                pageSize: 1,
                client: _fixture.CreateClient().WithUser(TestUser.Authenticated)
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoDataSetId_Returns400()
        {
            var response = await _fixture.CreateClient().WithUser(TestUser.Bau).GetAsync(BaseUrl);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasNotEmptyError("dataSetId");
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await ListLiveVersions(
                dataSetId: Guid.NewGuid(),
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> ListLiveVersions(
            Guid dataSetId,
            HttpClient client,
            int? page = null,
            int? pageSize = null
        )
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "dataSetId", dataSetId.ToString() },
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetVersionTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        public static TheoryData<DataSetVersionStatus> AllDataSetVersionStatuses =>
            new(EnumUtil.GetEnums<DataSetVersionStatus>());

        [Theory]
        [MemberData(nameof(AllDataSetVersionStatuses))]
        public async Task Success(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            var dataSetVersions = _dataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .GenerateList(3);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersions);
                    context.DataSets.Update(dataSet);
                });

            var requestedDataSetVersion = dataSetVersions[1];

            var response = await GetDataSetVersion(
                dataSetVersionId: requestedDataSetVersion.Id,
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<DataSetVersionInfoViewModel>();

            Assert.NotNull(viewModel);
            Assert.Equal(requestedDataSetVersion.Id, viewModel.Id);
            Assert.Equal(requestedDataSetVersion.PublicVersion, viewModel.Version);
            Assert.Equal(requestedDataSetVersion.Status, viewModel.Status);
            Assert.Equal(requestedDataSetVersion.VersionType, viewModel.Type);
            Assert.Equal(requestedDataSetVersion.Notes, viewModel.Notes);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = _fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await GetDataSetVersion(
                dataSetVersionId: Guid.NewGuid(),
                client: client
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionDoesNotExist_Returns404()
        {
            var response = await GetDataSetVersion(
                dataSetVersionId: Guid.NewGuid(),
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetVersion(
            Guid dataSetVersionId,
            HttpClient client
        )
        {
            var uri = $"{BaseUrl}/{dataSetVersionId}";

            return await client.GetAsync(uri);
        }
    }

    public class CreateNextVersionTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        [Fact]
        public async Task Success()
        {
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await _fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(currentDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            DataSetVersion? nextVersion = null;

            _fixture
                .GetProcessorClientMock()
                .Setup(c =>
                    c.CreateNextDataSetVersionMappings(
                        dataSet.Id,
                        releaseFile.Id,
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(async () =>
                {
                    var savedDataSet = await _fixture
                        .GetPublicDataDbContext()
                        .DataSets.SingleAsync(ds => ds.Id == dataSet.Id);

                    nextVersion = _dataFixture
                        .DefaultDataSetVersion()
                        .WithStatusMapping()
                        .WithVersionNumber(major: 1, minor: 1)
                        .WithDataSet(savedDataSet)
                        .WithRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFile.Id)
                        )
                        .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

                    await _fixture
                        .GetPublicDataDbContext()
                        .AddTestData(context =>
                        {
                            context.DataSetVersions.Add(nextVersion);
                            context.DataSets.Update(savedDataSet);
                        });

                    return new ProcessDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet.Id,
                        DataSetVersionId = nextVersion.Id,
                        InstanceId = Guid.NewGuid(),
                    };
                });

            var response = await CreateNextVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<DataSetVersionSummaryViewModel>();

            Assert.NotNull(nextVersion);
            Assert.Equal(viewModel.Id, nextVersion.Id);
            Assert.Equal(viewModel.Version, nextVersion.PublicVersion);
            Assert.Equal(viewModel.Status, nextVersion.Status);
            Assert.Equal(viewModel.Type, nextVersion.VersionType);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = _fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await CreateNextVersion(
                dataSetId: Guid.NewGuid(),
                releaseFileId: Guid.NewGuid(),
                client: client
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var response = await CreateNextVersion(
                dataSetId: Guid.Empty,
                releaseFileId: Guid.Empty,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var validationProblem = response.AssertValidationProblem();
            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasNotEmptyError("dataSetId");
            validationProblem.AssertHasNotEmptyError("releaseFileId");
        }

        private async Task<HttpResponseMessage> CreateNextVersion(
            Guid dataSetId,
            Guid releaseFileId,
            HttpClient client
        )
        {
            var uri = new Uri(BaseUrl, UriKind.Relative);

            return await client.PostAsync(
                uri,
                new JsonNetContent(
                    new NextDataSetVersionCreateRequest
                    {
                        DataSetId = dataSetId,
                        ReleaseFileId = releaseFileId,
                    }
                )
            );
        }
    }

    public class CompleteNextVersionTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        [Fact]
        public async Task Success()
        {
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await _fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            _fixture
                .GetProcessorClientMock()
                .Setup(c =>
                    c.CompleteNextDataSetVersionImport(
                        nextDataSetVersion.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(() =>
                    new ProcessDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet.Id,
                        DataSetVersionId = nextDataSetVersion.Id,
                        InstanceId = Guid.NewGuid(),
                    }
                );

            var response = await CompleteNextVersionImport(
                dataSetVersionId: nextDataSetVersion.Id,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<DataSetVersionSummaryViewModel>();

            Assert.Equal(viewModel.Id, nextDataSetVersion.Id);
            Assert.Equal(viewModel.Version, nextDataSetVersion.PublicVersion);
            Assert.Equal(viewModel.Status, nextDataSetVersion.Status);
            Assert.Equal(viewModel.Type, nextDataSetVersion.VersionType);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = _fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await CompleteNextVersionImport(
                dataSetVersionId: Guid.NewGuid(),
                client: client
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var response = await CompleteNextVersionImport(
                dataSetVersionId: Guid.Empty,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var validationProblem = response.AssertValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasNotEmptyError("dataSetVersionId");
        }

        private async Task<HttpResponseMessage> CompleteNextVersionImport(
            Guid dataSetVersionId,
            HttpClient client
        )
        {
            var uri = new Uri($"{BaseUrl}/complete", UriKind.Relative);

            return await client.PostAsync(
                uri,
                new JsonNetContent(
                    new NextDataSetVersionCompleteImportRequest
                    {
                        DataSetVersionId = dataSetVersionId,
                    }
                )
            );
        }
    }

    public class DeleteVersionTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        [Fact]
        public async Task Success()
        {
            var dataSetVersionId = Guid.NewGuid();

            _fixture
                .GetProcessorClientMock()
                .Setup(c => c.DeleteDataSetVersion(dataSetVersionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

            var response = await DeleteVersion(
                dataSetVersionId,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertNoContent();
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = _fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await DeleteVersion(Guid.NewGuid(), client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ProcessorReturns404_Returns404()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => _dataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            _fixture
                .GetProcessorClientMock()
                .Setup(c =>
                    c.DeleteDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new Either<ActionResult, Unit>(new NotFoundResult()));

            var response = await DeleteVersion(
                dataSetVersion.Id,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task ProcessorReturns400_Returns400()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => _dataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            _fixture
                .GetProcessorClientMock()
                .Setup(c =>
                    c.DeleteDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new Either<ActionResult, Unit>(
                        new BadRequestObjectResult(
                            new ValidationProblemViewModel
                            {
                                Errors = new ErrorViewModel[]
                                {
                                    new() { Code = "error code", Path = "error path" },
                                },
                            }
                        )
                    )
                );

            var response = await DeleteVersion(
                dataSetVersion.Id,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError("error path", "error code");
        }

        [Fact]
        public async Task ProcessorClientThrows_Returns500()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => _dataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            _fixture
                .GetProcessorClientMock()
                .Setup(c =>
                    c.DeleteDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>())
                )
                .ThrowsAsync(new HttpRequestException());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DeleteVersion(
                    dataSetVersion.Id,
                    client: _fixture.CreateClient().WithUser(TestUser.Bau)
                )
            );

            Assert.IsType<HttpRequestException>(exception.InnerException);
        }

        private async Task<HttpResponseMessage> DeleteVersion(
            Guid dataSetVersionId,
            HttpClient client
        )
        {
            var uri = new Uri($"{BaseUrl}/{dataSetVersionId}", UriKind.Relative);

            return await client.DeleteAsync(uri);
        }
    }

    public class GetVersionChangesTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        [Fact]
        public async Task Success_Returns200()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var mockedChanges = new MockedChanges { Changes = ["test"] };

            _fixture
                .GetPublicDataApiClientMock()
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(mockedChanges),
                    }
                );

            var response = await GetVersionChanges(
                dataSetVersion.Id,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertOk(mockedChanges, useSystemJson: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = _fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await GetVersionChanges(Guid.NewGuid(), client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            var response = await GetVersionChanges(
                Guid.NewGuid(),
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task PublicDataApiReturns400_Returns400()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            _fixture
                .GetPublicDataApiClientMock()
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ValidationUtils.ValidationResult());

            var response = await GetVersionChanges(
                dataSetVersion.Id,
                client: _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            MockUtils.VerifyAllMocks(_fixture.GetPublicDataApiClientMock());

            response.AssertValidationProblem();
        }

        [Fact]
        public async Task PublicDataApiClientThrows_Returns500()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            _fixture
                .GetPublicDataApiClientMock()
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new HttpRequestException());

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                GetVersionChanges(dataSetVersion.Id, _fixture.CreateClient().WithUser(TestUser.Bau))
            );

            Assert.IsType<HttpRequestException>(exception.InnerException);

            MockUtils.VerifyAllMocks(_fixture.GetPublicDataApiClientMock());
        }

        private async Task<HttpResponseMessage> GetVersionChanges(
            Guid dataSetVersionId,
            HttpClient client
        )
        {
            var uri = new Uri($"{BaseUrl}/{dataSetVersionId}/changes", UriKind.Relative);

            return await client.GetAsync(uri);
        }

        private record MockedChanges
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local - the Test response is accessed in a Deep Assert, which the compiler can not determine.
            public List<string> Changes { get; init; } = [];
        }
    }

    public class UpdateVersionTests(
        OptimisedDataSetVersionsControllerTestsFixture fixture,
        ITestOutputHelper output
    ) : OptimisedDataSetVersionsControllerTests(fixture, output)
    {
        private readonly OptimisedDataSetVersionsControllerTestsFixture _fixture = fixture;

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.UpdateableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task Success(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await _fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .WithNotes("initial notes.")
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var updateRequest = new DataSetVersionUpdateRequest { Notes = "updated notes." };

            var response = await UpdateVersion(
                nextDataSetVersion.Id,
                updateRequest,
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var viewModel = response.AssertOk<DataSetDraftVersionViewModel>();

            Assert.NotNull(viewModel);
            Assert.Equal(nextDataSetVersion.Id, viewModel.Id);
            Assert.Equal(nextDataSetVersion.PublicVersion, viewModel.Version);
            Assert.Equal(nextDataSetVersion.Status, viewModel.Status);
            Assert.Equal(nextDataSetVersion.VersionType, viewModel.Type);
            Assert.Equal(releaseFile.File.DataSetFileId!.Value, viewModel.File.Id);
            Assert.Equal(releaseFile.Name, viewModel.File.Title);
            Assert.Equal(releaseFile.ReleaseVersion.Id, viewModel.ReleaseVersion.Id);
            Assert.Equal(releaseFile.ReleaseVersion.Release.Title, viewModel.ReleaseVersion.Title);
            Assert.Equal(nextDataSetVersion.TotalResults, viewModel.TotalResults);
            Assert.Equal("updated notes.", viewModel.Notes);
            Assert.Equal(
                nextDataSetVersion.MetaSummary!.GeographicLevels.Select(l => l.GetEnumLabel()),
                viewModel.GeographicLevels
            );
            Assert.Equal(
                TimePeriodRangeViewModel.Create(nextDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.TimePeriods
            );
            Assert.Equal(nextDataSetVersion.MetaSummary!.Filters, viewModel.Filters);
            Assert.Equal(nextDataSetVersion.MetaSummary!.Indicators, viewModel.Indicators);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = _fixture.CreateClient().WithUser(TestUser.Authenticated);

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(Guid.NewGuid(), updateRequest, client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionDoesNotExist_Returns404()
        {
            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(
                Guid.NewGuid(),
                updateRequest,
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            response.AssertNotFound();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.ReadOnlyStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionCannotBeUpdated_Returns400(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusPublished();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(
                nextDataSetVersion.Id,
                updateRequest,
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetVersionId",
                expectedCode: ValidationMessages.DataSetVersionCannotBeUpdated.Code
            );
        }

        [Fact]
        public async Task DataSetVersionIsFirstVersion_UpdatingNotes_Returns400()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet().WithStatusDraft();

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await _fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var updateRequest = new DataSetVersionUpdateRequest { Notes = "updated notes." };

            var response = await UpdateVersion(
                dataSetVersion.Id,
                updateRequest,
                _fixture.CreateClient().WithUser(TestUser.Bau)
            );

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "notes",
                expectedCode: ValidationMessages.DataSetVersionCannotHaveNotes.Code
            );
        }

        private async Task<HttpResponseMessage> UpdateVersion(
            Guid dataSetVersionId,
            DataSetVersionUpdateRequest updateRequest,
            HttpClient client
        )
        {
            return await client.PatchAsJsonAsync($"{BaseUrl}/{dataSetVersionId}", updateRequest);
        }
    }
}
