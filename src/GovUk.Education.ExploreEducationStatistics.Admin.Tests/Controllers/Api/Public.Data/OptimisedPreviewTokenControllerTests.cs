#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// ReSharper disable once ClassNeverInstantiated.Global
public class OptimisedPreviewTokenControllerTestsFixture : OptimisedHttpClientWithPsqlFixture;

[CollectionDefinition(nameof(OptimisedPreviewTokenControllerTestsFixture))]
public class OptimisedPreviewTokenControllerTestsCollection
    : ICollectionFixture<OptimisedPreviewTokenControllerTestsFixture>;

[Collection(nameof(OptimisedPreviewTokenControllerTestsFixture))]
public abstract class OptimisedPreviewTokenControllerTests
{
    private const string BaseUrl = "api/public-data/preview-tokens";

    private readonly DataFixture _dataFixture = new(new Random().Next());

    public class CreatePreviewTokenTests(OptimisedPreviewTokenControllerTestsFixture fixture)
        : OptimisedPreviewTokenControllerTests
    {
        [Fact]
        public async Task Success()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

            var label = new string('A', count: 100);

            var response = await CreatePreviewToken(
                dataSetVersionId: dataSetVersion.Id,
                label: label,
                user: bauUser
            );

            var (viewModel, createdEntityId) = response.AssertCreated<PreviewTokenViewModel>(
                expectedLocationPrefix: "http://localhost/api/public-data/preview-tokens/"
            );
            Assert.True(Guid.TryParse(createdEntityId, out var previewTokenId));

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, viewModel.Id),
                () => Assert.Equal(label, viewModel.Label),
                () => Assert.Equal(PreviewTokenStatus.Active, viewModel.Status),
                () => Assert.Equal(bauUserEntry.Email, viewModel.CreatedByEmail),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Expiry.AssertEqual(DateTimeOffset.UtcNow.AddDays(1)),
                () => Assert.Null(viewModel.Updated)
            );

            var actualPreviewToken = Assert.Single(
                await fixture.GetPublicDataDbContext().PreviewTokens.ToListAsync()
            );

            Assert.Multiple(
                () => Assert.Equal(previewTokenId, actualPreviewToken.Id),
                () => Assert.Equal(label, actualPreviewToken.Label),
                () => Assert.Equal(dataSetVersion.Id, actualPreviewToken.DataSetVersionId),
                () => Assert.Equal(bauUserEntry.Id, actualPreviewToken.CreatedByUserId),
                () => viewModel.Created.AssertUtcNow(),
                () => viewModel.Expiry.AssertEqual(DateTimeOffset.UtcNow.AddDays(1)),
                () => Assert.Null(actualPreviewToken.Updated)
            );
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.StatusesExceptDraft),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionIsNotDraft_ReturnsValidationProblem(
            DataSetVersionStatus status
        )
        {
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
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
            validationProblem.AssertHasNotEmptyError(
                nameof(PreviewTokenCreateRequest.DataSetVersionId).ToLowerFirst()
            );
        }

        [Fact]
        public async Task LabelIsEmpty_ReturnsValidationProblem()
        {
            var response = await CreatePreviewToken(Guid.NewGuid(), label: string.Empty);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasNotEmptyError(
                nameof(PreviewTokenCreateRequest.Label).ToLowerFirst()
            );
        }

        [Fact]
        public async Task LabelAboveMaximumLength_ReturnsValidationProblem()
        {
            var response = await CreatePreviewToken(
                Guid.NewGuid(),
                label: new string('A', count: 101)
            );

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
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
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
                dataSetVersionId: dataSetVersion.Id,
                label: "Label",
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> CreatePreviewToken(
            Guid dataSetVersionId,
            string label,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient().WithUser(user ?? OptimisedTestUsers.Bau);

            var request = new PreviewTokenCreateRequest
            {
                DataSetVersionId = dataSetVersionId,
                Label = label,
            };

            return await client.PostAsJsonAsync(BaseUrl, request);
        }
    }

    public class GetPreviewTokenTests(OptimisedPreviewTokenControllerTestsFixture fixture)
        : OptimisedPreviewTokenControllerTests
    {
        [Fact]
        public async Task Success()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    _dataFixture
                        .DefaultPreviewToken()
                        .WithCreatedByUserId(bauUserEntry.Id)
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await GetPreviewToken(previewTokenId: previewToken.Id, user: bauUser);

            var expectedResult = new PreviewTokenViewModel
            {
                Id = previewToken.Id,
                Label = previewToken.Label,
                Status = PreviewTokenStatus.Active,
                Created = previewToken.Created,
                CreatedByEmail = bauUserEntry.Email,
                Expiry = previewToken.Expiry,
                Updated = previewToken.Updated,
            };

            response.AssertOk(expectedResult);
        }

        [Fact]
        public async Task PreviewTokenIsExpired_StatusIsExpired()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>

                    [
                        _dataFixture
                            .DefaultPreviewToken(expired: true)
                            .WithCreatedByUserId(bauUserEntry.Id),
                    ]
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

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
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await GetPreviewToken(
                previewTokenId: dataSetVersion.PreviewTokens[0].Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> GetPreviewToken(
            Guid previewTokenId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient().WithUser(user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/{previewTokenId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ListPreviewTokensTests(OptimisedPreviewTokenControllerTestsFixture fixture)
        : OptimisedPreviewTokenControllerTests
    {
        [Fact]
        public async Task Success()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    _dataFixture
                        .DefaultPreviewToken()
                        .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                        .WithCreatedByUserId(bauUserEntry.Id)
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

            var response = await ListPreviewTokens(dataSetVersion.Id);

            var expectedViewModels = dataSetVersion
                .PreviewTokens.Select(pt => new PreviewTokenViewModel
                {
                    Id = pt.Id,
                    Label = pt.Label,
                    Status = pt.Status,
                    Created = pt.Created,
                    CreatedByEmail = bauUserEntry.Email,
                    Expiry = pt.Expiry,
                    Updated = pt.Updated,
                })
                .ToList();

            response.AssertOk(expectedViewModels);
        }

        [Fact]
        public async Task MultipleDataSetVersions_FiltersByDataSetVersion()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            var (dataSetVersion1, dataSetVersion2) = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    [_dataFixture.DefaultPreviewToken().WithCreatedByUserId(bauUserEntry.Id)]
                )
                .GenerateTuple2();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2);
                    context.DataSets.Update(dataSet);
                });

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

            var response = await ListPreviewTokens(dataSetVersion1.Id);

            var viewModels = response.AssertOk<List<PreviewTokenViewModel>>();

            Assert.Single(viewModels);
            Assert.Equal(dataSetVersion1.PreviewTokens[0].Id, viewModels[0].Id);
        }

        [Fact]
        public async Task DataSetVersionHasMultiplePreviewTokens_OrdersByDescendingExpiry()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    _dataFixture
                        .DefaultPreviewToken()
                        .ForIndex(0, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddHours(1)))
                        .ForIndex(1, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddDays(-1)))
                        .ForIndex(2, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddSeconds(-1)))
                        .ForIndex(3, pt => pt.SetExpiry(DateTimeOffset.UtcNow.AddDays(1)))
                        .WithCreatedByUserId(bauUserEntry.Id)
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

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

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
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
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
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
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

            var response = await ListPreviewTokens(
                dataSetVersionId: dataSetVersion.Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> ListPreviewTokens(
            Guid dataSetVersionId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient().WithUser(user ?? OptimisedTestUsers.Bau);

            var queryParams = new Dictionary<string, string?>
            {
                { "dataSetVersionId", dataSetVersionId.ToString() },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class RevokePreviewTokenTests(OptimisedPreviewTokenControllerTestsFixture fixture)
        : OptimisedPreviewTokenControllerTests
    {
        [Fact]
        public async Task Success()
        {
            ClaimsPrincipal bauUser = _dataFixture.BauUser();
            var bauUserEntry = new User { Id = bauUser.GetUserId(), Email = bauUser.GetEmail() };
            fixture.RegisterTestUser(bauUser);

            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() =>
                    [_dataFixture.DefaultPreviewToken().WithCreatedByUserId(bauUserEntry.Id)]
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Users.Add(bauUserEntry));

            var previewToken = dataSetVersion.PreviewTokens[0];

            var response = await RevokePreviewToken(previewToken.Id);

            var viewModel = response.AssertOk<PreviewTokenViewModel>();

            Assert.Multiple(
                () => Assert.Equal(previewToken.Id, viewModel.Id),
                () => Assert.Equal(previewToken.Label, viewModel.Label),
                () => Assert.Equal(PreviewTokenStatus.Expired, viewModel.Status),
                () => Assert.Equal(bauUserEntry.Email, viewModel.CreatedByEmail),
                () => Assert.Equal(previewToken.Created.TruncateNanoseconds(), viewModel.Created),
                () => viewModel.Expiry.AssertUtcNow(),
                () => viewModel.Updated.AssertUtcNow()
            );

            var actualPreviewToken = await fixture
                .GetPublicDataDbContext()
                .PreviewTokens.SingleAsync(pt => pt.Id == dataSetVersion.PreviewTokens[0].Id);

            Assert.Multiple(
                () => Assert.Equal(PreviewTokenStatus.Expired, actualPreviewToken.Status),
                () => actualPreviewToken.Expiry.AssertUtcNow(),
                () => actualPreviewToken.Updated.AssertUtcNow()
            );
        }

        [Fact]
        public async Task PreviewTokenIsExpired_ReturnsValidationProblem()
        {
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken(expired: true)])
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
            DataSet dataSet = _dataFixture.DefaultDataSet();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithPreviewTokens(() => [_dataFixture.DefaultPreviewToken()])
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await RevokePreviewToken(
                previewTokenId: dataSetVersion.PreviewTokens[0].Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> RevokePreviewToken(
            Guid previewTokenId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient().WithUser(user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/{previewTokenId}/revoke", UriKind.Relative);

            return await client.PostAsync(uri, content: null);
        }
    }
}
