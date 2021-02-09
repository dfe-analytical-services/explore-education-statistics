using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class ImportMessage
    {
        public Guid Id { get; set; }

        public ImportMessage(Guid id)
        {
            Id = id;
        }
    }
}