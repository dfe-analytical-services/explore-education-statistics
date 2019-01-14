using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers
{
    [Route("api/Upload")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public class UploadController : Controller
    {
        [HttpPost("Upload/{publicationId}")]
        public async Task<IActionResult> Post(IFormFile file, String publicationId)
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

        private static async Task ProcessAsync(string sourceFile, Guid publicationId)
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");

            if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
            {
                try
                {
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var queueClient = storageAccount.CreateCloudQueueClient();

                    var blobContainer = blobClient.GetContainerReference("releases");
                    var queue = queueClient.GetQueueReference("releases");

                    await blobContainer.CreateIfNotExistsAsync();
                    await queue.CreateIfNotExistsAsync();

                    // Set the permissions so the blobs are public. 
                    var permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
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

        private static string getReleaseMessage(Guid publicationId, Guid releaseId)
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