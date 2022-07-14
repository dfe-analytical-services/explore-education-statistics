#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Utils
{
    public static class CopyDirectoryCallbacks
    {
#pragma warning disable 1998
        public static async Task<bool> TransferBlobIfFileExistsCallback(object source,
#pragma warning restore 1998
            List<File> files,
            IBlobContainer sourceContainerName,
            ILogger logger)
        {
            if (source is not CloudBlockBlob item)
            {
                return false;
            }

            if (files.Exists(file => file.Path() == item.Name))
            {
                return true;
            }

            logger.LogInformation("Not transferring {0}/{1}", sourceContainerName, item.Name);
            return false;
        }
    }
}
