using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class Release
    {
        public Guid Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public Publication Publication { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Id)}: {Id}, {nameof(ReleaseDate)}: {ReleaseDate}, {nameof(Title)}: {Title}, {nameof(Slug)}: {Slug}, {nameof(Publication)}: {Publication}";
        }
    }
}