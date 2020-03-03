using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class ImportMessage
    {
        public Guid SubjectId { get; set; }
        public string DataFileName { get; set; }
        public string OrigDataFileName { get; set; }
        public Release Release { get; set; }
        public int NumBatches { get; set; }
        public int BatchNo { get; set; }
        public int RowsPerBatch { get; set; }
        public bool Seeding{ get; set; }
        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, {nameof(DataFileName)}: {DataFileName}, {nameof(OrigDataFileName)}: {OrigDataFileName}, {nameof(Release)}: {Release}, " +
                $"{nameof(NumBatches)}: {NumBatches}, {nameof(BatchNo)}: {BatchNo}, {nameof(RowsPerBatch)}: {RowsPerBatch}, {nameof(Seeding)}: {Seeding}";
        }
    }
}