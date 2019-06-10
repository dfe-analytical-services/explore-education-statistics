using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

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
            var model = _mapper.Map<PublicationViewModel>(
                _context.Publications
                    .Include(p => p.Topic.Theme.Title)
                    .Include(p => p.Contact)
                    .FirstOrDefault(t => t.Slug == slug));

            return model;
        }
    }
}