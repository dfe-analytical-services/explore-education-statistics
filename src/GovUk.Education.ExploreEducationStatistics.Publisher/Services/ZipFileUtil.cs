using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public static class ZipFileUtil
    {
        public static async Task ZipFilesToBlob(IEnumerable<CloudBlockBlob> files,
            CloudBlobDirectory directory,
            string filePath,
            string name,
            SetAttributesCallbackAsync setAttributesCallbackAsync,
            string excludePattern = null)
        {
            var cloudBlockBlob = CreateZip(directory, filePath, name);
            var memoryStream = new MemoryStream();

            var zipOutputStream = new ZipOutputStream(memoryStream);
            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                var excluded = excludePattern != null &&
                               Regex.IsMatch(file.Name, excludePattern, RegexOptions.IgnoreCase);
                if (!excluded)
                {
                    PutNextZipEntry(zipOutputStream, file);
                }
            }

            var context = new SingleTransferContext();
            context.SetAttributesCallbackAsync += setAttributesCallbackAsync;

            zipOutputStream.Finish();
            await TransferManager.UploadAsync(memoryStream, cloudBlockBlob, new UploadOptions(), context);

            zipOutputStream.Close();
        }

        private static void PutNextZipEntry(ZipOutputStream zipOutputStream, CloudBlob cloudBlob)
        {
            var zipEntry = new ZipEntry(GetZipEntryName(cloudBlob));
            zipOutputStream.PutNextEntry(zipEntry);
            cloudBlob.DownloadToStream(zipOutputStream);
        }

        private static string GetZipEntryName(CloudBlob cloudBlob)
        {
            return cloudBlob.Uri.Segments.Last();
        }

        private static CloudBlockBlob CreateZip(CloudBlobDirectory directory,
            string filePath,
            string name)
        {
            var blob = directory.GetBlockBlobReference(filePath);
            blob.Properties.ContentType = "application/x-zip-compressed";
            blob.Metadata.Add("name", name);
            return blob;
        }
    }
}