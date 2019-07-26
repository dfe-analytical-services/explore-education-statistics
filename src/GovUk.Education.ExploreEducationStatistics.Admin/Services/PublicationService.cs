using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using Microsoft.EntityFrameworkCore;
using UserId = System.Guid;
using TopicId = System.Guid;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;

        public PublicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Publication Get(Guid id)
        {
            return _context.Publications.FirstOrDefault(x => x.Id == id);
        }

        public Publication Get(string slug)
        {
            return _context.Publications.FirstOrDefault(x => x.Slug == slug);
        }

        public List<Publication> List()
        {
            return _context.Publications.ToList();
        }

        // TODO it maybe necessary to add authorisation to this method
        public List<PublicationViewModel> GetByTopicAndUser(TopicId topicId, UserId userId)
        {
            var publications = _context.Publications.Where(p => p.TopicId == topicId)
                .Include(p => p.Contact)
                .Include(p => p.Releases)
                .Include(p => p.Methodology)
                .ToList();

            return PublicationToPublicationViewModelMapper.Map<List<PublicationViewModel>>(publications);
        }

        public PublicationViewModel CreatePublication(CreatePublicationViewModel publication)
        {
            var saved = _context.Publications.Add(new Publication
            {
                Id = Guid.NewGuid(),
                ContactId = publication.ContactId,
                Title = publication.Title,
                TopicId = publication.TopicId,
                MethodologyId = publication.MethodologyId
            });
            _context.SaveChanges();
            return GetViewModel(saved.Entity.Id);
        }

        public PublicationViewModel GetViewModel(PublicationId publicationId)
        {
            var publication = _context.Publications.Include(p => p.Methodology)
                .Include(p => p.Contact)
                .Include(p => p.Releases)
                .Where(p => p.Id == publicationId)
                .Select(p => PublicationToPublicationViewModelMapper.Map<PublicationViewModel>(p))
                .FirstOrDefault();
            return publication;
        }

        private static readonly IMapper PublicationToPublicationViewModelMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Publication, PublicationViewModel>();
            cfg.CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id));
            cfg.CreateMap<Methodology, MethodologyViewModel>();
        }).CreateMapper();
    }
}