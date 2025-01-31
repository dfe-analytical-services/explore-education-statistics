using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions
{
    public class InvalidTimeIdentifierException : Exception
    {
        public InvalidTimeIdentifierException(string name) : base($"Invalid time identifier: {name}")
        {
        }
    }
}