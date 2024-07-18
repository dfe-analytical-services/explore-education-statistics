#nullable enable
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class PreviewTokenControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/preview-tokens";

    private static readonly ClaimsPrincipal BauUser = BauUser();

    private static readonly User CreatedByBauUser = new()
    {
        Id = BauUser.GetUserId(),
        Email = "bau.user@test.com"
    };

    public class CreatePreviewTokenTests(TestApplicationFactory testApp) : PreviewTokenControllerTests(testApp)
    {
        public static TheoryData<DataSetVersionStatus> NonDraftDataSetVersionStatuses = new(
            EnumUtil.GetEnums<DataSetVersionStatus>().Except([DataSetVersionStatus.Draft])
        );

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
                () => Assert.Equal(DateTimeOffset.UtcNow.AddDays(1),
                    viewModel.Expiry,
                    precision: TimeSpan.FromSeconds(2)),
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
                () => Assert.Equal(DateTimeOffset.UtcNow.AddDays(1),
                    viewModel.Expiry,
                    precision: TimeSpan.FromSeconds(2)),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }

        [Theory]
        [MemberData(nameof(NonDraftDataSetVersionStatuses))]
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

            var client = BuildApp(AuthenticatedUser()).CreateClient();

            var response = await CreatePreviewToken(dataSetVersion.Id, "Label", client);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> CreatePreviewToken(
            Guid dataSetVersionId,
            string label,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var request = new PreviewTokenCreateRequest
            {
                DataSetVersionId = dataSetVersionId,
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

        [Fact]
        public async Task ExpiryInPast_StatusIsExpired()
        {
            DataSet dataSet = DataFixture.DefaultDataSet();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => DataFixture.DefaultPreviewToken()
                    .WithExpiry(DateTimeOffset.UtcNow.AddSeconds(-1))
                    .WithCreatedByUserId(CreatedByBauUser.Id)
                    .Generate(1))
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
                .WithPreviewTokens(() => DataFixture.DefaultPreviewToken()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp(AuthenticatedUser()).CreateClient();

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

    private WebApplicationFactory<TestStartup> BuildApp(ClaimsPrincipal? user = null)
    {
        return TestApp.SetUser(user ?? BauUser);
    }
}
