#nullable enable
using System.Net.Http.Json;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// TODO EES-6450 - decide if we're happy with ignoring this #pragma in test projects.
// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
public class PreviewTokenControllerTestsFixture : OptimisedHttpClientWithPsqlCollectionFixture;

[CollectionDefinition(nameof(PreviewTokenControllerTestsFixture))]
public class PreviewTokenControllerTestsCollection : ICollectionFixture<PreviewTokenControllerTestsFixture>;

[Collection(nameof(PreviewTokenControllerTestsFixture))]
public abstract class PreviewTokenControllerTests(PreviewTokenControllerTestsFixture fixture) : IAsyncLifetime
{
    private const string BaseUrl = "api/public-data/preview-tokens";
    private static readonly DataFixture DataFixture = new(new Random().Next());

    private static bool _commonDataInitialized;

    private static readonly ClaimsPrincipal BauUser = DataFixture.BauUser();

    private static readonly User BauUserEntry = DataFixture
        .DefaultUser()
        .WithId(BauUser.GetUserId())
        .WithEmail(BauUser.GetEmail());

    /// <summary>
    /// Set up common test data that is commonly used by all tests within this suite.
    /// TODO EES-6450 - see if we're happy with this way of setting up global test suite test data.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (!_commonDataInitialized)
        {
            fixture.RegisterTestUser(BauUser);
            await fixture.GetContentDbContext().AddTestData(context => context.Users.Add(BauUserEntry));
            _commonDataInitialized = true;
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public class CreatePreviewTokenTests(PreviewTokenControllerTestsFixture fixture)
        : PreviewTokenControllerTests(fixture)
    {
        private record CreatePreviewTokenValidationError(string Message, string Path);

        [Fact]
        public async Task Success()
        {
            var sevenDaysFromNow = DateTimeOffset.UtcNow.AddDays(7);
            var dataSetVersion = await SetUpDataSetVersionTestData();

            var label = new string('A', count: 100);
            var response = await CreatePreviewToken(dataSetVersionId: dataSetVersion.Id, label: label);

            var (viewModel, createdEntityId) = response.AssertCreated<PreviewTokenViewModel>(
                expectedLocationPrefix: "http://localhost/api/public-data/preview-tokens/"
            );
            Assert.True(Guid.TryParse(createdEntityId, out var previewTokenId));

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, viewModel.Id),
                () => Assert.Equal(label, viewModel.Label),
                () => Assert.Equal(PreviewTokenStatus.Active, viewModel.Status),
                () => Assert.Equal(BauUserEntry.Email, viewModel.CreatedByEmail),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Activates.AssertUtcNow(),
                () => viewModel.Expires.AssertEqual(sevenDaysFromNow),
                () => Assert.Null(viewModel.Updated)
            );

            var actualPreviewToken = Assert.Single(await fixture.GetPublicDataDbContext().PreviewTokens.ToListAsync());

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, actualPreviewToken.Id),
                () => Assert.Equal(label, actualPreviewToken.Label),
                () => Assert.Equal(dataSetVersion.Id, actualPreviewToken.DataSetVersionId),
                () => Assert.Equal(BauUserEntry.Id, actualPreviewToken.CreatedByUserId),
                () => actualPreviewToken.Created.AssertUtcNow(),
                () => actualPreviewToken.Activates.AssertUtcNow(),
                () => actualPreviewToken.Expires.AssertEqual(sevenDaysFromNow),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task CustomActivatesOrExpiresProvided_Success(bool activatesProvided, bool expiresProvided)
        {
            var dataSetVersion = await SetUpDataSetVersionTestData();

            DateTimeOffset? suppliedActivates = activatesProvided
                ? DateTimeOffset.UtcNow.AddDays(2).GetUkStartOfDayOn() // 2 days from now start of day UK time
                : null;

            DateTimeOffset? suppliedExpires =
                !expiresProvided ? null
                : activatesProvided ? suppliedActivates?.AddDays(5).GetUkEndOfDayOn() // 5 days after activates end of day UK time
                : DateTimeOffset.UtcNow.AddDays(5).GetUkEndOfDayOn(); // 5 days from now end of day UK time

            var label = new string('A', count: 100);

            var response = await CreatePreviewToken(
                dataSetVersionId: dataSetVersion.Id,
                label: label,
                activates: suppliedActivates,
                expires: suppliedExpires
            );

            var (viewModel, createdEntityId) = response.AssertCreated<PreviewTokenViewModel>(
                expectedLocationPrefix: "http://localhost/api/public-data/preview-tokens/"
            );
            Assert.True(Guid.TryParse(createdEntityId, out var previewTokenId));

            var expectedActivates = suppliedActivates ?? viewModel.Created;
            var expectedExpiry =
                suppliedExpires
                ?? suppliedActivates?.AddDays(7).GetUkStartOfDayOn() // 7 days after activates end of day UK time
                ?? DateTimeOffset.UtcNow.AddDays(7).GetUkStartOfDayOn(); // 7 days from now end of day UK time

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, viewModel.Id),
                () => Assert.Equal(label, viewModel.Label),
                () =>
                    Assert.Equal(
                        activatesProvided ? PreviewTokenStatus.Pending : PreviewTokenStatus.Active,
                        viewModel.Status
                    ),
                () => Assert.Equal(BauUserEntry.Email, viewModel.CreatedByEmail),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Activates.AssertEqual(expectedActivates),
                () => viewModel.Expires.AssertEqual(expectedExpiry),
                () => Assert.Null(viewModel.Updated)
            );

            var previewTokens = await fixture
                .GetPublicDataDbContext()
                .PreviewTokens.Where(pt => pt.DataSetVersionId == dataSetVersion.Id)
                .ToListAsync();

            var actualPreviewToken = Assert.Single(previewTokens);

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, actualPreviewToken.Id),
                () => Assert.Equal(label, actualPreviewToken.Label),
                () => Assert.Equal(dataSetVersion.Id, actualPreviewToken.DataSetVersionId),
                () => Assert.Equal(BauUserEntry.Id, actualPreviewToken.CreatedByUserId),
                () => actualPreviewToken.Created.AssertUtcNow(),
                () => actualPreviewToken.Activates.AssertEqual(expectedActivates),
                () => actualPreviewToken.Expires.AssertEqual(expectedExpiry),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }

        [Fact]
        public async Task CustomDateOutOfRange_ReturnsValidationProblem()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();
            var expiresDate = DateTimeOffset.UtcNow.AddDays(14);
            var expectedError = new CreatePreviewTokenValidationError(
                Path: nameof(PreviewTokenCreateRequest.Expires).ToLowerFirst(),
                Message: "Expires date must be no more than 7 days from today."
            );

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await CreatePreviewToken(
                dataSetVersion.Id,
                new string('A', count: 100),
                activates: null,
                expires: expiresDate
            );

            var validationProblem = response.AssertValidationProblem();
            var validationError = validationProblem.AssertHasPredicateError(expectedError.Path);
            Assert.Equal(expectedError.Message, validationError.Message);
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.StatusesExceptDraft),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionIsNotDraft_ReturnsValidationProblem(DataSetVersionStatus status)
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithStatus(status)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await CreatePreviewToken(dataSetVersion.Id, "Label");

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedCode: ValidationMessages.DataSetVersionStatusNotDraft.Code,
                expectedMessage: ValidationMessages.DataSetVersionStatusNotDraft.Message,
                expectedPath: nameof(PreviewTokenCreateRequest.DataSetVersionId).ToLowerFirst()
            );
        }

        [Fact]
        public async Task DataSetVersionIdIsEmpty_ReturnsValidationProblem()
        {
            var response = await CreatePreviewToken(dataSetVersionId: Guid.Empty, "Label");

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasNotEmptyError(nameof(PreviewTokenCreateRequest.DataSetVersionId).ToLowerFirst());
        }

        [Fact]
        public async Task LabelIsEmpty_ReturnsValidationProblem()
        {
            var response = await CreatePreviewToken(Guid.NewGuid(), label: string.Empty);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasNotEmptyError(nameof(PreviewTokenCreateRequest.Label).ToLowerFirst());
        }

        [Fact]
        public async Task LabelAboveMaximumLength_ReturnsValidationProblem()
        {
            var response = await CreatePreviewToken(Guid.NewGuid(), label: new string('A', count: 101));

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasMaximumLengthError(
                nameof(PreviewTokenCreateRequest.Label).ToLowerFirst(),
                maxLength: 100
            );
        }

        [Fact]
        public async Task NoDataSetVersion_ReturnsNotFound()
        {
            var response = await CreatePreviewToken(dataSetVersionId: Guid.NewGuid(), "Label");
            response.AssertNotFound();
        }

        [Fact]
        public async Task NotBauUser_ReturnsForbidden()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await CreatePreviewToken(dataSetVersion.Id, "Label", user: OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> CreatePreviewToken(
            Guid dataSetVersionId,
            string label,
            DateTimeOffset? activates = null,
            DateTimeOffset? expires = null,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient().WithUser(user ?? BauUser);

            var request = new PreviewTokenCreateRequest
            {
                DataSetVersionId = dataSetVersionId,
                Activates = activates,
                Expires = expires,
                Label = label,
            };

            return await client.PostAsJsonAsync(BaseUrl, request);
        }

        private async Task<DataSetVersion> SetUpDataSetVersionTestData()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            return dataSetVersion;
        }
    }

    public class GetPreviewTokenTests(PreviewTokenControllerTestsFixture fixture) : PreviewTokenControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    DataFixture.DefaultPreviewToken().WithCreatedByUserId(BauUserEntry.Id).Generate(2)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await GetPreviewToken(previewToken.Id);

            var expectedResult = new PreviewTokenViewModel
            {
                Id = previewToken.Id,
                Label = previewToken.Label,
                Status = PreviewTokenStatus.Active,
                Activates = previewToken.Activates,
                Created = previewToken.Created,
                CreatedByEmail = BauUserEntry.Email,
                Expires = previewToken.Expires,
                Updated = previewToken.Updated,
            };

            response.AssertOk(expectedResult);
        }

        [Fact]
        public async Task PreviewTokenIsExpired_StatusIsExpired()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    [DataFixture.DefaultPreviewToken(expired: true).WithCreatedByUserId(BauUserEntry.Id)]
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await GetPreviewToken(previewToken.Id);

            var viewModel = response.AssertOk<PreviewTokenViewModel>();
            Assert.Equal(PreviewTokenStatus.Expired, viewModel.Status);
        }

        [Fact]
        public async Task NoPreviewToken_ReturnsNotFound()
        {
            var response = await GetPreviewToken(previewTokenId: Guid.NewGuid());
            response.AssertNotFound();
        }

        [Fact]
        public async Task NotBauUser_ReturnsForbidden()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await GetPreviewToken(
                dataSetVersion.PreviewTokens[0].Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> GetPreviewToken(Guid previewTokenId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient().WithUser(user ?? BauUser);

            var uri = new Uri($"{BaseUrl}/{previewTokenId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ListPreviewTokensTests(PreviewTokenControllerTestsFixture fixture)
        : PreviewTokenControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    DataFixture
                        .DefaultPreviewToken()
                        .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                        .WithCreatedByUserId(BauUserEntry.Id)
                        .Generate(2)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPreviewTokens(dataSetVersion.Id);

            var expectedViewModels = dataSetVersion
                .PreviewTokens.Select(pt => new PreviewTokenViewModel
                {
                    Id = pt.Id,
                    Label = pt.Label,
                    Status = pt.Status,
                    Activates = pt.Activates,
                    Created = pt.Created,
                    CreatedByEmail = BauUserEntry.Email,
                    Expires = pt.Expires,
                    Updated = pt.Updated,
                })
                .ToList();

            response.AssertOk(expectedViewModels);
        }

        [Fact]
        public async Task MultipleDataSetVersions_FiltersByDataSetVersion()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var (dataSetVersion1, dataSetVersion2) = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken().WithCreatedByUserId(BauUserEntry.Id)])
                .GenerateTuple2();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPreviewTokens(dataSetVersion1.Id);

            var viewModels = response.AssertOk<List<PreviewTokenViewModel>>();

            Assert.Single(viewModels);
            Assert.Equal(dataSetVersion1.PreviewTokens[0].Id, viewModels[0].Id);
        }

        [Fact]
        public async Task DataSetVersionHasMultiplePreviewTokens_OrdersByDescendingExpiry()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    DataFixture
                        .DefaultPreviewToken()
                        .ForIndex(0, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddHours(1)))
                        .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddDays(-1)))
                        .ForIndex(2, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                        .ForIndex(3, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddDays(1)))
                        .WithCreatedByUserId(BauUserEntry.Id)
                        .Generate(4)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPreviewTokens(dataSetVersion.Id);

            var viewModels = response.AssertOk<List<PreviewTokenViewModel>>();

            // Preview tokens are expected in descending expiry date order
            Guid[] expectedPreviewTokenIds =
            [
                dataSetVersion.PreviewTokens[3].Id,
                dataSetVersion.PreviewTokens[0].Id,
                dataSetVersion.PreviewTokens[2].Id,
                dataSetVersion.PreviewTokens[1].Id,
            ];
            Assert.Equal(expectedPreviewTokenIds, viewModels.Select(vm => vm.Id));
        }

        [Fact]
        public async Task DataSetVersionHasNoPreviewTokens_ReturnsEmpty()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPreviewTokens(dataSetVersion.Id);

            var result = response.AssertOk<List<PreviewTokenViewModel>>();

            Assert.Empty(result);
        }

        [Fact]
        public async Task NoDataSetVersion_ReturnsNotFound()
        {
            var response = await ListPreviewTokens(dataSetVersionId: Guid.NewGuid());
            response.AssertNotFound();
        }

        [Fact]
        public async Task NotBauUser_ReturnsForbidden()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPreviewTokens(dataSetVersion.Id, user: OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> ListPreviewTokens(Guid dataSetVersionId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient().WithUser(user ?? BauUser);

            var queryParams = new Dictionary<string, string?> { { "dataSetVersionId", dataSetVersionId.ToString() } };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class RevokePreviewTokenTests(PreviewTokenControllerTestsFixture fixture)
        : PreviewTokenControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken().WithCreatedByUserId(BauUserEntry.Id)])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await RevokePreviewToken(previewToken.Id);

            var viewModel = response.AssertOk<PreviewTokenViewModel>();

            Assert.Multiple(
                () => Assert.Equal(previewToken.Id, viewModel.Id),
                () => Assert.Equal(previewToken.Label, viewModel.Label),
                () => Assert.Equal(PreviewTokenStatus.Expired, viewModel.Status),
                () => Assert.Equal(BauUserEntry.Email, viewModel.CreatedByEmail),
                () => Assert.Equal(previewToken.Created.TruncateNanoseconds(), viewModel.Created),
                () => viewModel.Expires.AssertUtcNow(),
                () => viewModel.Updated.AssertUtcNow()
            );

            var actualPreviewToken = await fixture
                .GetPublicDataDbContext()
                .PreviewTokens.SingleAsync(pt => pt.Id == dataSetVersion.PreviewTokens[0].Id);

            Assert.Multiple(
                () => Assert.Equal(PreviewTokenStatus.Expired, actualPreviewToken.Status),
                () => actualPreviewToken.Expires.AssertUtcNow(),
                () => actualPreviewToken.Updated.AssertUtcNow()
            );
        }

        [Fact]
        public async Task PreviewTokenIsExpired_ReturnsValidationProblem()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await RevokePreviewToken(dataSetVersion.PreviewTokens[0].Id);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "previewTokenId",
                expectedCode: ValidationMessages.PreviewTokenExpired.Code,
                expectedMessage: ValidationMessages.PreviewTokenExpired.Message
            );

            var actualPreviewToken = await fixture
                .GetPublicDataDbContext()
                .PreviewTokens.SingleAsync(pt => pt.Id == dataSetVersion.PreviewTokens[0].Id);

            Assert.Multiple(
                () => Assert.Equal(PreviewTokenStatus.Expired, actualPreviewToken.Status),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }

        [Fact]
        public async Task NoPreviewToken_ReturnsNotFound()
        {
            var response = await RevokePreviewToken(previewTokenId: Guid.NewGuid());
            response.AssertNotFound();
        }

        [Fact]
        public async Task NotBauUser_ReturnsForbidden()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await RevokePreviewToken(
                dataSetVersion.PreviewTokens[0].Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> RevokePreviewToken(Guid previewTokenId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient().WithUser(user ?? BauUser);

            var uri = new Uri($"{BaseUrl}/{previewTokenId}/revoke", UriKind.Relative);

            return await client.PostAsync(uri, content: null);
        }
    }
}
