using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.AzureBlobStorage;

public class BlobTests(ITestOutputHelper output)
{
    [Fact]
    public void GivenABlobWithMetadata_WhenConvertedToAString_ThenShouldRenderAllMetadataItems()
    {
        // ARRANGE
        var sut = new Blob(
            "These are the contents of the blob.",
            new Dictionary<string, string>
            {
                { "key1", "value1" }, 
                { "key2", "value2" }, 
                { "key3", "value3" }
            });
        
        // ACT
        var actual = sut.ToString();
        
        // ASSERT
        output.WriteLine(actual);
        
        Assert.NotNull(actual);
        Assert.Contains("These are the contents of the blob.", actual);
        Assert.Contains("key1", actual);
        Assert.Contains("key2", actual);
        Assert.Contains("key3", actual);
        Assert.Contains("value1", actual);
        Assert.Contains("value2", actual);
        Assert.Contains("value3", actual);
    }
    
    [Fact]
    public void GivenABlobWithEmptyMetadata_WhenConvertedToAString_ThenMetadataShouldRenderAsEmpty()
    {
        // ARRANGE
        var sut = new Blob(
            "These are the contents of the blob.",
            new Dictionary<string, string>());
        
        // ACT
        var actual = sut.ToString();
        
        // ASSERT
        output.WriteLine(actual);
        
        Assert.NotNull(actual);
        Assert.Contains("<empty>", actual);
    }
    
    [Fact]
    public void GivenABlobWithNullMetadata_WhenConvertedToAString_ThenMetadataShouldRenderAsNull()
    {
        // ARRANGE
        var sut = new Blob(
            "These are the contents of the blob.",
            Metadata: null);
        
        // ACT
        var actual = sut.ToString();
        
        // ASSERT
        output.WriteLine(actual);
        
        Assert.NotNull(actual);
        Assert.Contains("<null>", actual);
    }
}
