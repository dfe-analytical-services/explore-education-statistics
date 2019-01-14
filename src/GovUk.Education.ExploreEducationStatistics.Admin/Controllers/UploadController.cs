using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Route("api/Upload")]
    [RequestSizeLimit(100*1024*1024)]
    public class UploadController : Controller
    {
        [HttpPost("Upload")]
        public async Task<IActionResult> Post(IFormFile file)
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
            
            ProcessAsync(filePath).GetAwaiter().GetResult();
            return Ok(new { count = 1, file.Length, filePath});
        }
        
        private static async Task ProcessAsync(string sourceFile)
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
            
            if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
            {
                try
                {   
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var queueClient = storageAccount.CreateCloudQueueClient();
                    
                    var blobContainer = blobClient.GetContainerReference("publications");
                    var queue = queueClient.GetQueueReference("publications");
                    
                    await blobContainer.CreateIfNotExistsAsync();
                    await queue.CreateIfNotExistsAsync();

                    // Set the permissions so the blobs are public. 
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await blobContainer.SetPermissionsAsync(permissions);

                    var publicationName = "publication-" + Guid.NewGuid();
                    
                    CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference(publicationName);
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);
                    
                    CloudQueueMessage message = new CloudQueueMessage(publicationName);
                    await queue.AddMessageAsync(message);
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                }
            }
        }
    }
}
