using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    [Obsolete]
    public class School
    {
        public School()
        {
        }

        public School(string estab, string laestab, string academyType, string academyOpenDate)
        {
            this.estab = estab;
            this.laestab = laestab;
            AcademyType = academyType;
            AcademyOpenDate = academyOpenDate;
        }

        public string estab { get; set; }
        
        public string laestab { get; set; }
        
        public string AcademyType { get; set; }
        
        public string AcademyOpenDate { get; set; }
    }
}