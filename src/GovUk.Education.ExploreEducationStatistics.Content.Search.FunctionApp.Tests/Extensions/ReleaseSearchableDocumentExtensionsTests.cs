﻿using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
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
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseSlug, "release-slug"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseVersionId, "5cd3ae70-ff32-409b-aa6b-363b380eb4c8"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.PublicationId, "caf751b8-5f8c-4526-8b5f-7fd28199866b"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.PublicationSlug, "publication-slug"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ThemeId, "4625ca38-68aa-4d73-a1f9-2aab732aecc2"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ThemeTitle, "Theme Title"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Published, "2025-02-21T08:24:01Z"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Summary, "This is a summary."),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.Title, "Publication Title"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.ReleaseType, "Official Statistics"),
            AssertMetadata(SearchableDocumentAzureBlobMetadataKeys.TypeBoost, "10"),
            () => Assert.Equal(12, actual.Keys.Count) // Ensure there aren't any extra items in the metadata
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
