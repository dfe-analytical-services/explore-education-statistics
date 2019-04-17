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
            {GeographicLevel.Establishment, new[] {"establishment"}},
            {GeographicLevel.Local_Authority, new[] {"local authority"}},
            {GeographicLevel.Local_Authority_District, new[] {"local authority district"}},
            {GeographicLevel.Local_Enterprise_Partnerships, new[] {"local enterprise partnerships"}},
            {GeographicLevel.Mayoral_Combined_Authorities, new[] {"mayoral combined authorities"}},
            {GeographicLevel.National, new[] {"national"}},
            {GeographicLevel.Opportunity_Areas, new[] {"opportunity areas"}},
            {GeographicLevel.Parliamentary_Constituency, new[] {"parliamentary constituency"}},
            {GeographicLevel.Regional, new[] {"regional"}},
            {GeographicLevel.RSC_Region, new[] {"rsc region"}},
            {GeographicLevel.School, new[] {"school"}}
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