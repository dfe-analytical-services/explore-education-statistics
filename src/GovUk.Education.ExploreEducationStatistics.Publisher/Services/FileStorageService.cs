using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger _logger;

        private readonly string _privateStorageConnectionString;
        private readonly string _publicStorageConnectionString;

        private const string PrivateContainerName = "releases";
        private const string PublicContainerName = "downloads";

        public FileStorageService(IConfiguration config,
            ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _privateStorageConnectionString = config.GetConnectionString("PrivateStorage");
            _publicStorageConnectionString = config.GetConnectionString("PublicStorage");
        }

        public async void CopyReleaseToPublicContainer(string publication, string release)
        {
            var privateStorageAccount = CloudStorageAccount.Parse(_privateStorageConnectionString);
            var publicStorageAccount = CloudStorageAccount.Parse(_publicStorageConnectionString);

            var privateBlobClient = privateStorageAccount.CreateCloudBlobClient();
            var publicBlobClient = publicStorageAccount.CreateCloudBlobClient();

            var privateContainer = privateBlobClient.GetContainerReference(PrivateContainerName);
            var publicContainer = publicBlobClient.GetContainerReference(PublicContainerName);

            // TODO
            //var searchPattern = "^.+$(?<!\\.meta\\.csv)";
            const string searchPattern = "*.csv";

            var directoryAddress = $"{publication}/{release}";
            await CopyDirectoryAsync(directoryAddress, directoryAddress, searchPattern, privateContainer,
                publicContainer);
        }

        private async Task CopyDirectoryAsync(string sourceDirectoryAddress, string destinationDirectoryAddress,
            string searchPattern,
            CloudBlobContainer sourceContainer, CloudBlobContainer destinationContainer)
        {
            var sourceDirectory = sourceContainer.GetDirectoryReference(sourceDirectoryAddress);
            var destinationDirectory = destinationContainer.GetDirectoryReference(destinationDirectoryAddress);

            var options = new CopyDirectoryOptions
            {
                SearchPattern = searchPattern,
                Recursive = true
            };

            var context = new DirectoryTransferContext();
            context.FileTransferred += FileTransferredCallback;
            context.FileFailed += FileFailedCallback;
            context.FileSkipped += FileSkippedCallback;

            await TransferManager.CopyDirectoryAsync(sourceDirectory, destinationDirectory, true, options, context);
        }

        private void FileTransferredCallback(object sender, TransferEventArgs e)
        {
            _logger.LogWarning("Transfer succeeds. {0} -> {1}.", e.Source, e.Destination);
        }

        private void FileFailedCallback(object sender, TransferEventArgs e)
        {
            _logger.LogWarning("Transfer fails. {0} -> {1}. Error message:{2}", e.Source, e.Destination,
                e.Exception.Message);
        }

        private void FileSkippedCallback(object sender, TransferEventArgs e)
        {
            _logger.LogWarning("Transfer skips. {0} -> {1}.", e.Source, e.Destination);
        }
    }
}