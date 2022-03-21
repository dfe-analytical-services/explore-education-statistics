#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    /// <summary>
    /// Defines Geographic levels which data will only be imported for if a file consists solely of data of that level.
    /// </summary>
    public static class SoloImportableLevels
    {
        private static readonly List<GeographicLevel> Values = new()
        {
            GeographicLevel.Institution,
            GeographicLevel.PlanningArea,
            GeographicLevel.Provider,
            GeographicLevel.School,
        };

        public static bool IsSoloImportableLevel(this GeographicLevel geographicLevel)
        {
            return Values.Contains(geographicLevel);
        }
    }
}
