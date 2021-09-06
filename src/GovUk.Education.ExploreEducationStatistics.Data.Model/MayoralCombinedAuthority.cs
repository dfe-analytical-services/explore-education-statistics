namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class MayoralCombinedAuthority : ObservationalUnit
    {
        public MayoralCombinedAuthority()
        {
        }
        
        public MayoralCombinedAuthority(string code, string name) : base(code, name)
        {
        }
        
        public static MayoralCombinedAuthority Empty()
        {
            return new MayoralCombinedAuthority(null, null);
        }
    }
}