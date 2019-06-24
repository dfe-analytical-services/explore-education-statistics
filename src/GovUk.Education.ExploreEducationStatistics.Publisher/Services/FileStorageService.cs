using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger _logger;

        private readonly string _storageConnectionString;

        public FileStorageService(IConfiguration config,
            ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _storageConnectionString = config.GetConnectionString("AzureStorage");
        }

        public async Task CopyFilesAsync(string publication,
            string release,
            string sourceContainerName,
            string destinationContainerName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var sourceContainer = blobClient.GetContainerReference(sourceContainerName);
            var destinationContainer = blobClient.GetContainerReference(destinationContainerName);

            var sourceDirectory = sourceContainer.GetDirectoryReference($"{publication}/{release}");
            var destinationDirectory = destinationContainer.GetDirectoryReference($"{publication}/{release}");

            var options = new CopyDirectoryOptions
            {
                SearchPattern = "*.csv",
                Recursive = true
            };

            var context = new DirectoryTransferContext();
            context.FileTransferred += FileTransferredCallback;
            context.FileFailed += FileFailedCallback;
            context.FileSkipped += FileSkippedCallback;

            await TransferManager.CopyDirectoryAsync(sourceDirectory, destinationDirectory, false, options, context);
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