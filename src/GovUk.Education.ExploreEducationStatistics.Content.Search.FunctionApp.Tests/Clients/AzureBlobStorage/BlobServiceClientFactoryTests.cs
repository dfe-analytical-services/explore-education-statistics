using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.AzureBlobStorage;

public class BlobServiceClientFactoryTests
{
    private readonly DefaultAzureCredential _credential = new();

    [Theory]
    [InlineData(
        "DefaultEndpointsProtocol=https;AccountName=testaccount;AccountKey=dGVzdGtleQ==;EndpointSuffix=core.windows.net"
    )]
    [InlineData("accountname=testaccount;accountkey=dGVzdGtleQ==")]
    public void GivenAConnectionStringWithAnAccountKey_WhenCreated_ThenShouldUseTheKey(string connectionString)
    {
        var client = BlobServiceClientFactory.Create(connectionString, _credential);

        Assert.Equal("testaccount", client.AccountName);
        Assert.True(client.CanGenerateAccountSasUri);
    }

    [Fact]
    public void GivenDevelopmentStorage_WhenCreated_ThenShouldUseTheEmulatorKey()
    {
        var client = BlobServiceClientFactory.Create("UseDevelopmentStorage=true", _credential);

        Assert.Equal("devstoreaccount1", client.AccountName);
        Assert.True(client.CanGenerateAccountSasUri);
    }

    [Theory]
    [InlineData("AccountName=testaccount;", "https://testaccount.blob.core.windows.net/")]
    [InlineData(
        "DefaultEndpointsProtocol=https;AccountName=testaccount;EndpointSuffix=core.example.net",
        "https://testaccount.blob.core.example.net/"
    )]
    [InlineData("AccountName=testaccount;BlobEndpoint=https://custom.example.net/", "https://custom.example.net/")]
    public void GivenAKeylessConnectionString_WhenCreated_ThenShouldUseTheCredential(
        string connectionString,
        string expectedUri
    )
    {
        var client = BlobServiceClientFactory.Create(connectionString, _credential);

        Assert.Equal(new Uri(expectedUri), client.Uri);
        Assert.False(client.CanGenerateAccountSasUri);
    }

    [Fact]
    public void GivenAValueThatIsNotAConnectionString_WhenCreated_ThenShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => BlobServiceClientFactory.Create("not a connection string", _credential));
    }

    [Fact]
    public void GivenAConnectionStringWithNoAccountNameOrEndpoint_WhenCreated_ThenShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            BlobServiceClientFactory.Create("QueueEndpoint=https://custom.example.net/", _credential)
        );
    }
}
