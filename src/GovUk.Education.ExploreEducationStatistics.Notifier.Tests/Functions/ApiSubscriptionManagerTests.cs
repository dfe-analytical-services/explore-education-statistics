using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public abstract class ApiSubscriptionManagerTests(NotifierFunctionsIntegrationTestFixture fixture)
    : NotifierFunctionsIntegrationTest(fixture)
{
    private readonly Guid dataSetId = Guid.NewGuid();
    private readonly string dataSetTitle = "data set title";
    private readonly string email = "test@test.com";

    public class RequestPendingApiSubscriptionTests(NotifierFunctionsIntegrationTestFixture fixture)
        : ApiSubscriptionManagerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            string verificationLink = null!;
            fixture._notificationClient
                .Setup(mock => mock.SendEmail(
                    email,
                    GetGovUkNotifyOptions().EmailTemplates.ApiSubscriptionVerificationId,
                    It.Is<Dictionary<string, dynamic>>(values =>
                        AssertEmailTemplateValues(
                            values,
                            dataSetTitle,
                            $"{GetAppSettingsOptions().PublicAppUrl}/api-subscriptions/{dataSetId}/confirm-subscription/",
                            null)
                    ),
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((string email, string templateId, Dictionary<string, dynamic> values, string clientReference, string emailReplyToId)
                    => verificationLink = values["verification_link"]); ;

            var result = await RequestPendingApiSubscription(
                dataSetId: dataSetId,
                dataSetTitle: dataSetTitle,
                email: email);

            var response = result.AssertOkObjectResult<ApiSubscriptionViewModel>();

            Assert.Equal(dataSetId, response.DataSetId);
            Assert.Equal(dataSetTitle, response.DataSetTitle);
            Assert.Equal(email, response.Email);
            Assert.Equal(ApiSubscriptionStatus.SubscriptionPending, response.Status);

            // Assert that the verification link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(verificationLink);
            Assert.Equal(extractedEmail, email);

            var subscription = await GetApiSubscriptionIfExists(
                dataSetId: dataSetId,
                email: email);

            Assert.NotNull(subscription);
            Assert.Equal(email, subscription.PartitionKey);
            Assert.Equal(dataSetId.ToString(), subscription.RowKey);
            Assert.Equal(dataSetTitle, subscription.DataSetTitle);
            Assert.Equal(ApiSubscriptionStatus.SubscriptionPending, subscription.Status);
            subscription.ExpiryDateTime.AssertEqual(DateTimeOffset.UtcNow.AddHours(1));
            subscription.Timestamp.AssertUtcNow();
        }

        [Fact]
        public async Task UserAlreadyHasPendingSubscription_400()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = email,
                RowKey = dataSetId.ToString(),
                DataSetTitle = dataSetTitle,
                Status = ApiSubscriptionStatus.SubscriptionPending,
                ExpiryDateTime = DateTimeOffset.UtcNow.AddHours(1),
            };

            await CreateApiSubscription(subscription);

            var result = await RequestPendingApiSubscription(
                dataSetId: dataSetId,
                dataSetTitle: dataSetTitle,
                email: email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NewPendingApiSubscriptionRequest.DataSetId).ToLowerFirst(),
                expectedCode: ValidationMessages.ApiPendingSubscriptionAlreadyExists.Code);
        }

        [Fact]
        public async Task UserAlreadyHasVerifiedSubscription_400()
        {
            var subscription = new ApiSubscription
            {
                PartitionKey = email,
                RowKey = dataSetId.ToString(),
                DataSetTitle = dataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed
            };

            await CreateApiSubscription(subscription);

            var result = await RequestPendingApiSubscription(
                dataSetId: dataSetId,
                dataSetTitle: dataSetTitle,
                email: email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NewPendingApiSubscriptionRequest.DataSetId).ToLowerFirst(),
                expectedCode: ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmailEmpty_400(string? email)
        {
            var result = await RequestPendingApiSubscription(
                dataSetId: dataSetId,
                dataSetTitle: dataSetTitle,
                email: email!);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(expectedPath: nameof(NewPendingApiSubscriptionRequest.Email).ToLowerFirst());
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test@")]
        [InlineData("@")]
        [InlineData("@test")]
        public async Task EmailNotValid_400(string email)
        {
            var result = await RequestPendingApiSubscription(
                dataSetId: dataSetId,
                dataSetTitle: dataSetTitle,
                email: email!);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasEmailError(expectedPath: nameof(NewPendingApiSubscriptionRequest.Email).ToLowerFirst());
        }

        [Fact]
        public async Task DataSetIdEmpty_400()
        {
            var result = await RequestPendingApiSubscription(
                dataSetId: Guid.Empty,
                dataSetTitle: dataSetTitle,
                email: email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(expectedPath: nameof(NewPendingApiSubscriptionRequest.DataSetId).ToLowerFirst());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DataSetTitleEmpty_400(string? dataSetTitle)
        {
            var result = await RequestPendingApiSubscription(
                dataSetId: dataSetId,
                dataSetTitle: dataSetTitle!,
                email: email);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasNotEmptyError(expectedPath: nameof(NewPendingApiSubscriptionRequest.DataSetTitle).ToLowerFirst());
        }

        private async Task<IActionResult> RequestPendingApiSubscription(
            Guid dataSetId,
            string dataSetTitle,
            string email,
            FunctionContext? functionContext = null)
        {
            var request = new NewPendingApiSubscriptionRequest
            {
                DataSetId = dataSetId,
                DataSetTitle = dataSetTitle,
                Email = email
            };

            var apiSubscriptionManager = GetRequiredService<ApiSubscriptionManager>();

            return await apiSubscriptionManager.RequestPendingApiSubscription(
                request: request,
                context: functionContext ?? new TestFunctionContext(),
                cancellationToken: CancellationToken.None);
        }
    }

    public class VerifyApiSubscriptionTests(NotifierFunctionsIntegrationTestFixture fixture)
    : ApiSubscriptionManagerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var pendingSubscription = new ApiSubscription
            {
                PartitionKey = email,
                RowKey = dataSetId.ToString(),
                DataSetTitle = dataSetTitle,
                Status = ApiSubscriptionStatus.SubscriptionPending,
                ExpiryDateTime = DateTime.UtcNow,
            };

            await CreateApiSubscription(pendingSubscription);

            string unsubscribeLink = null!;
            fixture._notificationClient
                .Setup(mock => mock.SendEmail(
                    email,
                    GetGovUkNotifyOptions().EmailTemplates.ApiSubscriptionConfirmationId,
                    It.Is<Dictionary<string, dynamic>>(values =>
                        AssertEmailTemplateValues(
                            values,
                            dataSetTitle,
                            null,
                            $"{GetAppSettingsOptions().PublicAppUrl}/api-subscriptions/{dataSetId}/confirm-unsubscription/")
                    ),
                    null,
                    null))
                .Returns(It.IsAny<EmailNotificationResponse>())
                .Callback((string email, string templateId, Dictionary<string, dynamic> values, string clientReference, string emailReplyToId)
                    => unsubscribeLink = values["unsubscribe_link"]);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(pendingSubscription.PartitionKey, pendingSubscription.ExpiryDateTime.Value.UtcDateTime);

            var result = await VerifyApiSubscription(
                dataSetId: dataSetId,
                token: subscribeToken);

            var response = result.AssertOkObjectResult<ApiSubscriptionViewModel>();

            Assert.Equal(dataSetId, response.DataSetId);
            Assert.Equal(dataSetTitle, response.DataSetTitle);
            Assert.Equal(email, response.Email);
            Assert.Equal(ApiSubscriptionStatus.Subscribed, response.Status);

            // Assert that the unsubscribe link contains a valid token
            var extractedEmail = ExtractEmailFromSubscriptionLinkToken(unsubscribeLink);
            Assert.Equal(extractedEmail, email);

            var verifiedSubscription = await GetApiSubscriptionIfExists(
                dataSetId: dataSetId,
                email: email);

            Assert.NotNull(verifiedSubscription);
            Assert.Equal(email, verifiedSubscription.PartitionKey);
            Assert.Equal(dataSetId.ToString(), verifiedSubscription.RowKey);
            Assert.Equal(dataSetTitle, verifiedSubscription.DataSetTitle);
            Assert.Equal(ApiSubscriptionStatus.Subscribed, verifiedSubscription.Status);
            Assert.Null(verifiedSubscription.ExpiryDateTime);
            verifiedSubscription.Timestamp.AssertUtcNow();
        }

        [Fact]
        public async Task PendingSubscriptionDoesNotExist_404()
        {
            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(email, DateTime.UtcNow);

            var result = await VerifyApiSubscription(
                dataSetId: dataSetId,
                token: subscribeToken);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task VerifiedSubscriptionAlreadyExists_400()
        {
            var verifiedSubscription = new ApiSubscription
            {
                PartitionKey = email,
                RowKey = dataSetId.ToString(),
                DataSetTitle = dataSetTitle,
                Status = ApiSubscriptionStatus.Subscribed
            };

            await CreateApiSubscription(verifiedSubscription);

            var tokenService = GetRequiredService<ITokenService>();
            var subscribeToken = tokenService.GenerateToken(verifiedSubscription.PartitionKey, DateTime.UtcNow);

            var result = await VerifyApiSubscription(
                dataSetId: dataSetId,
                token: subscribeToken);

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: nameof(NewPendingApiSubscriptionRequest.DataSetId).ToLowerFirst(),
                expectedCode: ValidationMessages.ApiVerifiedSubscriptionAlreadyExists.Code);
        }

        [Fact]
        public async Task SubscribeTokenInvalid_400()
        {
            var result = await VerifyApiSubscription(
                dataSetId: dataSetId,
                token: "");

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "token",
                expectedCode: ValidationMessages.AuthorizationTokenInvalid.Code);
        }

        private async Task<IActionResult> VerifyApiSubscription(
            Guid dataSetId,
            string token,
            FunctionContext? functionContext = null)
        {
            var apiSubscriptionManager = GetRequiredService<ApiSubscriptionManager>();

            return await apiSubscriptionManager.VerifyApiSubscription(
                context: functionContext ?? new TestFunctionContext(),
                dataSetId: dataSetId,
                token: token,
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
