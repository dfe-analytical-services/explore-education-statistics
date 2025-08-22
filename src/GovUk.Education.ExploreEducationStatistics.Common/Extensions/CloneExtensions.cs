namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class CloneExtensions
{
    public static T ShallowClone<T>(this T obj)
    {
        return ObjectCloner.ObjectCloner.ShallowClone(obj);
    }

    public static T DeepClone<T>(this T obj)
    {
        return ObjectCloner.ObjectCloner.DeepClone(obj);
    }
}
