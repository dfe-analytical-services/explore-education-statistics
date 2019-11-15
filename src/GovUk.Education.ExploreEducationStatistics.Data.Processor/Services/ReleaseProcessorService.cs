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
        private readonly IMapper _mapper;

        public ReleaseProcessorService(
            ILogger<IReleaseProcessorService> logger,
            IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public Subject CreateOrUpdateRelease(SubjectData subjectData, ImportMessage message, StatisticsDbContext context)
        {
            var release = CreateOrUpdateRelease(message, context);
            return RemoveAndCreateSubject(subjectData.Name, release, context);
        }

        private Subject RemoveAndCreateSubject(string name, Release release, StatisticsDbContext context)
        {
            var subject = context.Subject
                .FirstOrDefault(s => s.Name.Equals(name) && s.ReleaseId == release.Id);
            
            // If the subject exists then this must be a reload of the same release/subject so delete & re-create.

            if (subject != null)
            {
                context.Subject.Remove(subject);
            }

            subject = context.Subject.Add(new Subject(name, release)).Entity;

            return subject;
        }
        
        private Release CreateOrUpdateRelease(ImportMessage message, StatisticsDbContext context)
        {
            var release = context.Release
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
                    Publication = CreateOrUpdatePublication(message, context)
                };
                return context.Release.Add(release).Entity;
            }

            release = _mapper.Map(message.Release, release);
            return context.Release.Update(release).Entity;
        }

        private Publication CreateOrUpdatePublication(ImportMessage message, StatisticsDbContext context)
        {
            var publication = context.Publication
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
                    Topic = CreateOrUpdateTopic(message, context)
                };
                return context.Publication.Add(publication).Entity;
            }

            publication = _mapper.Map(message.Release.Publication, publication);
            return context.Publication.Update(publication).Entity;
        }
        
        private Topic CreateOrUpdateTopic(ImportMessage message, StatisticsDbContext context)
        {
            var topic = context.Topic
                .Include(p => p.Theme)
                .FirstOrDefault(t => t.Id.Equals(message.Release.Publication.Topic.Id));

            if (topic == null)
            {
                topic = new Topic
                {
                    Id = message.Release.Publication.Topic.Id,
                    Title = message.Release.Publication.Topic.Title,
                    Slug = message.Release.Publication.Topic.Slug,
                    Theme = CreateOrUpdateTheme(message, context)
                };
                return context.Topic.Add(topic).Entity;
            }

            topic = _mapper.Map(message.Release.Publication.Topic, topic);
            return context.Topic.Update(topic).Entity;
        }

        private Theme CreateOrUpdateTheme(ImportMessage message, StatisticsDbContext context)
        {
            var theme = context.Theme
                .FirstOrDefault(t => t.Id.Equals(message.Release.Publication.Topic.Theme.Id));

            if (theme == null)
            {
                theme = _mapper.Map<Theme>(message.Release.Publication.Topic.Theme);
                return context.Theme.Add(theme).Entity;
            }

            theme = _mapper.Map(message.Release.Publication.Topic.Theme, theme);
            return context.Theme.Update(theme).Entity;
        }
    }
}