#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class SubjectDeleter(StatisticsDbContext statisticsDbContext) : ISubjectDeleter
{
    public void Delete(Guid subjectId)
    {
        // Use raw delete as EF can't correctly figure out how to cascade the delete, whilst the database can.
        // N.B. This delete will be slow if there are a large number of observations but this is only
        // executed by the tests when the theme is torn down so ensure files used are < 1000 rows.
        statisticsDbContext.Database.ExecuteSqlInterpolated($"DELETE FROM Subject WHERE Id = {subjectId}");
    }
}
