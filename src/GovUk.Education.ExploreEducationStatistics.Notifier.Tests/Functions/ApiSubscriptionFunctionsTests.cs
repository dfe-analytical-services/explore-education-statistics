using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
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
    private const string DataSetTitle = "data set title";
    private const string Email = "test@test.com";

    public class RequestPendingApiSubscriptionTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionFunctionsTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            string verificationLink = null!;
            fixture._notificationClient
                .Setup(mock => mock.SendEmail(
                    Email,
                    GetGovUkNotifyOptions().EmailTemplates.ApiSubscriptionVerificationId,
                    It.Is<Dictionary<string, dynamic>>(values =>
                        AssertEmailTemplateValues(
                            values,
                            DataSetTitle,
                            $"{GetAppSettingsOptions().PublicAppUrl}/api-subscriptions/{_dataSetId}/confirm-subscription/",
                            null)
                    ),
                    null,
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((
                        string email,
                        string templateId,
                        Dictionary<string, dynamic> values,
                        string clientReference,
                        string emailReplyToId,
                        string oneClickUnsubscribeURL)
                    => verificationLink = values["verification_link"]);

            var result = await RequestPendingApiSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: DataSetTitle,
                email: Email);

            var response = result.AssertOkObjectResult<ApiSubscriptionViewModel>();

            Assert.Equal(_dataSetId, response.DataSetId);
            Assert.Equal(DataSetTitle, response.DataSetTitle);
            Assert.Equal(Email, response.Email);
            Assert.Equal(ApiSubscriptionStatus.Pending, response.Status);

            // Assert that the verification link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(verificationLink);
            Assert.Equal(Email, extractedEmail);

            var subscription = await GetApiSubscriptionIfExists(
                dataSetId: _dataSetId,
                email: Email);

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

            var result = await RequestPendingApiSubscription(
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
                Status = ApiSubscriptionStatus.Subscribed
            };

            await CreateApiSubscription(subscription);

            var result = await RequestPendingApiSubscription(
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
            var result = await RequestPendingApiSubscription(
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
            var result = await RequestPendingApiSubscription(
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
            var result = await RequestPendingApiSubscription(
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
            var result = await RequestPendingApiSubscription(
                dataSetId: _dataSetId,
                dataSetTitle: dataSetTitle!,
                email: Email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(
                expectedPath: nameof(PendingApiSubscriptionCreateRequest.DataSetTitle).ToLowerFirst());
        }

        private async Task<IActionResult> RequestPendingApiSubscription(
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

            var apiSubscriptionManager = GetRequiredService<ApiSubscriptionFunctions>();

            return await apiSubscriptionManager.RequestPendingApiSubscription(
                request: request,
                cancellationToken: CancellationToken.None);
        }
    }

    public class VerifyApiSubscriptionTests(NotifierFunctionsIntegrationTestFixture fixture)
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

            string unsubscribeLink = null!;
            fixture._notificationClient
                .Setup(mock => mock.SendEmail(
                    Email,
                    GetGovUkNotifyOptions().EmailTemplates.ApiSubscriptionConfirmationId,
                    It.Is<Dictionary<string, dynamic>>(values =>
                        AssertEmailTemplateValues(
                            values,
                            DataSetTitle,
                            null,
                            $"{GetAppSettingsOptions().PublicAppUrl}/api-subscriptions/{_dataSetId}/confirm-unsubscription/")
                    ),
                    null,
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((
                        string email,
                        string templateId,
                        Dictionary<string, dynamic> values,
                        string clientReference,
                        string emailReplyToId,
                        string oneClickUnsubscribeURL)
                    => unsubscribeLink = values["unsubscribe_link"]);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(pendingSubscription.RowKey,
                pendingSubscription.Expiry.Value.UtcDateTime);

            var result = await VerifyApiSubscription(
                dataSetId: _dataSetId,
                token: subscribeToken);

            var response = result.AssertOkObjectResult<ApiSubscriptionViewModel>();

            Assert.Equal(_dataSetId, response.DataSetId);
            Assert.Equal(DataSetTitle, response.DataSetTitle);
            Assert.Equal(Email, response.Email);
            Assert.Equal(ApiSubscriptionStatus.Subscribed, response.Status);

            // Assert that the unsubscribe link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(unsubscribeLink);
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

            var result = await VerifyApiSubscription(
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
                Status = ApiSubscriptionStatus.Subscribed
            };

            await CreateApiSubscription(verifiedSubscription);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(verifiedSubscription.RowKey, DateTime.UtcNow);

            var result = await VerifyApiSubscription(
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

            var result = await VerifyApiSubscription(
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
            var result = await VerifyApiSubscription(
                dataSetId: _dataSetId,
                token: "");

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "token",
                expectedCode: ValidationMessages.AuthorizationTokenInvalid.Code);
        }

        private async Task<IActionResult> VerifyApiSubscription(
            Guid dataSetId,
            string token)
        {
            var apiSubscriptionManager = GetRequiredService<ApiSubscriptionFunctions>();

            return await apiSubscriptionManager.VerifyApiSubscription(
                request: null!,
                dataSetId: dataSetId,
                token: token,
                cancellationToken: CancellationToken.None);
        }
    }

    public class ApiUnsubscribeTests(NotifierFunctionsIntegrationTestFixture fixture)
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
                Expiry = DateTimeOffset.UtcNow.AddYears(1)
            };

            await CreateApiSubscription(subscription);

            var tokenService = GetRequiredService<ITokenService>();
            var unsubscribeToken =
                tokenService.GenerateToken(subscription.RowKey, subscription.Expiry.Value.UtcDateTime);

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
                Expiry = DateTimeOffset.UtcNow,
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
            var apiSubscriptionManager = GetRequiredService<ApiSubscriptionFunctions>();

            return await apiSubscriptionManager.ApiUnsubscribe(
                request: null!,
                dataSetId: dataSetId,
                token: token,
                cancellationToken: CancellationToken.None);
        }
    }

    public class RemoveExpiredApiSubscriptionsTests(NotifierFunctionsIntegrationTestFixture fixture)
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

            await RemoveExpiredApiSubscriptions();

            var subscriptions = await QueryApiSubscriptions();

            Assert.Equal(2, subscriptions.Count);

            Assert.Single(subscriptions, s => s.RowKey == pendingSubscription.RowKey);
            Assert.Single(subscriptions, s => s.RowKey == subscribedSubscription.RowKey);
        }

        private async Task RemoveExpiredApiSubscriptions(TimerInfo? timerInfo = null)
        {
            var apiSubscriptionManager = GetRequiredService<ApiSubscriptionFunctions>();

            await apiSubscriptionManager.RemoveExpiredApiSubscriptions(
                timerInfo: timerInfo ?? new TimerInfo(),
                cancellationToken: CancellationToken.None);
        }
    }

    private string? ExtractEmailFromSubscriptionLinkToken(string subscriptionLink)
    {
        var tokenService = GetRequiredService<ITokenService>();

        var unsubscribeToken = ExtractToken(subscriptionLink);

        return tokenService.GetEmailFromToken(unsubscribeToken);
    }

    private static string ExtractToken(string subscriptionLink) => subscriptionLink.Split("/").Last();

    private static bool AssertEmailTemplateValues(
        Dictionary<string, dynamic> values,
        string dataSetTitle,
        string? verificationLinkPrefix,
        string? unsubscribeLinkPrefix)
    {
        Assert.Equal(dataSetTitle, values["api_dataset"]);

        if (verificationLinkPrefix != null)
        {
            Assert.Contains(verificationLinkPrefix, values["verification_link"]);
        }

        if (unsubscribeLinkPrefix != null)
        {
            Assert.Contains(unsubscribeLinkPrefix, values["unsubscribe_link"]);
        }

        return true;
    }
}
