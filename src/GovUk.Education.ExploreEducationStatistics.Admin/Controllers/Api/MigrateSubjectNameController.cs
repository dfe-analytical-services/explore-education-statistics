using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    /**
     * EES-1816 Temporary controller for copying Subject.Name to ReleaseFiles.Name
     */
    [ApiController]
    [Authorize]
    public class MigrateSubjectNameController : ControllerBase
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;

        public MigrateSubjectNameController(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
        }

        [HttpPatch("api/subjects/migrate-subject-names")]
        public async Task<ActionResult<Unit>> MigrateSubjectNames()
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccessVoid(async () =>
                {
                    await _contentDbContext.ReleaseFiles
                        .Include(rf => rf.File)
                        .Where(rf => rf.File.Type == FileType.Data)
                        .ToList()
                        .ForEachAsync(async rf =>
                        {
                            var subject = await _statisticsDbContext.Subject
                                .FindAsync(rf.File.SubjectId);
                            _contentDbContext.Update(rf);
                            rf.Name = subject.Name;
                        });
                    await _contentDbContext.SaveChangesAsync();
                })
                .HandleFailuresOrOk();
        }
    }
}
