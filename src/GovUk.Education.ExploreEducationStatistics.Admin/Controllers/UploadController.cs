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
        public UploadController(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("AzureStorage");
        }
        
        [HttpPost("Upload/{publicationId}")]
        public async Task<IActionResult> Post(IFormFile file, string publicationId)
        {
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
            if (CloudStorageAccount.TryParse(_storageConnectionString, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var queueClient = storageAccount.CreateCloudQueueClient();

                    var blobContainer = blobClient.GetContainerReference("releases");
                    var queue = queueClient.GetQueueReference("releases");

                    await blobContainer.CreateIfNotExistsAsync();
                    await queue.CreateIfNotExistsAsync();
                    
                    // Set the permissions so the blobs are private. 
                    var permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Off
                    };
                    
                    await blobContainer.SetPermissionsAsync(permissions);

                    var releaseId = Guid.NewGuid();

                    var cloudBlockBlob = blobContainer.GetBlockBlobReference(releaseId.ToString());
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);


                    var message = new CloudQueueMessage(getReleaseMessage(publicationId, releaseId));
                    await queue.AddMessageAsync(message);
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
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