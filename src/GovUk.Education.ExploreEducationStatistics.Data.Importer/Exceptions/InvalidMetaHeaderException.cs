using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidMetaHeaderException : ImporterException
    {
        public InvalidMetaHeaderException(long subjectId) : base(subjectId, "Invalid meta header found")
        {
        }
    }
}