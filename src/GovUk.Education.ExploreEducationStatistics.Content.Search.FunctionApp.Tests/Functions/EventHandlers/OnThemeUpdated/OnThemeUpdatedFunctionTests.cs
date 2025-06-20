using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnThemeUpdated;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Functions.EventHandlers.OnThemeUpdated;

public class OnThemeUpdatedFunctionTests
{
    private readonly ContentApiClientMockBuilder _contentApiMockBuilder = new();

    private OnThemeUpdatedFunction GetSut() => new(
        new EventGridEventHandler(new NullLogger<EventGridEventHandler>()),
        _contentApiMockBuilder.Build(),
        new NullLogger<OnThemeUpdatedFunction>()
    );

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    public async Task GivenEvent_WhenThemeContainsNPublications_ThenNMessagesReturned(int numberOfPublications)
    {
        // ARRANGE
        var themeId = Guid.NewGuid();
        var eventDto = new EventGridEventBuilder()
            .WithSubject(themeId.ToString())
            .Build();
        var sut = GetSut();
        var publications = Enumerable.Range(0, numberOfPublications)
            .Select(i => new PublicationInfo
            {
                PublicationSlug = $"publication-slug-{i}",
                LatestReleaseSlug = $"release-slug-{i}"
            })
            .ToArray();
        
        _contentApiMockBuilder.WhereThemeHasPublications(publications);
        
        // ACT
        var response = await sut.OnThemeUpdated(eventDto, new FunctionContextMockBuilder().Build());

        // ASSERT
        _contentApiMockBuilder.Assert.PublicationsRequestedForThemeId(themeId);
        Assert.Equal(numberOfPublications, response.Length);
        Assert.Equal(publications.Select(p => p.PublicationSlug), response.Select(message => message.PublicationSlug));
    }
}
