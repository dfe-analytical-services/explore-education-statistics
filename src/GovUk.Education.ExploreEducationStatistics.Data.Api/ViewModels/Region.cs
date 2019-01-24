using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    [Obsolete]
    public class Region
    {
        public Region()
        {
        }

        public Region(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        
        public string Code { get; set; }
    }
}