using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class ImporterUtils
    {
        public static readonly List<GeographicLevel> IgnoredGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Institution,
            GeographicLevel.Provider,
            GeographicLevel.School,
            GeographicLevel.PlanningArea
        };

        private static readonly List<GeographicLevel> AllowedSoloGeographicLevels = new List<GeographicLevel>
        {
            GeographicLevel.Provider,
            GeographicLevel.School,
        };

        /// <summary>
        /// If a subject contains a single geographic level that is contained in AllowedSoloGeographicLevels, then
        /// we import every CSV row of that subject's data file.
        /// </summary>
        /// <remarks>
        /// If a subject contains more than one geographic level or isn't included in AllowedSoloGeographicLevels,
        /// we then defer to AllowRowImport to determine if a particular row should be imported.
        /// </remarks>
        public static bool HasSoloAllowedGeographicLevel(HashSet<GeographicLevel> subjectGeographicLevels)
        {
            return subjectGeographicLevels.Count == 1
                   && AllowedSoloGeographicLevels.Contains(subjectGeographicLevels.ElementAt(0));
        }

        /// <summary>
        /// Determines if a specific data file CSV row should be imported based on whether it is in
        /// IgnoredGeographicLevels.
        /// </summary>
        /// <remarks>
        /// This method is used in conjunction with HasSoloAllowedGeographicLevel to determine what data file CSV rows
        /// should and shouldn't be imported.
        /// </remarks>
        public static bool AllowRowImport(GeographicLevel rowGeographicLevel)
        {
            return !IgnoredGeographicLevels.Contains(rowGeographicLevel);
        }

    }
}
