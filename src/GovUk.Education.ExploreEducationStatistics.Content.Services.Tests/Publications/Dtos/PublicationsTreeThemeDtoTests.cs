using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications.Dtos;

public class PublicationsTreeThemeDtoTests
{
    [Fact]
    public void WhenSerializingAndDeserializingArray_WithSupersededBy_PreservesData()
    {
        PublicationsTreeThemeDto[] tree =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Summary = "Theme summary",
                Title = "Theme title",
                Publications =
                [
                    new PublicationsTreePublicationDto
                    {
                        Id = Guid.NewGuid(),
                        Slug = "publication-slug",
                        Title = "Publication title",
                        SupersededBy = new PublicationsTreePublicationSupersededByPublicationDto
                        {
                            Id = Guid.NewGuid(),
                            Slug = "superseding-publication-slug",
                            Title = "Superseding publication title",
                        },
                        AnyLiveReleaseHasData = true,
                        LatestReleaseHasData = true,
                    },
                ],
            },
        ];

        var converted = JsonConvert.DeserializeObject<PublicationsTreeThemeDto[]>(JsonConvert.SerializeObject(tree));
        Assert.Equal(tree, converted);
    }

    [Fact]
    public void WhenSerializingAndDeserializingArray_WithNullSupersededBy_PreservesData()
    {
        PublicationsTreeThemeDto[] tree =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Summary = "Theme summary",
                Title = "Theme title",
                Publications =
                [
                    new PublicationsTreePublicationDto
                    {
                        Id = Guid.NewGuid(),
                        Slug = "publication-slug",
                        Title = "Publication title",
                        SupersededBy = null,
                        AnyLiveReleaseHasData = false,
                        LatestReleaseHasData = false,
                    },
                ],
            },
        ];

        var converted = JsonConvert.DeserializeObject<PublicationsTreeThemeDto[]>(JsonConvert.SerializeObject(tree));
        Assert.Equal(tree, converted);
    }
}
