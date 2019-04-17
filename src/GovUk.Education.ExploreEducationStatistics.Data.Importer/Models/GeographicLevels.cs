using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Models
{
    public static class GeographicLevels
    {
        private static readonly Dictionary<GeographicLevel, string[]> values = new Dictionary<GeographicLevel, string[]>
        {
            {GeographicLevel.National, new[] {"National"}},
            {GeographicLevel.Regional, new[] {"Region", "Regional"}},
            {GeographicLevel.RSC_Region, new[] {"RSC region"}},
            {GeographicLevel.Local_Authority, new[] {"Local Authority"}},
            {GeographicLevel.Local_Authority_District, new[] {"Local authority district"}},
            {GeographicLevel.Parliamentary_Constituency, new[] {"Parliamentary constituency"}},
            {GeographicLevel.Establishment, new[] {"Establishment"}},
            {GeographicLevel.School, new[] {"School"}}
        };

        public static GeographicLevel EnumFromStringForImport(string value)
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