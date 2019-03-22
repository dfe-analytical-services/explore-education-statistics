using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Models
{
    public static class Levels
    {
        private static readonly Dictionary<Level, string[]> values = new Dictionary<Level, string[]>
        {
            {Level.National, new[] {"National"}},
            {Level.Regional, new[] {"Region", "Regional"}},
            {Level.RSC_Region, new[] {"RSC region"}},
            {Level.Local_Authority, new[] {"Local Authority"}},
            {Level.Local_Authority_District, new[] {"Local authority district"}},
            {Level.Parliamentary_Constituency, new[] {"Parliamentary constituency"}},
            {Level.Establishment, new[] {"Establishment"}},
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