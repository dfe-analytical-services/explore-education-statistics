using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Extensions;

public class ReleaseSearchableDocumentExtensionsTests
{
    [Fact]
    public void BuildMetadataShouldReturnExpectedMetadataKeysAndValues()
    {
        var releaseSearchableDocument = new ReleaseSearchableDocument
        {
            ReleaseId = new Guid("76640d46-3f02-4b08-a4d9-c1fbf1bdd502"),
            ReleaseVersionId = new Guid("5cd3ae70-ff32-409b-aa6b-363b380eb4c8"),
            Published = new DateTimeOffset(2025, 02, 21, 09, 24, 01, TimeSpan.FromHours(1)),
            PublicationId = new Guid("caf751b8-5f8c-4526-8b5f-7fd28199866b"),
            PublicationTitle = "Publication Title",
            ThemeId = new Guid("4625ca38-68aa-4d73-a1f9-2aab732aecc2"),
            ThemeTitle = "Theme Title",
            Summary = "This is a summary.",
            ReleaseType = "Official Statistics",
            TypeBoost = 10,
            PublicationSlug = "publication-slug",
            ReleaseSlug = "release-slug",
            HtmlContent = "<p>This is some Html Content</p>",
        };

        // ACT
        var actual = releaseSearchableDocument.BuildMetadata();

        // ASSERT
        AssertAll(
        [
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseId, "76640d46-3f02-4b08-a4d9-c1fbf1bdd502"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId, "5cd3ae70-ff32-409b-aa6b-363b380eb4c8"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.PublicationId, "caf751b8-5f8c-4526-8b5f-7fd28199866b"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ThemeId, "4625ca38-68aa-4d73-a1f9-2aab732aecc2"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Published, "2025-02-21T08:24:01Z"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseType, "Official Statistics"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.TypeBoost, "10"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, "publication-slug"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, "release-slug"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.Summary, "This is a summary."),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.ThemeTitle, "Theme Title"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.Title, "Publication Title"),
            () => Assert.Equal(12, actual.Keys.Count) // Ensure there aren't any extra items in the metadata
        ]);

        Action AssertMetadata(string key, string value) => () => Assert.Equal(value, actual[key]);
        Action AssertEncodedMetadata(string key, string value) => () => AssertEncodedMetadataValue(value, actual[key]);
    }

    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore, we need to trim them. 
    /// </summary>
    [Fact]
    public void GivenSummaryWithExtraSpaces_WhenBuildingMetadata_ThenSummaryIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithSummary("  extra spaces either side ")
            .Build();

        var actual = releaseSearchViewModel.BuildMetadata();

        AssertEncodedMetadataValue("extra spaces either side", actual[SearchableDocumentAzureBlobMetadataKeys.Summary]);
    }

    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore, we need to trim them. 
    /// </summary>
    [Fact]
    public void GivenThemeTitleWithExtraSpaces_WhenBuildingMetadata_ThenThemeTitleIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithThemeTitle("  extra spaces either side ")
            .Build();

        var actual = releaseSearchViewModel.BuildMetadata();

        AssertEncodedMetadataValue("extra spaces either side",
            actual[SearchableDocumentAzureBlobMetadataKeys.ThemeTitle]);
    }

    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore, we need to trim them. 
    /// </summary>
    [Fact]
    public void GivenTitleWithExtraSpaces_WhenBuildingMetadata_ThenTitleIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithTitle("  extra spaces either side ")
            .Build();

        var actual = releaseSearchViewModel.BuildMetadata();

        AssertEncodedMetadataValue("extra spaces either side", actual[SearchableDocumentAzureBlobMetadataKeys.Title]);
    }

    private static void AssertAll(params IEnumerable<Action>[] assertions) =>
        Assert.All(assertions.SelectMany(a => a), assertion => assertion());

    private static void AssertEncodedMetadataValue(string expectedDecodedValue, string actualEncodedValue) =>
        Assert.Equal(expectedDecodedValue, actualEncodedValue.FromBase64String());
}
