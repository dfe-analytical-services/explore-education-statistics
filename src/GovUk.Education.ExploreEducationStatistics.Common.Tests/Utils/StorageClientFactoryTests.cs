using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class StorageClientFactoryTests
{
    private const string AccountKeyConnectionString =
        "DefaultEndpointsProtocol=https;AccountName=testaccount;AccountKey=dGVzdGtleQ==;EndpointSuffix=core.windows.net";

    private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

    private const string SasConnectionString =
        "BlobEndpoint=https://testaccount.blob.core.windows.net/;SharedAccessSignature=sv=2022-11-02&sig=abc%3D";

    private const string KeylessConnectionString = "AccountName=testaccount;";

    public class UsesManagedIdentityTests : StorageClientFactoryTests
    {
        [Theory]
        [InlineData(AccountKeyConnectionString)]
        [InlineData(DevelopmentStorageConnectionString)]
        [InlineData(SasConnectionString)]
        [InlineData("accountname=testaccount;accountkey=dGVzdGtleQ==")]
        public void ConnectionStringWithCredentials_ReturnsFalse(string connectionString)
        {
            Assert.False(StorageClientFactory.UsesManagedIdentity(connectionString));
        }

        [Theory]
        [InlineData(KeylessConnectionString)]
        [InlineData("AccountName=testaccount")]
        [InlineData("DefaultEndpointsProtocol=https;AccountName=testaccount;EndpointSuffix=core.windows.net")]
        [InlineData("BlobEndpoint=https://testaccount.blob.core.windows.net/")]
        public void ConnectionStringWithoutCredentials_ReturnsTrue(string connectionString)
        {
            Assert.True(StorageClientFactory.UsesManagedIdentity(connectionString));
        }

        [Fact]
        public void NotAConnectionString_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => StorageClientFactory.UsesManagedIdentity("not a connection string"));
        }
    }

    public class GetServiceUriTests : StorageClientFactoryTests
    {
        [Theory]
        [InlineData("blob", "https://testaccount.blob.core.windows.net/")]
        [InlineData("queue", "https://testaccount.queue.core.windows.net/")]
        [InlineData("table", "https://testaccount.table.core.windows.net/")]
        public void AccountName_BuildsDefaultEndpoint(string service, string expectedUri)
        {
            Assert.Equal(new Uri(expectedUri), StorageClientFactory.GetServiceUri(KeylessConnectionString, service));
        }

        [Fact]
        public void ProtocolAndEndpointSuffix_AreUsed()
        {
            var uri = StorageClientFactory.GetServiceUri(
                "DefaultEndpointsProtocol=http;AccountName=testaccount;EndpointSuffix=core.example.net",
                "queue"
            );

            Assert.Equal(new Uri("http://testaccount.queue.core.example.net/"), uri);
        }

        [Fact]
        public void ExplicitEndpoint_TakesPrecedence()
        {
            var uri = StorageClientFactory.GetServiceUri(
                "AccountName=testaccount;TableEndpoint=https://custom.example.net/",
                "table"
            );

            Assert.Equal(new Uri("https://custom.example.net/"), uri);
        }

        [Fact]
        public void NoAccountNameOrEndpoint_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                StorageClientFactory.GetServiceUri("QueueEndpoint=https://custom.example.net/", "blob")
            );
        }
    }

    public class CreateClientTests : StorageClientFactoryTests
    {
        [Fact]
        public void KeylessConnectionString_CreatesIdentityClients()
        {
            var blobClient = StorageClientFactory.CreateBlobServiceClient(KeylessConnectionString);
            var queueClient = StorageClientFactory.CreateQueueServiceClient(KeylessConnectionString);
            var tableClient = StorageClientFactory.CreateTableServiceClient(KeylessConnectionString);

            Assert.Equal(new Uri("https://testaccount.blob.core.windows.net/"), blobClient.Uri);
            Assert.Equal(new Uri("https://testaccount.queue.core.windows.net/"), queueClient.Uri);
            Assert.Equal(new Uri("https://testaccount.table.core.windows.net/"), tableClient.Uri);

            // Clients authorised with a token credential can't sign SAS URIs with an account key.
            Assert.False(blobClient.CanGenerateAccountSasUri);
            Assert.False(queueClient.CanGenerateAccountSasUri);
        }

        [Fact]
        public void AccountKeyConnectionString_CreatesSharedKeyClients()
        {
            var blobClient = StorageClientFactory.CreateBlobServiceClient(AccountKeyConnectionString);
            var queueClient = StorageClientFactory.CreateQueueServiceClient(AccountKeyConnectionString);
            var tableClient = StorageClientFactory.CreateTableServiceClient(AccountKeyConnectionString);

            Assert.Equal("testaccount", blobClient.AccountName);
            Assert.Equal("testaccount", queueClient.AccountName);
            Assert.Equal("testaccount", tableClient.AccountName);

            Assert.True(blobClient.CanGenerateAccountSasUri);
            Assert.True(queueClient.CanGenerateAccountSasUri);
        }

        [Fact]
        public void CreateQueueClient_UsesQueueName()
        {
            var queueClient = StorageClientFactory.CreateQueueClient(KeylessConnectionString, "imports-pending");

            Assert.Equal("imports-pending", queueClient.Name);
            Assert.Equal(new Uri("https://testaccount.queue.core.windows.net/imports-pending"), queueClient.Uri);
        }
    }
}
