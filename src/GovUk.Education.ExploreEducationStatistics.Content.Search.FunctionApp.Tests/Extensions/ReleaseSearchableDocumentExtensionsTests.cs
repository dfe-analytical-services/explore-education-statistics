using System.Text.Json;
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
            PublishingOrganisations =
            [
                new PublishingOrganisation
                {
                    Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                    Title = "Department for Education",
                },
                new PublishingOrganisation { Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"), Title = "Ofsted" },
            ],
            HtmlContent = "<p>This is some Html Content</p>",
        };

        // ACT
        var actual = releaseSearchableDocument.BuildMetadata();

        // ASSERT
        Assert.Equal(14, actual.Keys.Count);
        AssertAll([
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseId, "76640d46-3f02-4b08-a4d9-c1fbf1bdd502"),
            AssertMetadata(
                SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId,
                "5cd3ae70-ff32-409b-aa6b-363b380eb4c8"
            ),
            AssertMetadata(
                SearchableDocumentAzureBlobMetadataKeys.PublicationId,
                "caf751b8-5f8c-4526-8b5f-7fd28199866b"
            ),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ThemeId, "4625ca38-68aa-4d73-a1f9-2aab732aecc2"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Published, "2025-02-21T08:24:01Z"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseType, "Official Statistics"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.TypeBoost, "10"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, "publication-slug"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, "release-slug"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.Summary, "This is a summary."),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.ThemeTitle, "Theme Title"),
            AssertEncodedMetadata(SearchableDocumentAzureBlobMetadataKeys.Title, "Publication Title"),
            AssertJsonArrayMetadata(
                SearchableDocumentAzureBlobMetadataKeys.PublishingOrganisationIds,
                "7cbcfe03-9f7e-478a-8512-a1a5e0ca793b",
                "7c3252c6-6e94-4a34-8762-ff52aae5c0c0"
            ),
            AssertJsonArrayMetadata(
                SearchableDocumentAzureBlobMetadataKeys.PublishingOrganisationTitles,
                "Department for Education",
                "Ofsted"
            ),
        ]);

        Action AssertMetadata(string key, string value) => () => Assert.Equal(value, actual[key]);
        Action AssertEncodedMetadata(string key, string value) => () => AssertEncodedMetadataValue(value, actual[key]);
        Action AssertJsonArrayMetadata(string key, params string[] expectedValues) =>
            () => Assert.Equal(expectedValues, JsonSerializer.Deserialize<string[]>(actual[key]));
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

        AssertEncodedMetadataValue(
            "extra spaces either side",
            actual[SearchableDocumentAzureBlobMetadataKeys.ThemeTitle]
        );
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

    [Fact]
    public void GivenPublishingOrganisationTitlesWithUnicode_WhenBuildingMetadata_ThenJsonMetadataIsAsciiAndPreservesValues()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithPublishingOrganisations(
                new PublishingOrganisation { Id = Guid.NewGuid(), Title = "Department for Education" },
                new PublishingOrganisation { Id = Guid.NewGuid(), Title = "Y Grŵp Addysg, Diwylliant a’r Gymraeg" } // Welsh for "The Education, Culture and Welsh Language Group", containing Unicode characters 'Latin small w with circumflex' (U+0175) and 'Right Single Quotation Mark' (U+2019)
            )
            .Build();

        var actual = releaseSearchViewModel.BuildMetadata();
        var titlesJson = actual[SearchableDocumentAzureBlobMetadataKeys.PublishingOrganisationTitles];

        // Metadata key/value pairs are set using HTTP headers and must be valid headers containing only ASCII characters.
        // Verify that JsonSerializer.Serialize turns the non-ASCII characters into Unicode escape sequences like \u0175 and \u2019.
        Assert.True(titlesJson.All(ch => ch <= 127));

        // It's not possible to test the values will be preserved when indexed by Azure AI Search as that uses its built-in
        // `jsonArrayToStringCollection` mapping function. Instead, verify JsonSerializer.Deserialize<string[]> preserves
        // the values when deserialising the JSON back into a string array.
        Assert.Equal(
            releaseSearchViewModel
                .PublishingOrganisations.Select(publishingOrganisation => publishingOrganisation.Title)
                .ToArray(),
            JsonSerializer.Deserialize<string[]>(titlesJson)
        );
    }

    private static void AssertAll(params IEnumerable<Action>[] assertions) =>
        Assert.All(assertions.SelectMany(a => a), assertion => assertion());

    private static void AssertEncodedMetadataValue(string expectedDecodedValue, string actualEncodedValue) =>
        Assert.Equal(expectedDecodedValue, actualEncodedValue.FromBase64String());
}
