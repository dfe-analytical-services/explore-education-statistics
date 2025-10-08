#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;

public class InvalidGeographicLevelException : Exception
{
    public InvalidGeographicLevelException(string name)
        : base($"Invalid geographic level: {name}") { }
}
