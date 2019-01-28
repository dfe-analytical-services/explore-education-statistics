using System;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Route("api/Upload")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public class UploadController : Controller
    {
        private readonly string _storageConnectionString;
        private readonly ILogger _logger;
        
        public UploadController(IConfiguration config, ILogger<UploadController> logger)
        {
            _storageConnectionString = config.GetConnectionString("AzureStorage");
            _logger = logger;
        }
        
        [HttpPost("Upload/{publicationId}")]
        public async Task<IActionResult> Post(IFormFile file, string publicationId)
        {
            _logger.LogInformation("Received file for upload");
            // full path to file in temp location
            var filePath = Path.GetTempFileName();
            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            
            ProcessAsync(filePath, Guid.Parse(publicationId)).GetAwaiter().GetResult();
            return Ok(new {count = 1, file.Length, filePath});
        }

        private async Task ProcessAsync(string sourceFile, Guid publicationId)
        {
            _logger.LogInformation("Starting processing of file");

            if (CloudStorageAccount.TryParse(_storageConnectionString, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var queueClient = storageAccount.CreateCloudQueueClient();

                    var blobContainer = blobClient.GetContainerReference("releases");
                    var queue = queueClient.GetQueueReference("releases");

                    _logger.LogInformation("Create blob containers if not exist");
                    await blobContainer.CreateIfNotExistsAsync();
                    await queue.CreateIfNotExistsAsync();
                    
                    // Set the permissions so the blobs are public. 
                    var permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };

                    _logger.LogInformation("Set blob permissions");
                    await blobContainer.SetPermissionsAsync(permissions);

                    var releaseId = Guid.NewGuid();

                    _logger.LogInformation("Starting file upload to blob stroage");
                    var cloudBlockBlob = blobContainer.GetBlockBlobReference(releaseId.ToString());
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                    _logger.LogInformation("Add message to queue");
                    var message = new CloudQueueMessage(getReleaseMessage(publicationId, releaseId));
                    await queue.AddMessageAsync(message);
                }
                catch (StorageException ex)
                {
                    _logger.LogInformation("Error returned from the service: {0}", ex.Message);
                }
            }
        }

        private string getReleaseMessage(Guid publicationId, Guid releaseId)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(ReleaseMessage));
            var ms = new MemoryStream();
            
            jsonSerializer.WriteObject(ms, new ReleaseMessage()
            {
                Publication = publicationId,
                Release = releaseId,
                UploadedOn = DateTime.Now
            });

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}