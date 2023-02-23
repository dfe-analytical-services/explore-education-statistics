#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils
{
    public static class ObservationViewModelBuilder
    {
        public static ObservationViewModel BuildObservation(Observation observation, IEnumerable<Guid> indicators)
        {
            return new ObservationViewModel
            {
                Id = observation.Id,
                Filters = FilterItems(observation),
                GeographicLevel = observation.Location.GeographicLevel,
                LocationId = observation.LocationId,
                Measures = Measures(observation, indicators.ToList()),
                TimePeriod = observation.GetTimePeriod()
            };
        }

        private static List<Guid> FilterItems(Observation observation)
        {
            return observation
                .FilterItems
                .Select(item => item.FilterItemId)
                .ToList();
        }

        private static Dictionary<Guid, string> Measures(Observation observation, List<Guid> indicators)
        {
            if (!indicators.Any())
            {
                return observation.Measures;
            }

            return observation.Measures
                .Where(pair => indicators.Contains(pair.Key))
                .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
