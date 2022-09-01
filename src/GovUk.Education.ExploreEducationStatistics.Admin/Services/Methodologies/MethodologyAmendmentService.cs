#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyAmendmentService : IMethodologyAmendmentService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IMethodologyService _methodologyService;
        private readonly ContentDbContext _context;

        public MethodologyAmendmentService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IMethodologyService methodologyService,
            ContentDbContext context)
        {
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _methodologyService = methodologyService;
            _context = context;
        }

        public async Task<Either<ActionResult, MethodologyVersionViewModel>> CreateMethodologyAmendment(
            Guid originalMethodologyVersionId)
        {
            return await _persistenceHelper.CheckEntityExists<MethodologyVersion>(originalMethodologyVersionId)
                .OnSuccess(_userService.CheckCanMakeAmendmentOfMethodology)
                .OnSuccess(HydrateMethodologyVersionForAmendment)
                .OnSuccess(CreateAndSaveAmendment)
                .OnSuccessDo(LinkOriginalMethodologyFilesToAmendment)
                .OnSuccess(amendment => _methodologyService.GetMethodology(amendment.Id));
        }

        private async Task<Either<ActionResult, MethodologyVersion>> CreateAndSaveAmendment(
            MethodologyVersion methodologyVersion)
        {
            var amendment = methodologyVersion.CreateMethodologyAmendment(DateTime.UtcNow, _userService.GetUserId());
            var savedAmendment = (await _context.MethodologyVersions.AddAsync(amendment)).Entity;
            await _context.SaveChangesAsync();
            return savedAmendment;
        }

        private async Task<Either<ActionResult, Unit>> LinkOriginalMethodologyFilesToAmendment(
            MethodologyVersion methodologyVersion)
        {
            var originalFiles = await _context
                .MethodologyFiles
                .AsQueryable()
                .Where(f => f.MethodologyVersionId == methodologyVersion.PreviousVersionId)
                .ToListAsync();

            var fileCopies = originalFiles
                .Select(f => new MethodologyFile
                {
                    FileId = f.FileId,
                    MethodologyVersionId = methodologyVersion.Id
                });

            await _context.AddRangeAsync(fileCopies);
            await _context.SaveChangesAsync();
            return Unit.Instance;
        }

        private async Task<Either<ActionResult, MethodologyVersion>> HydrateMethodologyVersionForAmendment(
            MethodologyVersion methodologyVersion)
        {
            await _context
                .Entry(methodologyVersion)
                .Collection(m => m.Notes)
                .LoadAsync();

            await _context
                .Entry(methodologyVersion)
                .Reference(m => m.Methodology)
                .LoadAsync();

            await _context
                .Entry(methodologyVersion)
                .Reference(m => m.MethodologyContent)
                .LoadAsync();

            return methodologyVersion;
        }
    }
}
