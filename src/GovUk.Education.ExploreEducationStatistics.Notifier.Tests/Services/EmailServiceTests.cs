using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Services;

public class EmailServiceTests
{
    private const string TestApiKeyErrorMessage = "Can't send to this recipient using a team-only API key";

    private static readonly AppOptions DefaultAppOptions = new() { SuppressExceptionsForTeamOnlyApiKeyErrors = false };

    [Fact]
    public void SendEmail_Success()
    {
        var email = "test@test.com";
        var templateId = "test-template-id";
        var values = new Dictionary<string, dynamic> { { "test-key", "test-value" }, { "test-key-2", 2 } };

        var notificationClient = new Mock<INotificationClient>(MockBehavior.Strict);

        notificationClient
            .Setup(s => s.SendEmail(email, templateId, values, null, null, null))
            .Returns(new EmailNotificationResponse());

        var service = BuildService(notificationClient: notificationClient.Object);

        service.SendEmail(email: email, templateId: templateId, values: values);

        notificationClient.Verify(s => s.SendEmail(email, templateId, values, null, null, null), Times.Once);
    }

    [Fact]
    public void SendEmail_GenericNotifyClientExceptionRaised_RethrowsException()
    {
        var email = "test@test.com";
        var templateId = "test-template-id";
        var values = new Dictionary<string, dynamic> { { "test-key", "test-value" }, { "test-key-2", 2 } };

        var notificationClient = new Mock<INotificationClient>(MockBehavior.Strict);

        var originalException = new NotifyClientException("Generic error");

        notificationClient
            .Setup(s => s.SendEmail(email, templateId, values, null, null, null))
            .Throws(originalException);

        var service = BuildService(notificationClient: notificationClient.Object);

        var exception = Assert.Throws<NotifyClientException>(() =>
            service.SendEmail(email: email, templateId: templateId, values: values)
        );

        notificationClient.Verify(s => s.SendEmail(email, templateId, values, null, null, null), Times.Once);

        Assert.Equal(originalException, exception);
    }

    [Fact]
    public void SendEmail_TestApiNotifyClientExceptionRaised_SuppressionDisabled_RethrowsException()
    {
        var email = "test@test.com";
        var templateId = "test-template-id";
        var values = new Dictionary<string, dynamic> { { "test-key", "test-value" }, { "test-key-2", 2 } };

        var notificationClient = new Mock<INotificationClient>(MockBehavior.Strict);

        var originalException = new NotifyClientException(TestApiKeyErrorMessage);

        notificationClient
            .Setup(s => s.SendEmail(email, templateId, values, null, null, null))
            .Throws(originalException);

        var service = BuildService(notificationClient: notificationClient.Object);

        var exception = Assert.Throws<NotifyClientException>(() =>
            service.SendEmail(email: email, templateId: templateId, values: values)
        );

        notificationClient.Verify(s => s.SendEmail(email, templateId, values, null, null, null), Times.Once);

        Assert.Equal(originalException, exception);
    }

    [Fact]
    public void SendEmail_TestApiNotifyClientExceptionRaised_SuppressionEnabled_SuppressesException()
    {
        var email = "test@test.com";
        var templateId = "test-template-id";
        var values = new Dictionary<string, dynamic> { { "test-key", "test-value" }, { "test-key-2", 2 } };

        var notificationClient = new Mock<INotificationClient>(MockBehavior.Strict);

        var originalException = new NotifyClientException(TestApiKeyErrorMessage);

        notificationClient
            .Setup(s => s.SendEmail(email, templateId, values, null, null, null))
            .Throws(originalException);

        var logger = new Mock<ILogger<EmailService>>(MockBehavior.Strict);

        ExpectLogMessage(
            logger,
            LogLevel.Information,
            """Email could not be sent to "test@test.com" as they are not a valid recipient for this team-only API key."""
        );

        var service = BuildService(
            notificationClient: notificationClient.Object,
            options: new AppOptions { SuppressExceptionsForTeamOnlyApiKeyErrors = true },
            logger: logger.Object
        );

        service.SendEmail(email: email, templateId: templateId, values: values);

        notificationClient.Verify(s => s.SendEmail(email, templateId, values, null, null, null), Times.Once);

        VerifyAllMocks(logger);
    }

    private static EmailService BuildService(
        INotificationClient? notificationClient = null,
        AppOptions? options = null,
        ILogger<EmailService>? logger = null
    )
    {
        return new EmailService(
            notificationClient: notificationClient ?? Mock.Of<INotificationClient>(MockBehavior.Strict),
            appOptions: (options ?? DefaultAppOptions).ToOptionsWrapper(),
            logger: logger ?? Mock.Of<ILogger<EmailService>>()
        );
    }
}
