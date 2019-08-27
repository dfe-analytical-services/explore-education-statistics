using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Publication = GovUk.Education.ExploreEducationStatistics.Data.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Data.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Data.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ReleaseProcessorService : IReleaseProcessorService
    {
        private readonly ILogger<IReleaseProcessorService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        
        public ReleaseProcessorService(
            ILogger<IReleaseProcessorService> logger,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        public Subject CreateOrUpdateRelease(SubjectData subjectData, ImportMessage message)
        {
            var release = CreateOrUpdateRelease(message);
            return RemoveAndCreateSubject(subjectData.Name, release);
        }

        private Subject RemoveAndCreateSubject(string name, Release release)
        {
            var subject = _context.Subject
                .FirstOrDefault(s => s.Name.Equals(name) && s.ReleaseId == release.Id);
            
            // If the subject exists then this must be a reload of the same release/subject so delete & re-create.

            if (subject != null)
            {
                _context.Subject.Remove(subject);
            }

            subject = _context.Subject.Add(new Subject(name, release)).Entity;

            return subject;
        }
        
        private Release CreateOrUpdateRelease(ImportMessage message)
        {
            var release = _context.Release
                .Include(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(r => r.Id.Equals(message.Release.Id));

            if (release == null)
            {
                release = new Release
                {
                    Id = message.Release.Id,
                    ReleaseDate = message.Release.ReleaseDate,
                    Title = message.Release.Title,
                    Slug = message.Release.Slug,
                    Publication = CreateOrUpdatePublication(message)
                };
                return _context.Release.Add(release).Entity;
            }

            release = _mapper.Map(message.Release, release);
            return _context.Release.Update(release).Entity;
        }

        private Publication CreateOrUpdatePublication(ImportMessage message)
        {
            var publication = _context.Publication
                .Include(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(p => p.Id.Equals(message.Release.Publication.Id));

            if (publication == null)
            {
                publication = new Publication
                {
                    Id = message.Release.Publication.Id,
                    Title = message.Release.Publication.Title,
                    Slug = message.Release.Publication.Slug,
                    Topic = CreateOrUpdateTopic(message)
                };
                return _context.Publication.Add(publication).Entity;
            }

            publication = _mapper.Map(message.Release.Publication, publication);
            return _context.Publication.Update(publication).Entity;
        }
        
        private Topic CreateOrUpdateTopic(ImportMessage message)
        {
            var topic = _context.Topic
                .Include(p => p.Theme)
                .FirstOrDefault(t => t.Id.Equals(message.Release.Publication.Topic.Id));

            if (topic == null)
            {
                topic = new Topic
                {
                    Id = message.Release.Publication.Topic.Id,
                    Title = message.Release.Publication.Topic.Title,
                    Slug = message.Release.Publication.Topic.Slug,
                    Theme = CreateOrUpdateTheme(message)
                };
                return _context.Topic.Add(topic).Entity;
            }

            topic = _mapper.Map(message.Release.Publication.Topic, topic);
            return _context.Topic.Update(topic).Entity;
        }

        private Theme CreateOrUpdateTheme(ImportMessage message)
        {
            var theme = _context.Theme
                .FirstOrDefault(t => t.Id.Equals(message.Release.Publication.Topic.Theme.Id));

            if (theme == null)
            {
                theme = _mapper.Map<Theme>(message.Release.Publication.Topic.Theme);
                return _context.Theme.Add(theme).Entity;
            }

            theme = _mapper.Map(message.Release.Publication.Topic.Theme, theme);
            return _context.Theme.Update(theme).Entity;
        }
    }
}