namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class Subject
    {
        public DataCsvFilename Filename { get; set; }
        public string Name { get; set; }

        public DataCsvMetaFilename GetMetaFilename()
        {
            return Filename.GetMetaFilename();
        }
    }
}