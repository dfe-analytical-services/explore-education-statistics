using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public class Levels
    {
        private static readonly Dictionary<string, Level> levels = new Dictionary<string, Level>
        {
            {"National", Level.National},
            {"LOCAL AUTHORITY", Level.Local_Authority}
        };

        public static Level getLevel(string level)
        {
            return levels.GetValueOrDefault(level);
        }

        public static string getLevel(Level level)
        {
            foreach (var keyValuePair in levels)
            {
                if (keyValuePair.Value == level)
                {
                    return keyValuePair.Key;
                }
            }

            throw new ArgumentException("Unexpected level: " + level);
        }
    }
}