#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class OrganisationExtensions
{
    public static IOrderedQueryable<Organisation> OrderByTitleWithDepartmentForEducationFirst(
        this IQueryable<Organisation> query
    ) => query.OrderByDescending(o => o.Title == Organisation.DepartmentForEducationTitle).ThenBy(o => o.Title);

    public static IOrderedEnumerable<Organisation> OrderByTitleWithDepartmentForEducationFirst(
        this IEnumerable<Organisation> organisations
    ) => organisations.OrderByDescending(o => o.Title == Organisation.DepartmentForEducationTitle).ThenBy(o => o.Title);
}
