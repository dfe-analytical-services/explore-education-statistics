using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class BoundaryLevel
    {
        public long Id { get; set; }
        public GeographicLevel Level { get; set; }
        
        public string Label { get; set; }
        
        // The date that this Boundary Level was introduced to the service.
        public DateTime Created { get; set; }
        
        // The date that this Boundary Level was published by the data issuer.
        public DateTime Published { get; set; }
    }
}