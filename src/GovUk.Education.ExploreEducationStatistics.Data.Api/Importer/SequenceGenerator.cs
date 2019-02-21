namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class SequenceGenerator
    {
        private int index;

        public int Get()
        {
            index++;
            return index;
        }
    }
}