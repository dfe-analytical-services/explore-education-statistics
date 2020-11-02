using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class ImportRecoveryHandler
    {
        public static void CheckIncompleteImports(string connectionString)
        {
            var tblStorageAccount = CloudStorageAccount.Parse(connectionString);
            var storageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(connectionString);

            // Not ideal. We manually create the blob clients as we can't inject
            // FileStorageService at this point in the application lifecycle.
            // This would require re-architecting this code into something like a
            // cron function, which may or may not be possible as there
            // are a number of intricacies and potential pitfalls (ask Si).
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainer = blobServiceClient.GetBlobContainerClient(PrivateFilesContainerName);

            var tableClient = tblStorageAccount.CreateCloudTableClient();
            var queueClient = storageAccount.CreateCloudQueueClient();
            var availableQueue = queueClient.GetQueueReference("imports-available");
            var pendingQueue = queueClient.GetQueueReference("imports-pending");
            var table = tableClient.GetTableReference(DatafileImportsTableName);

            availableQueue.CreateIfNotExists();
            pendingQueue.CreateIfNotExists();
            table.CreateIfNotExists();
            availableQueue.Clear();
            pendingQueue.Clear();

            TableContinuationToken token = null;
            do
            {
                var resultSegment =
                    table.ExecuteQuerySegmentedAsync(BuildQuery(), token).Result;
                token = resultSegment.ContinuationToken;

                foreach (var entity in resultSegment.Results)
                {
                    Console.Out.WriteLine($"Recovering {entity.PartitionKey} : {entity.RowKey}");

                    // If batch was not split then just processing again by adding to pending queue
                    if (entity.Status.Equals(IStatus.QUEUED)
                        || entity.Status.Equals(IStatus.PROCESSING_ARCHIVE_FILE)
                        || entity.Status.Equals(IStatus.STAGE_1))
                    {
                        Console.WriteLine($"No data imported for {entity.PartitionKey} : {entity.RowKey} - Import will be re-run");
                        pendingQueue.AddMessage(new CloudQueueMessage(entity.Message));
                    }
                    // Else create a message for all remaining batches
                    else
                    {
                        var m = JsonConvert.DeserializeObject<ImportMessage>(entity.Message);
                        var batches = GetBatchesRemaining(blobContainer, entity.PartitionKey, m.OrigDataFileName)
                            .Result;

                        // If no batches then assume it didn't get passed initial validation stage
                        if (!batches.Any())
                        {
                            pendingQueue.AddMessage(new CloudQueueMessage(entity.Message));
                            return;
                        }

                        foreach (var folderAndFilename in batches)
                        {
                            availableQueue.AddMessage(new CloudQueueMessage(BuildMessage(m, folderAndFilename.Name)));
                        }
                    }
                }
            } while (token != null);
        }

        private static TableQuery<DatafileImport> BuildQuery()
        {
            var combineFilters = TableQuery.GenerateFilterCondition("Status",
                    QueryComparisons.Equal, IStatus.QUEUED.ToString());

            IStatus[] statuses = {
                IStatus.PROCESSING_ARCHIVE_FILE,
                IStatus.STAGE_1,
                IStatus.STAGE_2,
                IStatus.STAGE_3,
                IStatus.STAGE_4,
                IStatus.STAGE_5
            };

            combineFilters = statuses.Aggregate(combineFilters, (current, status) =>
                TableQuery.CombineFilters(current,
                    TableOperators.Or,
                    TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status.ToString())));

            return new TableQuery<DatafileImport>().Where(combineFilters);
        }

        private static async Task<List<BlobItem>> GetBatchesRemaining(
            BlobContainerClient blobContainer,
            string releaseId,
            string origDataFileName)
        {
            var batchBlobs = new List<BlobItem>();

            string continuationToken = null;

            do
            {
                var blobPages = blobContainer.GetBlobsAsync(
                        BlobTraits.Metadata,
                        prefix: AdminReleaseDirectoryPath(releaseId, ReleaseFileTypes.Data)
                    )
                    .AsPages(continuationToken);

                await foreach (Page<BlobItem> page in blobPages)
                {
                    foreach (var blob in page.Values)
                    {
                        if (blob == null)
                        {
                            break;
                        }

                        if (IsBatchFile(blob.Name, releaseId) && blob.Name.Contains(origDataFileName))
                        {
                            batchBlobs.Add(blob);
                        }
                    }

                    continuationToken = page.ContinuationToken;
                }
            } while (continuationToken != string.Empty);

            return batchBlobs;
        }

        private static bool IsBatchFile(string path, string releaseId)
        {
            return path.StartsWith(AdminReleaseBatchesDirectoryPath(releaseId));
        }

        private static string BuildMessage(ImportMessage message, string folderAndFilename)
        {
            var fileName = folderAndFilename.Split(BatchesDir + "/")[1];
            var batchNo = Int16.Parse(fileName.Substring(fileName.Length-6));

            Console.WriteLine($"Recreating message queue for {fileName}");

            var iMessage = new ImportMessage
            {
                SubjectId = message.SubjectId,
                DataFileName = $"{BatchesDir}/{fileName}",
                OrigDataFileName = message.DataFileName,
                Release = message.Release,
                BatchNo = batchNo,
                NumBatches = message.NumBatches,
                RowsPerBatch = message.RowsPerBatch,
                TotalRows = message.TotalRows
            };

            return JsonConvert.SerializeObject(iMessage);
        }
    }
}