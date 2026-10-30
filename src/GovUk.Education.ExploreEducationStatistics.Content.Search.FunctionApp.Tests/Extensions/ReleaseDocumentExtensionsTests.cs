using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Extensions;

public class ReleaseDocumentExtensionsTests
{
    [Fact]
    public void BuildMetadataShouldReturnExpectedMetadataKeysAndValues()
    {
        var releaseDocument = new ReleaseDocument
        {
            ReleaseId = new Guid("76640d46-3f02-4b08-a4d9-c1fbf1bdd502"),
            ReleaseVersionId = new Guid("5cd3ae70-ff32-409b-aa6b-363b380eb4c8"),
            Published = new DateTimeOffset(2025, 02, 21, 19, 24, 01, TimeSpan.FromHours(1)),
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
        var actual = releaseDocument.BuildMetadata();

        // ASSERT
        Assert.Equal(14, actual.Keys.Count);
        AssertAll([
            AssertMetadata(ReleaseDocumentAzureBlobMetadataKeys.ReleaseId, "76640d46-3f02-4b08-a4d9-c1fbf1bdd502"),
            AssertMetadata(
                ReleaseDocumentAzureBlobMetadataKeys.ReleaseVersionId,
                "5cd3ae70-ff32-409b-aa6b-363b380eb4c8"
            ),
            AssertMetadata(ReleaseDocumentAzureBlobMetadataKeys.PublicationId, "caf751b8-5f8c-4526-8b5f-7fd28199866b"),
            AssertMetadata(ReleaseDocumentAzureBlobMetadataKeys.ThemeId, "4625ca38-68aa-4d73-a1f9-2aab732aecc2"),
            AssertMetadata(ReleaseDocumentAzureBlobMetadataKeys.Published, "2025-02-21T18:24:01Z"),
            AssertMetadata(ReleaseDocumentAzureBlobMetadataKeys.ReleaseType, "Official Statistics"),
            AssertMetadata(ReleaseDocumentAzureBlobMetadataKeys.TypeBoost, "10"),
            AssertEncodedMetadata(ReleaseDocumentAzureBlobMetadataKeys.PublicationSlug, "publication-slug"),
            AssertEncodedMetadata(ReleaseDocumentAzureBlobMetadataKeys.ReleaseSlug, "release-slug"),
            AssertEncodedMetadata(ReleaseDocumentAzureBlobMetadataKeys.Summary, "This is a summary."),
            AssertEncodedMetadata(ReleaseDocumentAzureBlobMetadataKeys.ThemeTitle, "Theme Title"),
            AssertEncodedMetadata(ReleaseDocumentAzureBlobMetadataKeys.Title, "Publication Title"),
            AssertJsonArrayMetadata(
                ReleaseDocumentAzureBlobMetadataKeys.PublishingOrganisationIds,
                "7cbcfe03-9f7e-478a-8512-a1a5e0ca793b",
                "7c3252c6-6e94-4a34-8762-ff52aae5c0c0"
            ),
            AssertJsonArrayMetadata(
                ReleaseDocumentAzureBlobMetadataKeys.PublishingOrganisationTitles,
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
        var releaseSearchViewModel = new ReleaseDocumentBuilder().WithSummary("  extra spaces either side ").Build();

        var actual = releaseSearchViewModel.BuildMetadata();

        AssertEncodedMetadataValue("extra spaces either side", actual[ReleaseDocumentAzureBlobMetadataKeys.Summary]);
    }

    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore, we need to trim them.
    /// </summary>
    [Fact]
    public void GivenThemeTitleWithExtraSpaces_WhenBuildingMetadata_ThenThemeTitleIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseDocumentBuilder().WithThemeTitle("  extra spaces either side ").Build();

        var actual = releaseSearchViewModel.BuildMetadata();

        AssertEncodedMetadataValue("extra spaces either side", actual[ReleaseDocumentAzureBlobMetadataKeys.ThemeTitle]);
    }

    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore, we need to trim them.
    /// </summary>
    [Fact]
    public void GivenTitleWithExtraSpaces_WhenBuildingMetadata_ThenTitleIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseDocumentBuilder().WithTitle("  extra spaces either side ").Build();

        var actual = releaseSearchViewModel.BuildMetadata();

        AssertEncodedMetadataValue("extra spaces either side", actual[ReleaseDocumentAzureBlobMetadataKeys.Title]);
    }

    [Fact]
    public void GivenPublishingOrganisationTitlesWithUnicode_WhenBuildingMetadata_ThenJsonMetadataIsAsciiAndPreservesValues()
    {
        var releaseSearchViewModel = new ReleaseDocumentBuilder()
            .WithPublishingOrganisations(
                new PublishingOrganisation { Id = Guid.NewGuid(), Title = "Department for Education" },
                new PublishingOrganisation { Id = Guid.NewGuid(), Title = "Y Grŵp Addysg, Diwylliant a’r Gymraeg" } // Welsh for "The Education, Culture and Welsh Language Group", containing Unicode characters 'Latin small w with circumflex' (U+0175) and 'Right Single Quotation Mark' (U+2019)
            )
            .Build();

        var actual = releaseSearchViewModel.BuildMetadata();
        var titlesJson = actual[ReleaseDocumentAzureBlobMetadataKeys.PublishingOrganisationTitles];

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
