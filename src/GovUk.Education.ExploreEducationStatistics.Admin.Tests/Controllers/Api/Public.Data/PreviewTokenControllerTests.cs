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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class PreviewTokenControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/preview-tokens";

    private static readonly ClaimsPrincipal BauUser = new DataFixture().BauUser();

    private static readonly User CreatedByBauUser = new()
    {
        Id = BauUser.GetUserId(),
        Email = "bau.user@test.com"
    };
    
    public class CreatePreviewTokenTests(TestApplicationFactory testApp) : PreviewTokenControllerTests(testApp)
    {
        public record CreatePreviewTokenValidationError(string Code, string Message, string Path);

        private static readonly CreatePreviewTokenValidationError ExpectInvalidExpiry = new(
            InvalidExpiryError.Code, 
            InvalidExpiryError.Message, 
            "expires.value");
    
        private static readonly CreatePreviewTokenValidationError ExpectInvalidCreated = new(
            InvalidCreatedError.Code, 
            InvalidCreatedError.Message, 
            "activates.value");

        [Fact]
        public async Task Success()
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

            var label = new string('A', count: 100);
            var response = await CreatePreviewToken(dataSetVersion.Id, label);

            var (viewModel, createdEntityId) =
                response.AssertCreated<PreviewTokenViewModel>(
                    expectedLocationPrefix: "http://localhost/api/public-data/preview-tokens/");
            Assert.True(Guid.TryParse(createdEntityId, out var previewTokenId));

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, viewModel.Id),
                () => Assert.Equal(label, viewModel.Label),
                () => Assert.Equal(PreviewTokenStatus.Active, viewModel.Status),
                () => Assert.Equal(CreatedByBauUser.Email, viewModel.CreatedByEmail),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Expiry.AssertEqual(DateTimeOffset.UtcNow.AddDays(1)),
                () => Assert.Null(viewModel.Updated)
            );

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = Assert.Single(await publicDataDbContext.PreviewTokens.ToListAsync());

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, actualPreviewToken.Id),
                () => Assert.Equal(label, actualPreviewToken.Label),
                () => Assert.Equal(dataSetVersion.Id, actualPreviewToken.DataSetVersionId),
                () => Assert.Equal(CreatedByBauUser.Id, actualPreviewToken.CreatedByUserId),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Expiry.AssertEqual(DateTimeOffset.UtcNow.AddDays(1)),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }
        
        public static TheoryData<DateTimeOffset, DateTimeOffset, CreatePreviewTokenValidationError> CustomDateOutOfRangeData => new()
        {
            { DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(2), ExpectInvalidCreated }, // Start date is in the past and therefore is out of range.
            { DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(15), ExpectInvalidExpiry }, // End date is longer than 7 days and therefore is out of range. 
            { DateTimeOffset.UtcNow.AddDays(8), DateTimeOffset.UtcNow.AddDays(9), ExpectInvalidCreated }, // Start date beyond 7 days from current time and therefore is out of range.
            { DateTimeOffset.UtcNow.AddDays(6), DateTimeOffset.UtcNow.AddDays(14), ExpectInvalidExpiry }, // Duration is longer than 7 days and therefore is out of range.
        };

        [Theory]
        [MemberData(nameof(CustomDateOutOfRangeData))]
        public async Task CustomDateOutOfRange_ReturnsValidationProblem(DateTimeOffset activates, DateTimeOffset expires, CreatePreviewTokenValidationError expectedError)
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
                expires: expires);

            var validationProblem = response.AssertValidationProblem();

            // Assert that the validation error is for the date range
            validationProblem.AssertHasError(
                expectedPath: expectedError.Path,
                expectedCode: expectedError.Code,
                expectedMessage: expectedError.Message
            );
        }
        
        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.StatusesExceptDraft),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
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
                expectedPath: nameof(PreviewTokenCreateRequest.DataSetVersionId).ToLowerFirst());
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
            validationProblem.AssertHasMaximumLengthError(nameof(PreviewTokenCreateRequest.Label).ToLowerFirst(),
                maxLength: 100);
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
            DateTimeOffset? expires = null)
        {
            client ??= BuildApp().CreateClient();

            var request = new PreviewTokenCreateRequest
            {
                DataSetVersionId = dataSetVersionId,
                Activates = activates,
                Expires = expires,
                Label = label
            };

            return await client.PostAsJsonAsync(BaseUrl, request);
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
                .WithPreviewTokens(() => DataFixture.DefaultPreviewToken()
                    .WithCreatedByUserId(CreatedByBauUser.Id)
                    .Generate(2))
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
                Created = previewToken.Created,
                CreatedByEmail = CreatedByBauUser.Email,
                Expiry = previewToken.Expiry,
                Updated = previewToken.Updated
            };

            response.AssertOk(expectedResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PreviewTokenIsExpiredOrNotActiveYet_StatusIsExpiredOrPending(bool toggleBoolean)
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                [
                    // Test when activated is true and expired is also true.
                    // Also test when activated is not yet & expired is also not yet.
                    // Both result in non-valid tokens 
                    DataFixture.DefaultPreviewToken(activated: toggleBoolean, expired: toggleBoolean)
                        .WithCreatedByUserId(CreatedByBauUser.Id)
                ])
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
            Assert.Equal(toggleBoolean ? PreviewTokenStatus.Expired : PreviewTokenStatus.Pending, viewModel.Status);
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

        private async Task<HttpResponseMessage> GetPreviewToken(
            Guid previewTokenId,
            HttpClient? client = null)
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
                .WithPreviewTokens(() => DataFixture.DefaultPreviewToken()
                    .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                    .WithCreatedByUserId(CreatedByBauUser.Id)
                    .Generate(2))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            await TestApp.AddTestData<ContentDbContext>(context => context.Users.Add(CreatedByBauUser));

            var response = await ListPreviewTokens(dataSetVersion.Id);

            var expectedViewModels = dataSetVersion.PreviewTokens
                .Select(pt => new PreviewTokenViewModel
                {
                    Id = pt.Id,
                    Label = pt.Label,
                    Status = pt.Status,
                    Created = pt.Created,
                    CreatedByEmail = CreatedByBauUser.Email,
                    Expiry = pt.Expiry,
                    Updated = pt.Updated
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
                .WithPreviewTokens(() =>
                [
                    DataFixture.DefaultPreviewToken()
                        .WithCreatedByUserId(CreatedByBauUser.Id)
                ])
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
                .WithPreviewTokens(() => DataFixture.DefaultPreviewToken()
                    .ForIndex(0, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddHours(1)))
                    .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddDays(-1)))
                    .ForIndex(2, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                    .ForIndex(3, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddDays(1)))
                    .WithCreatedByUserId(CreatedByBauUser.Id)
                    .Generate(4))
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
                dataSetVersion.PreviewTokens[1].Id
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

        private async Task<HttpResponseMessage> ListPreviewTokens(
            Guid dataSetVersionId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var queryParams = new Dictionary<string, string?>
            {
                { "dataSetVersionId", dataSetVersionId.ToString() },
            };

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
                .WithPreviewTokens(() =>
                [
                    DataFixture.DefaultPreviewToken()
                        .WithCreatedByUserId(CreatedByBauUser.Id)
                ])
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
                () => viewModel.Expiry.AssertUtcNow(),
                () => viewModel.Updated.AssertUtcNow()
            );

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = await publicDataDbContext.PreviewTokens
                .SingleAsync(pt => pt.Id == dataSetVersion.PreviewTokens[0].Id);

            Assert.Multiple(
                () => Assert.Equal(PreviewTokenStatus.Expired, actualPreviewToken.Status),
                () => actualPreviewToken.Expiry.AssertUtcNow(),
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
                expectedMessage: ValidationMessages.PreviewTokenExpired.Message);

            await using var publicDataDbContext = TestApp.GetDbContext<PublicDataDbContext>();

            var actualPreviewToken = await publicDataDbContext.PreviewTokens
                .SingleAsync(pt => pt.Id == dataSetVersion.PreviewTokens[0].Id);

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

        private async Task<HttpResponseMessage> RevokePreviewToken(
            Guid previewTokenId,
            HttpClient? client = null)
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
