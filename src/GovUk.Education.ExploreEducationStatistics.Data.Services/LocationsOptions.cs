using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class LocationsOptions
    {
        public const string Locations = "Locations";

        /// <summary>
        /// Map of <c>GeographicLevel</c> to attribute names of the <c>Location</c> type.
        /// This is used to configure a hierarchy of location attributes for a geographic level.
        /// Example: For Local Authority level data where Country, Region and Local Authority attributes are provided,
        /// a hierarchy Country -> Region -> Local Authority can be configured for this level using configuration data
        /// <c>
        /// "Locations": {
        ///   "Hierarchies": {
        ///     "LocalAuthority": [
        ///       "Country",
        ///       "Region"
        ///     ]
        ///   }
        /// }
        /// </c>
        /// </summary>
        /// <remarks>
        /// Attributes of the <c>Location</c> type must be specified in order of the hierarchy from top to bottom.
        /// </remarks>
        public Dictionary<GeographicLevel, List<string>> Hierarchies { get; init; } = new();
    }
}
