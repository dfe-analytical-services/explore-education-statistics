namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Meta
{
    public class MetaGroup<T> where T : IMeta
    {
        public string Name { get; set; }
        public T[] Meta { get; set; }
    }
}