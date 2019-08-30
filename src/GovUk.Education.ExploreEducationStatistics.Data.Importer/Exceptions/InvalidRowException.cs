using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidRowException : Exception
    {
        public InvalidRowException(int row, string fileName) : base($"Invalid row found at row {row} in file {fileName}")
        {
        }
    }
}