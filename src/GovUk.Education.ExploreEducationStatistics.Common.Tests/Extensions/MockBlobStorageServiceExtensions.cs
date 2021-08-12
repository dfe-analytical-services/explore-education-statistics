#nullable enable
using System.IO;
using System.Threading;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class MockBlobStorageServiceExtensions
    {
        public static IReturnsResult<IBlobStorageService> SetupFindBlob(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            BlobInfo? blob)
        {
            return service.Setup(s => s.FindBlob(container, path))
                .ReturnsAsync(blob);
        }

        public static IReturnsResult<IBlobStorageService> SetupCheckBlobExists(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            bool exists)
        {
            return service.Setup(s => s.CheckBlobExists(container, path))
                .ReturnsAsync(exists);
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadToStream(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            string blobText,
            CancellationToken? cancellationToken = null)
        {
            return service.Setup(
                    s =>
                        s.DownloadToStream(container, path, It.IsAny<Stream>(), cancellationToken)
                )
                .Callback<IBlobContainer, string, Stream, CancellationToken?>(
                    (_, _, stream, _) => stream.WriteText(blobText)
                )
                .ReturnsAsync(blobText.ToStream());
        }
    }
}