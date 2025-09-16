#nullable enable
using System;
using System.Collections.Generic;
using Azure;
using Azure.Storage.Blobs;
using Moq;
using static Azure.Storage.Blobs.Models.BlobsModelFactory;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public static class BlobServiceTestUtils
{
    public static Mock<BlobClient> MockBlobClient(
        string name,
        DateTimeOffset createdOn = default,
        int contentLength = 0,
        string? contentType = null,
        IDictionary<string, string>? metadata = null,
        bool exists = true)
    {
        var blobClient = new Mock<BlobClient>(MockBehavior.Strict);

        blobClient.SetupGet(client => client.Name)
            .Returns(name);

        blobClient.Setup(client => client.ExistsAsync(default))
            .ReturnsAsync(Response.FromValue(exists, null!));

        var blobProperties = BlobProperties(
            createdOn: createdOn,
            contentLength: contentLength,
            contentType: contentType,
            metadata: metadata
        );

        blobClient.Setup(client => client.GetPropertiesAsync(default, default))
            .ReturnsAsync(Response.FromValue(blobProperties, null!));

        return blobClient;
    }

    public static Mock<BlobServiceClient> MockBlobServiceClient(
        params Mock<BlobContainerClient>[] blobContainerClients)
    {
        var blobServiceClient = new Mock<BlobServiceClient>(MockBehavior.Strict);

        blobServiceClient.Setup(s => s.Uri)
            .Returns(new Uri("https://data-storage:10001/devstoreaccount1;"));

        foreach (var blobContainerClient in blobContainerClients)
        {
            blobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerClient.Object.Name))
                .Returns(blobContainerClient.Object);
        }

        return blobServiceClient;
    }

    public static Mock<BlobContainerClient> MockBlobContainerClient(
        string containerName,
        params Mock<BlobClient>[] blobClients)
    {
        var blobContainerClient = new Mock<BlobContainerClient>(MockBehavior.Strict);

        blobContainerClient
            .SetupGet(client => client.Name)
            .Returns(containerName);

        foreach (var blobClient in blobClients)
        {
            blobContainerClient.Setup(client => client.GetBlobClient(blobClient.Object.Name))
                .Returns(blobClient.Object);
        }

        return blobContainerClient;
    }
}
