using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodologyAmendment(
            Guid originalMethodologyId)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(originalMethodologyId)
                .OnSuccess(_userService.CheckCanMakeAmendmentOfMethodology)
                .OnSuccess(HydrateMethodologyForAmendment)
                .OnSuccess(CreateAndSaveAmendment)
                .OnSuccess(amendment => _methodologyService.GetSummary(amendment.Id));
        }
        
        // TODO SOW4 EES-2156 - copy Methodology Files
        private async Task<Either<ActionResult, Methodology>> CreateAndSaveAmendment(Methodology methodology)
        {
            var amendment = methodology.CreateMethodologyAmendment(DateTime.UtcNow, _userService.GetUserId());
            var savedAmendment = await _context.Methodologies.AddAsync(amendment);
            await _context.SaveChangesAsync();
            return savedAmendment.Entity;
        }

        private async Task<Either<ActionResult, Methodology>> HydrateMethodologyForAmendment(Methodology methodology)
        {
            await _context
                .Entry(methodology)
                .Reference(m => m.MethodologyParent)
                .LoadAsync();

            return methodology;
        }
    }
}
