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
        private readonly IMapper _mapper;

        public MethodologyAmendmentService(
            IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService,
            IMapper mapper)
        {
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, MethodologySummaryViewModel>> CreateMethodologyAmendment(
            Guid originalMethodologyId)
        {
            return await _persistenceHelper.CheckEntityExists<Methodology>(originalMethodologyId)
                .OnSuccess(_userService.CheckCanMakeAmendmentOfMethodology)
                .OnSuccessDo(() => throw new NotImplementedException())
                .OnSuccess(_mapper.Map<MethodologySummaryViewModel>);
        }
    }
}
