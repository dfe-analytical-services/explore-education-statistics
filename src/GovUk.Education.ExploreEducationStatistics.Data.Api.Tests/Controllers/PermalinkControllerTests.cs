#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

public class PermalinkControllerTests : IntegrationTest<TestStartup>
{
    public PermalinkControllerTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
    {
    }

    [Fact]
    public async Task CreatePermalink()
    {
        var createRequest = new PermalinkCreateRequest();
        var expectedResult = new PermalinkViewModel
        {
            Id = Guid.NewGuid()
        };

        var permalinkService = new Mock<IPermalinkService>(MockBehavior.Strict);

        permalinkService
            .Setup(s => s.CreatePermalink(It.Is<PermalinkCreateRequest>(r => r
                    .IsDeepEqualTo(createRequest, null)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.PostAsync(
            requestUri: "/api/permalink",
            content: new JsonNetContent(createRequest));

        MockUtils.VerifyAllMocks(permalinkService);

        response.AssertCreated(expectedResult, $"http://localhost/api/permalink/{expectedResult.Id}");
    }

    [Fact]
    public async Task GetPermalink()
    {
        var permalinkId = Guid.NewGuid();
        var permalink = new PermalinkViewModel
        {
            Id = permalinkId
        };

        var permalinkService = new Mock<IPermalinkService>(MockBehavior.Strict);

        permalinkService
            .Setup(s => s.GetPermalink(permalinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permalink);

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.GetAsync(
            uri: $"/api/permalink/{permalinkId}",
            headers: new Dictionary<string, string>
            {
                { HeaderNames.Accept, "application/json" }
            }
        );

        MockUtils.VerifyAllMocks(permalinkService);

        response.AssertOk(permalink);
    }

    [Fact]
    public async Task GetPermalink_NotFound()
    {
        var permalinkId = Guid.NewGuid();

        var permalinkService = new Mock<IPermalinkService>(MockBehavior.Strict);

        permalinkService
            .Setup(s => s.GetPermalink(permalinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.GetAsync(
            uri: $"/api/permalink/{permalinkId}",
            headers: new Dictionary<string, string>
            {
                { HeaderNames.Accept, "application/json" }
            }
        );

        MockUtils.VerifyAllMocks(permalinkService);

        response.AssertNotFound();
    }

    [Fact]
    public async Task GetPermalink_Csv()
    {
        var permalinkId = Guid.NewGuid();
        var permalinkService = new Mock<IPermalinkService>(MockBehavior.Strict);

        permalinkService
            .Setup(s => s
                .DownloadCsvToStream(permalinkId, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance)
            .Callback<Guid, Stream, CancellationToken>(
                (_, stream, _) => { stream.WriteText("Test csv"); }
            );

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.GetAsync(
            uri: $"/api/permalink/{permalinkId}",
            headers: new Dictionary<string, string>
            {
                { HeaderNames.Accept, ContentTypes.Csv }
            }
        );

        MockUtils.VerifyAllMocks(permalinkService);

        response.AssertOk("Test csv");
    }

    [Fact]
    public async Task GetPermalink_Csv_NotFound()
    {
        var permalinkId = Guid.NewGuid();

        var permalinkService = new Mock<IPermalinkService>(MockBehavior.Strict);

        permalinkService
            .Setup(s => s
                .DownloadCsvToStream(permalinkId, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.GetAsync(
            uri: $"/api/permalink/{permalinkId}",
            headers: new Dictionary<string, string>
            {
                { HeaderNames.Accept, ContentTypes.Csv }
            }
        );

        MockUtils.VerifyAllMocks(permalinkService);

        response.AssertNotFound();
    }

    [Fact]
    public async Task GetPermalink_InvalidIdReturnsNotFound()
    {
        var client = SetupApp().CreateClient();

        var response = await client.GetAsync("/api/permalink/not-a-guid");

        response.AssertNotFound();
    }

    private WebApplicationFactory<TestStartup> SetupApp(IPermalinkService? permalinkService = null)
    {
        return _testApp.ConfigureServices(
            services =>
            {
                services.AddTransient(_ => permalinkService ?? Mock.Of<IPermalinkService>(MockBehavior.Strict));
            }
        );
    }
}
