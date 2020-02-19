using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class Release
    {
        public Guid Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Slug { get; set; }
        public Publication Publication { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public int Year { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ReleaseDate)}: {ReleaseDate}, {nameof(Slug)}: {Slug}, {nameof(Publication)}: {Publication}, {nameof(TimeIdentifier)}: {TimeIdentifier}, {nameof(Year)}: {Year}";
        }
    }
}