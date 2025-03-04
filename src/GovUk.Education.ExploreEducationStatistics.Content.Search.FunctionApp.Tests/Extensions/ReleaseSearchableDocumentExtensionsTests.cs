using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
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
            ReleaseVersionId = new Guid("12345678-1234-1234-1234-123456789abc"),
            Published = new DateTimeOffset(2025, 02, 21, 09, 24, 01, TimeSpan.FromHours(1)),
            PublicationTitle = "Publication Title",
            Summary = "This is a summary.",
            Theme = "Theme",
            Type = "Official Statistics",
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
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId, "12345678-1234-1234-1234-123456789abc"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, "publication-slug"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, "release-slug"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Published, "2025-02-21T08:24:01Z"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Summary, "This is a summary."),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Title, "Publication Title"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Theme, "Theme"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Type, "Official Statistics"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.TypeBoost, "10"),
            () => Assert.Equal(9, actual.Keys.Count) // Ensure there aren't any extra items in the metadata
        ]);
        
        Action AssertMetadata(string key, string value) => () => Assert.Equal(value, actual[key]);
    }

    /// <summary>
    /// Azure metadata does not accept unicode characters. Therefore we need to encode them. 
    /// </summary>
    [Fact]
    public void GivenSummaryWithUnicodeCharacters_WhenBuildingMetadata_ThenSummaryContainsOnlyAsciiCharacters()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithSummary("Right single quotation mark = ’")
            .Build();
        
        var actual = releaseSearchViewModel.BuildMetadata();
        
        Assert.Equal("Right single quotation mark = \\u2019", actual[SearchableDocumentAzureBlobMetadataKeys.Summary]);
    }
    
    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore we need to trim them. 
    /// </summary>
    [Fact]
    public void GivenSummaryWithExtraSpaces_WhenBuildingMetadata_ThenSummaryIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithSummary("  extra spaces either side ")
            .Build();
        
        var actual = releaseSearchViewModel.BuildMetadata();
        
        Assert.Equal("extra spaces either side", actual[SearchableDocumentAzureBlobMetadataKeys.Summary]);
    }
    
    /// <summary>
    /// Azure metadata does not accept unicode characters. Therefore we need to encode them. 
    /// </summary>
    [Fact]
    public void GivenTitleWithUnicodeCharacters_WhenBuildingMetadata_ThenTitleContainsOnlyAsciiCharacters()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithTitle("Right single quotation mark = ’")
            .Build();
        
        var actual = releaseSearchViewModel.BuildMetadata();
        
        Assert.Equal("Right single quotation mark = \\u2019", actual[SearchableDocumentAzureBlobMetadataKeys.Title]);
    }
    
    /// <summary>
    /// Azure metadata does not accept preceding or trailing spaces. Therefore we need to trim them. 
    /// </summary>
    [Fact]
    public void GivenTitleWithExtraSpaces_WhenBuildingMetadata_ThenTitleIsTrimmed()
    {
        var releaseSearchViewModel = new ReleaseSearchableDocumentBuilder()
            .WithTitle("  extra spaces either side ")
            .Build();
        
        var actual = releaseSearchViewModel.BuildMetadata();
        
        Assert.Equal("extra spaces either side", actual[SearchableDocumentAzureBlobMetadataKeys.Title]);
    }
    
    private void AssertAll(params IEnumerable<Action>[] assertions) => Assert.All(assertions.SelectMany(a => a), assertion => assertion());
}
