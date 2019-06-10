using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PublicationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public PublicationViewModel GetPublication(string slug)
        {
            return _mapper.Map<PublicationViewModel>(_context.Publications.FirstOrDefault(t => t.Slug == slug));
        }
    }
}