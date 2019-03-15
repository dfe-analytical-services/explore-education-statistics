namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta
{
    public class MetaGroup<T> where T : IMeta
    {
        public string Name { get; set; }
        public T[] Meta { get; set; }
    }
}