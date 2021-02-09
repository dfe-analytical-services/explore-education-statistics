using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions
{
    public class InvalidTimePeriodException : Exception
    {
        public InvalidTimePeriodException(string name) : base($"Invalid time period: {name}")
        {
        }
    }
}