using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions
{
    public static class DataImportExtensions
    {
        /// <summary>
        /// Determines if a file import consists exclusively of one geographic level.
        /// </summary>
        public static bool HasSoleGeographicLevel(this DataImport dataImport)
        {
            return dataImport.GeographicLevels.Count == 1;
        }
    }
}
