using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class CancelImportMessage
    {
        public Guid Id { get; set; }

        public CancelImportMessage(Guid id)
        {
            Id = id;
        }
    }
}