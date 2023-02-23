#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

public class PermalinkControllerTests : IClassFixture<TestApplicationFactory<TestStartup>>
{
    private readonly WebApplicationFactory<TestStartup> _testApp;

    public PermalinkControllerTests(TestApplicationFactory<TestStartup> testApp)
    {
        _testApp = testApp;
    }

    [Fact]
    public async Task Get()
    {
        var id = Guid.NewGuid();

        var permalink = new LegacyPermalinkViewModel
        {
            Id = id,
        };

        var permalinkService = new Mock<IPermalinkService>(Strict);

        permalinkService
            .Setup(s => s.Get(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permalink);

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.GetAsync($"/api/permalink/{id}");

        VerifyAllMocks(permalinkService);

        response.AssertOk(permalink);
    }

    [Fact]
    public async Task Get_Csv()
    {
        var id = Guid.NewGuid();

        var permalinkService = new Mock<IPermalinkService>(Strict);

        permalinkService
            .Setup(s => s
                .DownloadCsvToStream(id, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance)
            .Callback<Guid, Stream, CancellationToken>(
                (_, stream, _) => { stream.WriteText("Test csv"); }
            );;

        var client = SetupApp(permalinkService: permalinkService.Object)
            .CreateClient();

        var response = await client.GetAsync(
            uri: $"/api/permalink/{id}",
            headers: new Dictionary<string, string>
            {
                { HeaderNames.Accept, "text/csv" }
            }
        );

        VerifyAllMocks(permalinkService);

        response.AssertOk("Test csv");
    }

    [Fact]
    public async Task Get_InvalidIdReturnsNotFound()
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
                services.AddTransient(_ => permalinkService ?? Mock.Of<IPermalinkService>(Strict));
            }
        );
    }
}
