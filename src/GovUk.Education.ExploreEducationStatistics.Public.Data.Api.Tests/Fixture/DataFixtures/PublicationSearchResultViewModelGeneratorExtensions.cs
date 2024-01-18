using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;

public static class PublicationSearchResultViewModelGeneratorExtensions
{
    public static Generator<PublicationSearchResultViewModel> DefaultDataSet(this DataFixture fixture)
        => fixture.Generator<PublicationSearchResultViewModel>().WithDefaults();

    public static Generator<PublicationSearchResultViewModel> WithDefaults(this Generator<PublicationSearchResultViewModel> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<PublicationSearchResultViewModel> SetDefaults(this InstanceSetters<PublicationSearchResultViewModel> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Title)
            .SetDefault(f => f.Slug)
            .SetDefault(f => f.Summary)
            .SetDefault(f => f.Theme)
            .Set(f => f.Published, f => f.Date.Past())
            .Set(f => f.Type, Content.Model.ReleaseType.OfficialStatistics);

    public static Generator<PublicationSearchResultViewModel> WithPublicationId(this Generator<PublicationSearchResultViewModel> generator, Guid publicationId)
    => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static InstanceSetters<PublicationSearchResultViewModel> SetPublicationId(
        this InstanceSetters<PublicationSearchResultViewModel> instanceSetter,
        Guid publicationId)
        => instanceSetter.Set(f => f.Id, publicationId);
}
