#nullable enable
using System.Net.Http.Json;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class PreviewTokenControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/preview-tokens";

    private static readonly ClaimsPrincipal BauUser = new DataFixture().BauUser();

    private static readonly User CreatedByBauUser = new DataFixture().DefaultUser().WithId(BauUser.GetUserId());

    public class CreatePreviewTokenTests(TestApplicationFactory testApp) : PreviewTokenControllerTests(testApp)
    {
        public record CreatePreviewTokenValidationError(string Message, string Path);

        private static readonly CreatePreviewTokenValidationError InvalidExpiryOutOfBound = new(
            Path: nameof(PreviewTokenCreateRequest.Expires).ToLowerFirst(),
            Message: "Expires date must be no more than 7 days after the activates date."
        );

        private static readonly CreatePreviewTokenValidationError InvalidExpiryBeforeActivates = new(
            Path: nameof(PreviewTokenCreateRequest.Expires).ToLowerFirst(),
            Message: "Expires date must be after the activates date."
        );

        private static readonly CreatePreviewTokenValidationError InvalidExpiryInPast = new(
            Path: nameof(PreviewTokenCreateRequest.Expires).ToLowerFirst(),
            Message: "Expires date must not be in the past."
        );

        private static readonly CreatePreviewTokenValidationError InvalidExpiryLessThan7DaysFromToday = new(
            Path: nameof(PreviewTokenCreateRequest.Expires).ToLowerFirst(),
            Message: "Expires date must be no more than 7 days from today."
        );

        private static readonly CreatePreviewTokenValidationError InvalidActivatesOutOfBound = new(
            Path: nameof(PreviewTokenCreateRequest.Activates).ToLowerFirst(),
            Message: "Activates date must be within the next 7 days."
        );

        private static readonly CreatePreviewTokenValidationError InvalidActivatesInPast = new(
            Path: nameof(PreviewTokenCreateRequest.Activates).ToLowerFirst(),
            Message: "Activates date must not be in the past."
        );

        private static DateTimeOffset StartOfDay(DateTimeOffset d) => new(d.Year, d.Month, d.Day, 0, 0, 0, d.Offset);

        private static DateTimeOffset EndOfDay(DateTimeOffset d) => new(d.Year, d.Month, d.Day, 23, 59, 59, d.Offset);

        private static DateTimeOffset DaysFromNow(int days) => DateTimeOffset.UtcNow.AddDays(days);

        private static DateTimeOffset NineDaysLater => DaysFromNow(9);
        private static DateTimeOffset NineDaysLaterExpires => EndOfDay(NineDaysLater);
        private static DateTimeOffset NineDaysLaterActivates => StartOfDay(NineDaysLater);
        private static DateTimeOffset TwoDaysAgo => DaysFromNow(-2);
        private static DateTimeOffset TwoDaysAgoActivates => StartOfDay(TwoDaysAgo);
        private static DateTimeOffset SixDaysLater => DaysFromNow(6);
        private static DateTimeOffset SixDaysLaterActivates => StartOfDay(SixDaysLater);

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
                () => Assert.Equal(CreatedByBauUser.Email, viewModel.CreatedByEmail),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Activates.AssertUtcNow(),
                () => viewModel.Expires.AssertEqual(sevenDaysFromNow),
                () => Assert.Null(viewModel.Updated)
            );

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = Assert.Single(await publicDataDbContext.PreviewTokens.ToListAsync());

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, actualPreviewToken.Id),
                () => Assert.Equal(label, actualPreviewToken.Label),
                () => Assert.Equal(dataSetVersion.Id, actualPreviewToken.DataSetVersionId),
                () => Assert.Equal(CreatedByBauUser.Id, actualPreviewToken.CreatedByUserId),
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
        [InlineData(false, false)]
        public async Task CustomActivatesOrExpiresProvided_Success(bool activatesProvided, bool expiresProvided)
        {
            var dataSetVersion = await SetUpDataSetVersionTestData();

            var twoDaysFromNow = DateTimeOffset.UtcNow.AddDays(2);
            var nineDaysFromNow = DateTimeOffset.UtcNow.AddDays(9);

            TimeZoneInfo ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

            DateTimeOffset? activates = activatesProvided ? twoDaysFromNow : null;
            var sevenDaysFromNow = DateTimeOffset.UtcNow.AddDays(7);
            DateTimeOffset? expires = expiresProvided ? activates?.AddDays(7) ?? sevenDaysFromNow : null;
            if (activates.HasValue)
            {
                activates = TimeZoneInfo.ConvertTime(activates.Value, ukTimeZone);
                activates = new DateTimeOffset(
                    activates.Value.Year,
                    activates.Value.Month,
                    activates.Value.Day,
                    0,
                    0,
                    0,
                    activates.Value.Offset
                );
            }
            if (expires.HasValue)
            {
                expires = TimeZoneInfo.ConvertTime(expires.Value, ukTimeZone);
                expires = new DateTimeOffset(
                    expires.Value.Year,
                    expires.Value.Month,
                    expires.Value.Day,
                    23,
                    59,
                    59,
                    expires.Value.Offset
                );
            }

            var label = new string('A', count: 100);
            var response = await CreatePreviewToken(
                dataSetVersionId: dataSetVersion.Id,
                label: label,
                activates: activates,
                expires: expires
            );

            var (viewModel, createdEntityId) = response.AssertCreated<PreviewTokenViewModel>(
                expectedLocationPrefix: "http://localhost/api/public-data/preview-tokens/"
            );
            Assert.True(Guid.TryParse(createdEntityId, out var previewTokenId));

            var expectedActivates = activatesProvided ? activates!.Value : viewModel.Created;
            var expectedExpiry =
                expires
                ?? (
                    activatesProvided
                        ? new DateTimeOffset(
                            nineDaysFromNow.Year,
                            nineDaysFromNow.Month,
                            nineDaysFromNow.Day,
                            0,
                            0,
                            0,
                            nineDaysFromNow.Offset
                        )
                        : sevenDaysFromNow
                );

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, viewModel.Id),
                () => Assert.Equal(label, viewModel.Label),
                () =>
                    Assert.Equal(
                        activatesProvided ? PreviewTokenStatus.Pending : PreviewTokenStatus.Active,
                        viewModel.Status
                    ),
                () => Assert.Equal(CreatedByBauUser.Email, viewModel.CreatedByEmail),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Activates.AssertEqual(expectedActivates),
                () => viewModel.Expires.AssertEqual(expectedExpiry),
                () => Assert.Null(viewModel.Updated)
            );

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = Assert.Single(await publicDataDbContext.PreviewTokens.ToListAsync());

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, actualPreviewToken.Id),
                () => Assert.Equal(label, actualPreviewToken.Label),
                () => Assert.Equal(dataSetVersion.Id, actualPreviewToken.DataSetVersionId),
                () => Assert.Equal(CreatedByBauUser.Id, actualPreviewToken.CreatedByUserId),
                () => actualPreviewToken.Created.AssertUtcNow(),
                () => actualPreviewToken.Activates.AssertEqual(expectedActivates),
                () => actualPreviewToken.Expires.AssertEqual(expectedExpiry),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }

        public static TheoryData<
            DateTimeOffset?,
            DateTimeOffset,
            CreatePreviewTokenValidationError
        > CustomDateOutOfRangeData =>
            new()
            {
                { TwoDaysAgoActivates, DateTimeOffset.UtcNow.AddDays(2), InvalidActivatesInPast }, // Start date is in the past and therefore is out of range.
                { NineDaysLaterActivates, NineDaysLaterExpires, InvalidActivatesOutOfBound },
                { SixDaysLaterActivates, DateTimeOffset.UtcNow.AddDays(14), InvalidExpiryOutOfBound }, // Duration is longer than 8 days and therefore is out of range.
                { SixDaysLaterActivates, DateTimeOffset.UtcNow.AddDays(3), InvalidExpiryBeforeActivates },
                { null, DateTimeOffset.UtcNow.AddDays(-1), InvalidExpiryInPast },
                { null, DateTimeOffset.UtcNow.AddDays(14), InvalidExpiryLessThan7DaysFromToday },
            };

        [Theory]
        [MemberData(nameof(CustomDateOutOfRangeData))]
        public async Task CustomDateOutOfRange_ReturnsValidationProblem(
            DateTimeOffset? activates,
            DateTimeOffset expires,
            CreatePreviewTokenValidationError expectedError
        )
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

            var response = await CreatePreviewToken(
                dataSetVersion.Id,
                new string('A', count: 100),
                activates: activates,
                expires: expires
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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithStatus(status)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

            var response = await CreatePreviewToken(dataSetVersion.Id, "Label", client);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> CreatePreviewToken(
            Guid dataSetVersionId,
            string label,
            HttpClient? client = null,
            DateTimeOffset? activates = null,
            DateTimeOffset? expires = null
        )
        {
            client ??= BuildApp().CreateClient();

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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));
            return dataSetVersion;
        }
    }

    public class GetPreviewTokenTests(TestApplicationFactory testApp) : PreviewTokenControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    DataFixture.DefaultPreviewToken().WithCreatedByUserId(CreatedByBauUser.Id).Generate(2)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await GetPreviewToken(previewToken.Id);

            var expectedResult = new PreviewTokenViewModel
            {
                Id = previewToken.Id,
                Label = previewToken.Label,
                Status = PreviewTokenStatus.Active,
                Activates = previewToken.Activates,
                Created = previewToken.Created,
                CreatedByEmail = CreatedByBauUser.Email,
                Expires = previewToken.Expires,
                Updated = previewToken.Updated,
            };

            response.AssertOk(expectedResult);
        }

        [Fact]
        public async Task PreviewTokenIsExpired_StatusIsExpired()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    [DataFixture.DefaultPreviewToken(expired: true).WithCreatedByUserId(CreatedByBauUser.Id)]
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

            var response = await GetPreviewToken(dataSetVersion.PreviewTokens[0].Id, client);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> GetPreviewToken(Guid previewTokenId, HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{previewTokenId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ListPreviewTokensTests(TestApplicationFactory testApp) : PreviewTokenControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    DataFixture
                        .DefaultPreviewToken()
                        .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                        .WithCreatedByUserId(CreatedByBauUser.Id)
                        .Generate(2)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

            var response = await ListPreviewTokens(dataSetVersion.Id);

            var expectedViewModels = dataSetVersion
                .PreviewTokens.Select(pt => new PreviewTokenViewModel
                {
                    Id = pt.Id,
                    Label = pt.Label,
                    Status = pt.Status,
                    Activates = pt.Activates,
                    Created = pt.Created,
                    CreatedByEmail = CreatedByBauUser.Email,
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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var (dataSetVersion1, dataSetVersion2) = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken().WithCreatedByUserId(CreatedByBauUser.Id)])
                .GenerateTuple2();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

            var response = await ListPreviewTokens(dataSetVersion1.Id);

            var viewModels = response.AssertOk<List<PreviewTokenViewModel>>();

            Assert.Single(viewModels);
            Assert.Equal(dataSetVersion1.PreviewTokens[0].Id, viewModels[0].Id);
        }

        [Fact]
        public async Task DataSetVersionHasMultiplePreviewTokens_OrdersByDescendingExpiry()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

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
                        .WithCreatedByUserId(CreatedByBauUser.Id)
                        .Generate(4)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

            var response = await ListPreviewTokens(dataSetVersion.Id, client);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> ListPreviewTokens(Guid dataSetVersionId, HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var queryParams = new Dictionary<string, string?> { { "dataSetVersionId", dataSetVersionId.ToString() } };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class RevokePreviewTokenTests(TestApplicationFactory testApp) : PreviewTokenControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken().WithCreatedByUserId(CreatedByBauUser.Id)])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await RevokePreviewToken(previewToken.Id);

            var viewModel = response.AssertOk<PreviewTokenViewModel>();

            Assert.Multiple(
                () => Assert.Equal(previewToken.Id, viewModel.Id),
                () => Assert.Equal(previewToken.Label, viewModel.Label),
                () => Assert.Equal(PreviewTokenStatus.Expired, viewModel.Status),
                () => Assert.Equal(CreatedByBauUser.Email, viewModel.CreatedByEmail),
                () => Assert.Equal(previewToken.Created.TruncateNanoseconds(), viewModel.Created),
                () => viewModel.Expires.AssertUtcNow(),
                () => viewModel.Updated.AssertUtcNow()
            );

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = await publicDataDbContext.PreviewTokens.SingleAsync(pt =>
                pt.Id == dataSetVersion.PreviewTokens[0].Id
            );

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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
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

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = await publicDataDbContext.PreviewTokens.SingleAsync(pt =>
                pt.Id == dataSetVersion.PreviewTokens[0].Id
            );

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

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp(DataFixture.AuthenticatedUser()).CreateClient();

            var response = await RevokePreviewToken(dataSetVersion.PreviewTokens[0].Id, client);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> RevokePreviewToken(Guid previewTokenId, HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{previewTokenId}/revoke", UriKind.Relative);

            return await client.PostAsync(uri, content: null);
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp(ClaimsPrincipal? user = null)
    {
        return TestApp.SetUser(user ?? BauUser);
    }
}
