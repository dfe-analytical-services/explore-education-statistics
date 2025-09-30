using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Validators;
using GovUk.Education.ExploreEducationStatistics.Notifier.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Notify.Models.Responses;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public abstract class ApiSubscriptionFunctionsTests(NotifierFunctionsIntegrationTestFixture fixture)
    : NotifierFunctionsIntegrationTest(fixture)
{
    private readonly Guid _dataSetId = Guid.NewGuid();
    private readonly Guid _dataSetFileId = Guid.NewGuid();
    private const string DataSetTitle = "data set title";
    private const string Version = "1.0";
    private const string Email = "test@test.com";

    public class RequestPendingSubscriptionTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionFunctionsTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            string? verificationUrl = null;
            fixture.NotificationClient
                .Setup(mock => mock.SendEmail(
                    Email,
                    GetGovUkNotifyOptions().EmailTemplates.ApiSubscriptionVerificationId,
                    It.Is<Dictionary<string, dynamic>>(personalisation =>
                        AssertEmailTemplateValues(
                            personalisation,
                            DataSetTitle,
                            $"{GetAppOptions().PublicAppUrl}/api-subscriptions/{_dataSetId}/confirm-subscription/")
                    ),
                    null,
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((
                        // ReSharper disable UnusedParameter.Local
                        string email,
                        string templateId,
                        Dictionary<string, dynamic> personalisation,
                        string clientReference,
                        string emailReplyToId,
                        string oneClickUnsubscribeUrl)
                    // ReSharper restore UnusedParameter.Local
                    => verificationUrl = personalisation[NotifierEmailTemplateFields.VerificationUrl]);

            var result = await RequestPendingSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: DataSetTitle,
                email: Email);

            var response = result.AssertOkObjectResult<ApiSubscriptionViewModel>();

            Assert.Equal(_dataSetId, response.DataSetId);
            Assert.Equal(DataSetTitle, response.DataSetTitle);
            Assert.Equal(Email, response.Email);
            Assert.Equal(ApiSubscriptionStatus.Pending, response.Status);

            // Assert that the verification link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(verificationUrl);
            Assert.Equal(Email, extractedEmail);

            var subscription = await GetApiSubscriptionIfExists(
                dataSetId: _dataSetId,
                email: Email);

            MockUtils.VerifyAllMocks(fixture.NotificationClient);

            Assert.NotNull(subscription);
            Assert.Equal(Email, subscription.RowKey);
            Assert.Equal(_dataSetId.ToString(), subscription.PartitionKey);
            Assert.Equal(DataSetTitle, subscription.DataSetTitle);
            Assert.Equal(ApiSubscriptionStatus.Pending, subscription.Status);
            subscription.Expiry.AssertEqual(DateTimeOffset.UtcNow.AddHours(1));
            subscription.Timestamp.AssertUtcNow();
        }

        [Fact]
        public async Task UserAlreadyHasPendingSubscription_400()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTimeOffset.UtcNow.AddHours(1),
            };

            await CreateApiSubscription(subscription);

            var result = await RequestPendingSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: DataSetTitle,
                email: Email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst(),
                expectedCode: ValidationMessages.ApiPendingSubscriptionAlreadyExists.Code);
        }

        [Fact]
        public async Task UserAlreadyHasVerifiedSubscription_400()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed,
                Expiry = null
            };

            await CreateApiSubscription(subscription);

            var result = await RequestPendingSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: DataSetTitle,
                email: Email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst(),
                expectedCode: ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmailEmpty_400(string? email)
        {
            var result = await RequestPendingSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: DataSetTitle,
                email: email!);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.Email).ToLowerFirst());
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test@")]
        [InlineData("@")]
        [InlineData("@test")]
        public async Task EmailNotValid_400(string email)
        {
            var result = await RequestPendingSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: DataSetTitle,
                email: email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasEmailError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.Email).ToLowerFirst());
        }

        [Fact]
        public async Task DataSetIdEmpty_400()
        {
            var result = await RequestPendingSubscription(
                dataSetId: Guid.Empty,
                dataSetTitle: DataSetTitle,
                email: Email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.DataSetId).ToLowerFirst());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DataSetTitleEmpty_400(string? dataSetTitle)
        {
            var result = await RequestPendingSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: dataSetTitle!,
                email: Email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.DataSetTitle).ToLowerFirst());
        }

        private async Task<IActionResult> RequestPendingSubscription(
            Guid dataSetId,
            string dataSetTitle,
            string email)
        {
            var request = new PendingApiSubscriptionCreateRequest
            {
                DataSetId = dataSetId,
                DataSetTitle = dataSetTitle,
                Email = email
            };

            var functions = GetRequiredService<ApiSubscriptionFunctions>();

            return await functions.RequestPendingSubscription(
                request: request,
                cancellationToken: CancellationToken.None);
        }

        private static bool AssertEmailTemplateValues(
            Dictionary<string, dynamic> personalisation,
            string dataSetTitle,
            string verificationUrlPrefix)
        {
            Assert.Equal(dataSetTitle, personalisation[NotifierEmailTemplateFields.DataSetTitle]);
            Assert.StartsWith(verificationUrlPrefix, personalisation[NotifierEmailTemplateFields.VerificationUrl]);
            return true;
        }
    }

    public class VerifySubscriptionTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionFunctionsTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var pendingSubscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTime.UtcNow.AddHours(1),
            };

            await CreateApiSubscription(pendingSubscription);

            string? unsubscribeUrl = null;
            fixture.NotificationClient
                .Setup(mock => mock.SendEmail(
                    Email,
                    GetGovUkNotifyOptions().EmailTemplates.ApiSubscriptionConfirmationId,
                    It.Is<Dictionary<string, dynamic>>(personalisation =>
                        AssertEmailTemplateValues(
                            personalisation,
                            DataSetTitle,
                            $"{GetAppOptions().PublicAppUrl}/api-subscriptions/{_dataSetId}/confirm-unsubscription/")
                    ),
                    null,
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((
                        // ReSharper disable UnusedParameter.Local
                        string email,
                        string templateId,
                        Dictionary<string, dynamic> personalisation,
                        string clientReference,
                        string emailReplyToId,
                        string oneClickUnsubscribeUrl)
                    // ReSharper restore UnusedParameter.Local
                    => unsubscribeUrl = personalisation[NotifierEmailTemplateFields.UnsubscribeUrl]);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(pendingSubscription.RowKey,
                pendingSubscription.Expiry.Value.UtcDateTime);

            var result = await VerifySubscription(
                dataSetId: _dataSetId,
                token: subscribeToken);

            MockUtils.VerifyAllMocks(fixture.NotificationClient);

            var response = result.AssertOkObjectResult<ApiSubscriptionViewModel>();

            Assert.Equal(_dataSetId, response.DataSetId);
            Assert.Equal(DataSetTitle, response.DataSetTitle);
            Assert.Equal(Email, response.Email);
            Assert.Equal(ApiSubscriptionStatus.Subscribed, response.Status);

            // Assert that the unsubscribe link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(unsubscribeUrl);
            Assert.Equal(Email, extractedEmail);

            var verifiedSubscription = await GetApiSubscriptionIfExists(
                dataSetId: _dataSetId,
                email: Email);

            Assert.NotNull(verifiedSubscription);
            Assert.Equal(Email, verifiedSubscription.RowKey);
            Assert.Equal(_dataSetId.ToString(), verifiedSubscription.PartitionKey);
            Assert.Equal(DataSetTitle, verifiedSubscription.DataSetTitle);
            Assert.Equal(ApiSubscriptionStatus.Subscribed, verifiedSubscription.Status);
            Assert.Null(verifiedSubscription.Expiry);
            verifiedSubscription.Timestamp.AssertUtcNow();
        }

        [Fact]
        public async Task PendingSubscriptionDoesNotExist_404()
        {
            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(Email, DateTime.UtcNow);

            var result = await VerifySubscription(
                dataSetId: _dataSetId,
                token: subscribeToken);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task VerifiedSubscriptionAlreadyExists_400()
        {
            var verifiedSubscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed,
                Expiry = null
            };

            await CreateApiSubscription(verifiedSubscription);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(verifiedSubscription.RowKey, DateTime.UtcNow);

            var result = await VerifySubscription(
                dataSetId: _dataSetId,
                token: subscribeToken);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetId",
                expectedCode: ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code);
        }

        [Fact]
        public async Task PendingSubscriptionAlreadyExpired_400()
        {
            var verifiedSubscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTimeOffset.UtcNow.AddHours(-1)
            };

            await CreateApiSubscription(verifiedSubscription);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(verifiedSubscription.RowKey, DateTime.UtcNow);

            var result = await VerifySubscription(
                dataSetId: _dataSetId,
                token: subscribeToken);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetId",
                expectedCode: ValidationMessages.ApiPendingSubscriptionAlreadyExpired.Code);
        }

        [Fact]
        public async Task SubscribeTokenInvalid_400()
        {
            var result = await VerifySubscription(
                dataSetId: _dataSetId,
                token: "");

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "token",
                expectedCode: ValidationMessages.AuthorizationTokenInvalid.Code);
        }

        private async Task<IActionResult> VerifySubscription(
            Guid dataSetId,
            string token)
        {
            var functions = GetRequiredService<ApiSubscriptionFunctions>();

            return await functions.VerifySubscription(
                request: null!,
                dataSetId: dataSetId,
                token: token,
                cancellationToken: CancellationToken.None);
        }

        private static bool AssertEmailTemplateValues(
            Dictionary<string, dynamic> personalisation,
            string dataSetTitle,
            string unsubscribeUrlPrefix)
        {
            Assert.Equal(dataSetTitle, personalisation[NotifierEmailTemplateFields.DataSetTitle]);
            Assert.StartsWith(unsubscribeUrlPrefix, personalisation[NotifierEmailTemplateFields.UnsubscribeUrl]);
            return true;
        }
    }

    public class UnsubscribeTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionFunctionsTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed,
                Expiry = null
            };

            await CreateApiSubscription(subscription);

            var tokenService = GetRequiredService<ITokenService>();
            var unsubscribeToken =
                tokenService.GenerateToken(subscription.RowKey, expiryDateTime: DateTime.UtcNow.AddYears(1));

            var result = await Unsubscribe(
                dataSetId: _dataSetId,
                token: unsubscribeToken);

            result.AssertNoContent();

            var deletedSubscription = await GetApiSubscriptionIfExists(
                dataSetId: _dataSetId,
                email: Email);

            Assert.Null(deletedSubscription);
        }

        [Fact]
        public async Task SubscriptionDoesNotExist_404()
        {
            var tokenService = GetRequiredService<ITokenService>();
            var unsubscribeToken = tokenService.GenerateToken(Email, DateTime.UtcNow.AddYears(1));

            var result = await Unsubscribe(
                dataSetId: _dataSetId,
                token: unsubscribeToken);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task SubscriptionIsStillPending_400()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTime.UtcNow.AddHours(1),
            };

            await CreateApiSubscription(subscription);

            var tokenService = GetRequiredService<ITokenService>();
            var unsubscribeToken =
                tokenService.GenerateToken(subscription.RowKey, subscription.Expiry.Value.UtcDateTime);

            var result = await Unsubscribe(
                dataSetId: _dataSetId,
                token: unsubscribeToken);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetId",
                expectedCode: ValidationMessages.ApiSubscriptionHasNotBeenVerified.Code);
        }

        [Fact]
        public async Task UnsubscribeTokenInvalid_400()
        {
            var result = await Unsubscribe(
                dataSetId: _dataSetId,
                token: "");

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "token",
                expectedCode: ValidationMessages.AuthorizationTokenInvalid.Code);
        }

        [Fact]
        public async Task UnsubscribeTokenExpired_400()
        {
            var tokenService = GetRequiredService<ITokenService>();
            var unsubscribeToken = tokenService.GenerateToken(Email, DateTime.UtcNow.AddYears(-1));

            var result = await Unsubscribe(
                dataSetId: _dataSetId,
                token: unsubscribeToken);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "token",
                expectedCode: ValidationMessages.AuthorizationTokenInvalid.Code);
        }

        private async Task<IActionResult> Unsubscribe(
            Guid dataSetId,
            string token)
        {
            var functions = GetRequiredService<ApiSubscriptionFunctions>();

            return await functions.Unsubscribe(
                request: null!,
                dataSetId: dataSetId,
                token: token,
                cancellationToken: CancellationToken.None);
        }
    }

    public class NotifySubscribersTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionFunctionsTests(fixture)
    {
        [Theory]
        [InlineData("2.0.0", "api-subscription-major-data-set-version-published-id")]
        [InlineData("2.0", "api-subscription-major-data-set-version-published-id")]
        [InlineData("3.0.0", "api-subscription-major-data-set-version-published-id")]
        [InlineData("3.0", "api-subscription-major-data-set-version-published-id")]
        [InlineData("1.1.0", "api-subscription-data-set-version-published-id")]
        [InlineData("1.1", "api-subscription-data-set-version-published-id")]
        [InlineData("1.0.1", "api-subscription-data-set-version-published-id")]
        [InlineData("2.1.0", "api-subscription-data-set-version-published-id")]
        [InlineData("2.1", "api-subscription-data-set-version-published-id")]
        [InlineData("2.0.1", "api-subscription-data-set-version-published-id")]
        [InlineData("1.0.0", "api-subscription-data-set-version-published-id")]
        [InlineData("1.0", "api-subscription-data-set-version-published-id")]
        [InlineData("0.0.1", "api-subscription-data-set-version-published-id")]
        public async Task Success(string version, string templateId)
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed,
                Expiry = null
            };
            await CreateApiSubscription(subscription);
            string? unsubscribeUrl = null;
            fixture.NotificationClient
                .Setup(mock => mock.SendEmail(
                    Email,
                    templateId,
                    It.Is<Dictionary<string, dynamic>>(personalisation =>
                        AssertEmailTemplateValues(
                            personalisation,
                            DataSetTitle,
                            $"{GetAppOptions().PublicAppUrl}/data-catalogue/data-set/{_dataSetFileId}",
                            version,
                            $"{GetAppOptions().PublicAppUrl}/api-subscriptions/{_dataSetId}/confirm-unsubscription/")
                    ),
                    null,
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((
                        // ReSharper disable UnusedParameter.Local
                        string email,
                        string templateIdArg,
                        Dictionary<string, dynamic> personalisation,
                        string clientReference,
                        string emailReplyToId,
                        string oneClickUnsubscribeUrl)
                    // ReSharper restore UnusedParameter.Local
                    => unsubscribeUrl = personalisation[NotifierEmailTemplateFields.UnsubscribeUrl]);

            await NotifyApiSubscribers(
                dataSetId: _dataSetId,
                dataSetFileId: _dataSetFileId,
                version: version);

            MockUtils.VerifyAllMocks(fixture.NotificationClient);

            // Assert that the unsubscribe link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(unsubscribeUrl);
            Assert.Equal(Email, extractedEmail);
        }

        [Fact]
        public async Task DataSetHasNoVerifiedSubscriptions()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = _dataSetId.ToString(),
                RowKey = Email,
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTime.UtcNow.AddHours(1)
            };

            await CreateApiSubscription(subscription);

            await NotifyApiSubscribers(
                dataSetId: _dataSetId,
                dataSetFileId: _dataSetFileId,
                version: Version);

            // Expect no interactions with NotificationClient
        }

        [Fact]
        public async Task DataSetIdEmpty_ThrowsValidationException()
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                NotifyApiSubscribers(
                    dataSetId: Guid.Empty,
                    dataSetFileId: _dataSetFileId,
                    version: Version));

            Assert.Equal("Validation failed:  -- DataSetId: Must not be empty. Severity: Error",
                exception.Message.StripLines());
        }

        [Fact]
        public async Task DataSetFileIdEmpty_ThrowsValidationException()
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                NotifyApiSubscribers(
                    dataSetId: _dataSetId,
                    dataSetFileId: Guid.Empty,
                    version: Version));

            Assert.Equal("Validation failed:  -- DataSetFileId: Must not be empty. Severity: Error",
                exception.Message.StripLines());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task VersionEmpty_ThrowsValidationException(string? version)
        {
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                NotifyApiSubscribers(
                    dataSetId: _dataSetId,
                    dataSetFileId: _dataSetFileId,
                    version: version!));

            Assert.Equal("Validation failed:  -- Version: Must not be empty. Severity: Error",
                exception.Message.StripLines());
        }

        private async Task NotifyApiSubscribers(
            Guid dataSetId,
            Guid dataSetFileId,
            string version)
        {
            var message = new ApiNotificationMessage
            {
                DataSetId = dataSetId,
                DataSetFileId = dataSetFileId,
                Version = version
            };

            var functions = GetRequiredService<ApiSubscriptionFunctions>();

            await functions.NotifySubscribers(
                message: message,
                cancellationToken: CancellationToken.None);
        }

        private static bool AssertEmailTemplateValues(
            Dictionary<string, dynamic> personalisation,
            string dataSetTitle,
            string dataSetUrl,
            string dataSetVersion,
            string unsubscribeUrlPrefix)
        {
            Assert.Equal(dataSetTitle, personalisation[NotifierEmailTemplateFields.DataSetTitle]);
            Assert.Equal(dataSetUrl, personalisation[NotifierEmailTemplateFields.DataSetUrl]);
            Assert.Equal(dataSetVersion, personalisation[NotifierEmailTemplateFields.DataSetVersion]);
            Assert.StartsWith(unsubscribeUrlPrefix, personalisation[NotifierEmailTemplateFields.UnsubscribeUrl]);
            return true;
        }
    }

    public class RemoveExpiredSubscriptionsTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionFunctionsTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var pendingAndExpiredSubscription = new ApiSubscription
            {
                PartitionKey = Email,
                RowKey = Guid.NewGuid().ToString(),
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTime.UtcNow.AddHours(-1),
            };

            var pendingSubscription = new ApiSubscription
            {
                PartitionKey = Email,
                RowKey = Guid.NewGuid().ToString(),
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Pending,
                Expiry = DateTime.UtcNow.AddHours(1),
            };

            var subscribedSubscription = new ApiSubscription
            {
                PartitionKey = Email,
                RowKey = Guid.NewGuid().ToString(),
                DataSetTitle = DataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed,
                Expiry = null,
            };

            await CreateApiSubscriptions(pendingAndExpiredSubscription, pendingSubscription, subscribedSubscription);

            await RemoveExpiredSubscriptions();

            var subscriptions = await QueryApiSubscriptions();

            Assert.Equal(2, subscriptions.Count);

            Assert.Single(subscriptions, s => s.RowKey == pendingSubscription.RowKey);
            Assert.Single(subscriptions, s => s.RowKey == subscribedSubscription.RowKey);
        }

        private async Task RemoveExpiredSubscriptions(TimerInfo? timerInfo = null)
        {
            var functions = GetRequiredService<ApiSubscriptionFunctions>();

            await functions.RemoveExpiredSubscriptions(
                timerInfo: timerInfo ?? new TimerInfo(),
                cancellationToken: CancellationToken.None);
        }
    }

    public class EmailTemplateOptions
    {
        [Theory]
        [InlineData("2.0.0", "breaking-change-template-id")]
        [InlineData("2.0", "breaking-change-template-id")]
        [InlineData("3.0.0", "breaking-change-template-id")]
        [InlineData("3.0", "breaking-change-template-id")]
        [InlineData("1.1.0", "non-breaking-change-template-id")]
        [InlineData("1.1", "non-breaking-change-template-id")]
        [InlineData("1.0.1", "non-breaking-change-template-id")]
        [InlineData("2.1.0", "non-breaking-change-template-id")]
        [InlineData("2.1", "non-breaking-change-template-id")]
        [InlineData("2.0.1", "non-breaking-change-template-id")]
        [InlineData("1.0.0", "non-breaking-change-template-id")]
        [InlineData("1.0", "non-breaking-change-template-id")]
        [InlineData("0.0.1", "non-breaking-change-template-id")]
        public void SelectDataSetPublishedTemplateID_ReturnsCorrectTemplateId(string version, string expectedTemplateId)
        {
            GovUkNotifyOptions govUkNotifyOptions = new()
            {
                EmailTemplates = new GovUkNotifyOptions.EmailTemplateOptions
                {
                    ApiSubscriptionMajorDataSetVersionPublishedId = "breaking-change-template-id",
                    ApiSubscriptionDataSetVersionPublishedId = "non-breaking-change-template-id"
                }
            };
            var templateId = govUkNotifyOptions.EmailTemplates.SelectDataSetPublishedTemplateId(version);
            Assert.Equal(expectedTemplateId, templateId);
        }

        [Theory]
        [InlineData("2.*.0")]
        [InlineData("Not a version")]
        [InlineData("1.1.1.1")]
        [InlineData("")]
        public void SelectDataSetPublishedTemplateID_ThrowsArgumentException(string version)
        {
            GovUkNotifyOptions govUkNotifyOptions = new()
            {
                EmailTemplates = new GovUkNotifyOptions.EmailTemplateOptions
                {
                    ApiSubscriptionMajorDataSetVersionPublishedId = "breaking-change-template-id",
                    ApiSubscriptionDataSetVersionPublishedId = "non-breaking-change-template-id"
                }
            };
            var exception = Assert.Throws<ArgumentException>(() => govUkNotifyOptions.EmailTemplates.SelectDataSetPublishedTemplateId(version));
            Assert.Equal(ValidationMessages.InvalidDataSetVersion.Message, exception.Message);
        }
    }

    private string? ExtractEmailFromSubscriptionLinkToken(string? subscriptionLink)
    {
        if (subscriptionLink == null)
        {
            return null;
        }

        var tokenService = GetRequiredService<ITokenService>();

        var unsubscribeToken = ExtractToken(subscriptionLink);

        return tokenService.GetEmailFromToken(unsubscribeToken);
    }

    private static string ExtractToken(string subscriptionLink) => subscriptionLink.Split("/").Last();
}
