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
    /// Generate a short-lived, read-only SAS URI for a single blob, with which it can be read without any other
    /// credentials, e.g. by another storage account as the source of a server-side copy.
    /// </summary>
    Task<Uri> CreateBlobReadSasUri(
        BlobServiceClient blobServiceClient,
        BlobClient blobClient,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken = default
    );
}
