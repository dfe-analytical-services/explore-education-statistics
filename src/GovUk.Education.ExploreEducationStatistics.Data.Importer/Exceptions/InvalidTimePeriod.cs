using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidTimePeriod : Exception
    {
        public InvalidTimePeriod(string name) : base($"Invalid time period: {name}")
        {
        }
    }
}