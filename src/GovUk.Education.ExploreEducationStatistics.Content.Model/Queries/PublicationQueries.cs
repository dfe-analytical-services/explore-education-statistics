#nullable enable
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;

public static class PublicationQueries
{
    public static IQueryable<Publication> GetBySlug(this IQueryable<Publication> query, string slug)
    {
        return query.Where(p => p.Slug == slug);
    }
}
