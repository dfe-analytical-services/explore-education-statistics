using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.ViewModels;

public class AllMethodologiesThemeViewModelTests
{
    private static readonly MethodologyVersionSummaryViewModel _methodology = new()
    {
        Id = Guid.NewGuid(),
        Slug = "methodology-slug",
        Title = "Methodology title",
    };

    [Fact]
    public void RemovePublicationNodesWithoutMethodologiesAndSort_RemovesPublicationsWithoutMethodologies()
    {
        var model = new AllMethodologiesThemeViewModel
        {
            Title = "ThemeWithPublications",
            Publications =
            [
                new AllMethodologiesPublicationViewModel
                {
                    Title = "PublicationWithoutMethodology",
                    Methodologies = [],
                },
                new AllMethodologiesPublicationViewModel
                {
                    Title = "PublicationWithMethodology",
                    Methodologies = [_methodology],
                },
            ],
        };

        model.RemovePublicationNodesWithoutMethodologiesAndSort();

        Assert.Single(model.Publications);
        Assert.Equal("PublicationWithMethodology", model.Publications[0].Title);
    }

    [Fact]
    public void RemovePublicationNodesWithoutMethodologiesAndSort_SortsPublicationsByTitle()
    {
        var model = new AllMethodologiesThemeViewModel
        {
            Title = "ThemeWithPublications",
            Publications =
            [
                new AllMethodologiesPublicationViewModel { Title = "Publication C", Methodologies = [_methodology] },
                new AllMethodologiesPublicationViewModel { Title = "Publication A", Methodologies = [_methodology] },
                new AllMethodologiesPublicationViewModel { Title = "Publication B", Methodologies = [_methodology] },
            ],
        };

        model.RemovePublicationNodesWithoutMethodologiesAndSort();

        Assert.Equal(3, model.Publications.Count);
        Assert.Equal("Publication A", model.Publications[0].Title);
        Assert.Equal("Publication B", model.Publications[1].Title);
        Assert.Equal("Publication C", model.Publications[2].Title);
    }

    [Fact]
    public void RemovePublicationNodesWithoutMethodologiesAndSort_HandlesEmptyPublications()
    {
        var model = new AllMethodologiesThemeViewModel { Title = "ThemeWithoutPublications", Publications = [] };

        model.RemovePublicationNodesWithoutMethodologiesAndSort();

        Assert.Empty(model.Publications);
    }
}
