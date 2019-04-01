namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LevelComposite
    {
        public long Id { get; set; }
        public Level Level { get; set; }
        public Country Country { get; set; }
        public Region Region { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
    }
}