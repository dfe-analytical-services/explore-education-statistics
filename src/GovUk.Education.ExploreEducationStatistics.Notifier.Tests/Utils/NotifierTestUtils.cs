using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Utils;

public class NotifierTestUtils
{
    private const string notificationKey =
        "NOTIFYKEY-7ecfaa0c-d58b-43ca-a2f3-5d83c4124138-55dd5c51-3887-4941-85d5-0c1001f891b0";

    public static NotificationClient MockNotificationClient(HttpStatusCode responseCode, string responseContent)
    {
        var httpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage
            {
                StatusCode = responseCode,
                Content = new StringContent(responseContent),
            }));

        var httpClientWrapper = new HttpClientWrapper(new HttpClient(httpHandler.Object));

        return new NotificationClient(httpClientWrapper, notificationKey);
    }
}
