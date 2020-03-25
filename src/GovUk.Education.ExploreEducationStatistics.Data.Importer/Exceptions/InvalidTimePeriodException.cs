using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidTimePeriodException : Exception
    {
        public InvalidTimePeriodException(string name) : base($"Invalid time period: {name}")
        {
        }
    }
}