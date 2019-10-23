using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class DataBlockViewModel
    {
        public Guid Id { get; set; }

        public string Heading { get; set; }
        
        public string CustomFootnotes { get; set; }
        
        public string Name { get; set; }
        
        public string Source { get; set; }

        public DataBlockRequest DataBlockRequest { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public Summary Summary { get; set; }

        public List<Table> Tables { get; set; }
    }
}