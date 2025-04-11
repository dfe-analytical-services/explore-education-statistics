using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Events;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.EventGrid;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class AdminEventRaiserTests(ITestOutputHelper output)
{
    private readonly EventRaiserMockBuilder _eventRaiserMockBuilder = new();

    private IAdminEventRaiser GetSut() =>
        new AdminEventRaiser(_eventRaiserMockBuilder.Build());

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenTopicConfigured_WhenOnThemeUpdated_ThenEventPublished()
    {
        // ARRANGE
        var sut = GetSut();
        var theme = new ThemeBuilder().Build();

        // ACT
        await sut.OnThemeUpdated(theme);

        // ASSERT
        var expectedEvent = new ThemeChangedEvent(theme);
        _eventRaiserMockBuilder.Assert.EventRaised(expectedEvent);
    }
}
