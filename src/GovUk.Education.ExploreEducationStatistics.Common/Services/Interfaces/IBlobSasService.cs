using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IBlobSasService
{
    /// <summary>
    /// Generate a secure, short-lived SAS token with which a secure download
    /// can be requested.  These tokens should only be obtained after any relevant
    /// permission checks have been carried out.
    /// </summary>
    Task<Either<ActionResult, BlobDownloadToken>> CreateBlobDownloadToken(
        BlobServiceClient blobServiceClient,
        IBlobContainer container,
        string filename,
        string path,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Trade in a secure, short-lived SAS token in order to generate a secure
    /// BlobClient that can validate the SAS token and access the requested
    /// resource.
    /// </summary>
    Task<Either<ActionResult, BlobClient>> CreateSecureBlobClient(
        BlobServiceClient blobServiceClient,
        BlobDownloadToken token
    );

    /// <summary>
    /// Generate a short-lived, read-only SAS URI for a container, with which another storage account can read the
    /// container's blobs, e.g. as the source of a server-side (storage account to storage account) copy.
    /// </summary>
    Task<Uri> CreateContainerReadSasUri(
        BlobServiceClient blobServiceClient,
        BlobContainerClient containerClient,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken = default
    );
}
