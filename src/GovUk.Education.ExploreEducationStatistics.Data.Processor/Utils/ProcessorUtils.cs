using System;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class ProcessorUtils
    {
        public static string GetDataFileName(ImportMessage message)
        {
            var i = FileStoragePathUtils.BatchesDir.Length;
            return message.NumBatches > 1 ? message.DataFileName.Substring(
                i + 1,message.DataFileName.LastIndexOf("_", StringComparison.Ordinal) - i - 1) : message.DataFileName;
        }
    }
}