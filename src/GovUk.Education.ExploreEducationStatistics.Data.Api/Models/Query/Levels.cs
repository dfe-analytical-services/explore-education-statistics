using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class Levels
    {
        private static readonly Dictionary<Level, string[]> values = new Dictionary<Level, string[]>
        {
            {Level.National, new[] {"National"}},
            {Level.Region, new[] {"Region", "Regional"}},
            {Level.Local_Authority, new[] {"Local Authority"}},
            {Level.School, new[] {"School"}}
        };

        public static Level EnumFromStringForImport(string value)
        {
            foreach (var keyValuePair in values)
            {
                if (keyValuePair.Value.Select(s => s.ToLower()).Contains(value.ToLower()))
                {
                    return keyValuePair.Key;
                }
            }

            throw new ArgumentException("Unexpected value: " + value);
        }
    }
}