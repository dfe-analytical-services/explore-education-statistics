using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class ImporterException : Exception
    { 
        public long _subjectId { get; set; }

        protected ImporterException(long subjectId, string name) : base(name)
        {
            _subjectId = subjectId;
        }
    }
}