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
            {GeographicLevel.Local_Authority, new[] {"local authority"}},
            {GeographicLevel.Local_Authority_District, new[] {"local authority district"}},
            {GeographicLevel.Local_Enterprise_Partnership, new[] {"local enterprise partnerships"}},
            {GeographicLevel.Mayoral_Combined_Authority, new[] {"mayoral combined authorities"}},
            {GeographicLevel.Multi_Academy_Trust, new[] {"mat"}},
            {GeographicLevel.Country, new[] {"national"}},
            {GeographicLevel.Opportunity_Area, new[] {"opportunity areas"}},
            {GeographicLevel.Parliamentary_Constituency, new[] {"parliamentary constituency"}},
            {GeographicLevel.Provider, new[] {"provider"}},
            {GeographicLevel.Region, new[] {"regional"}},
            {GeographicLevel.RSC_Region, new[] {"rsc region"}},
            {GeographicLevel.School, new[] {"school"}},
            {GeographicLevel.Sponsor, new[] {"sponsor"}},
            {GeographicLevel.Ward, new[] {"ward"}}
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