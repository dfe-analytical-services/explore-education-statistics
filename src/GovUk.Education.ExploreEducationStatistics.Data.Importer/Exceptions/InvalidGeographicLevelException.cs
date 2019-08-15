using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidGeographicLevelException : Exception
    {
        public InvalidGeographicLevelException(string name) : base($"Invalid geographic level: {name}")
        {
        }
    }
}