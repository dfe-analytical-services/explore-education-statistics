namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class ImportMessage
    {
        public string DataFileName { get; set; }
        public Release Release { get; set; }

        public override string ToString()
        {
            return $"{nameof(DataFileName)}: {DataFileName}, {nameof(Release)}: {Release}";
        }
    }
}