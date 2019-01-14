namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class School
    {
        public School()
        {
        }

        public School(string estab, string laestab, string acadType, string acadOpend)
        {
            this.estab = estab;
            this.laestab = laestab;
            acad_type = acadType;
            acad_opend = acadOpend;
        }

        public string estab { get; set; }
        
        public string laestab { get; set; }
        
        public string acad_type { get; set; }
        
        public string acad_opend { get; set; }
    }
}