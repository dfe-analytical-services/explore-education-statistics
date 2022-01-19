#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class ImporterUtils
    {
        /// <summary>
        /// Levels which data will only be imported for if a file consists exclusively of data of that level.
        /// </summary>
        private static readonly List<GeographicLevel> SoloGeographicLevels = new()
        {
            GeographicLevel.Institution,
            GeographicLevel.PlanningArea,
            GeographicLevel.Provider,
            GeographicLevel.School,
        };

        public static bool IsSoloGeographicLevel(this GeographicLevel geographicLevel)
        {
            return SoloGeographicLevels.Contains(geographicLevel);
        }

        /// <summary>
        /// Determines if a file import consists exclusively of one geographic level.
        /// </summary>
        public static bool IsSoleGeographicLevel(this DataImport dataImport)
        {
            return dataImport.GeographicLevels.Count == 1;
        }

        /// <summary>
        /// Determines if a row should be imported based on geographic level.
        /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' levels are ignored. 
        /// </summary>
        public static bool AllowRowImport(bool soleGeographicLevel,
            IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return soleGeographicLevel ||
                   !GetGeographicLevel(rowValues, colValues).IsSoloGeographicLevel();
        }

        public static GeographicLevel GetGeographicLevel(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            var value = CsvUtil.Value(rowValues, colValues, "geographic_level");

            foreach (var val in (GeographicLevel[]) Enum.GetValues(typeof(GeographicLevel)))
            {
                if (val.GetEnumLabel().ToLower().Equals(value.ToLower()))
                {
                    return val;
                }
            }

            throw new InvalidGeographicLevelException(value);
        }
    }
}
