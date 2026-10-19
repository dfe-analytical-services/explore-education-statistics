#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

// Separate this into a separate class
// for ease of mocking in tests.
public class SubjectDeleter
{
    public virtual void Delete(Guid subjectId, StatisticsDbContext context)
    {
        // Use raw delete as EF can't correctly figure out how to cascade the delete, whilst the database can.
        // N.B. This delete will be slow if there are a large number of observations but this is only
        // executed by the tests when the theme is torn down so ensure files used are < 1000 rows.
        context.Database.ExecuteSqlInterpolated($"DELETE FROM Subject WHERE Id = {subjectId}");
    }
}
