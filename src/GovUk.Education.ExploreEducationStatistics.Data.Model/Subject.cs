using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Release Release { get; set; }
        public Guid ReleaseId { get; set; }
        public IEnumerable<Observation> Observations { get; set; }

        public Subject()
        {
        }

        public Subject(string name, Release release)
        {
            Name = name;
            Release = release;
        }
    }
}