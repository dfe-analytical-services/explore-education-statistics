using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class PermlinkInfo
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public int QueryLocationCodes { get; set; }
        public int QueryLocationProviderCodes { get; set; }
        public GeographicLevel? QueryLocationGeographicLevel { get; set; }
        public int TableRows { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
    }
}
