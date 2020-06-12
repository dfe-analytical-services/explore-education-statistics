using System;
using System.Linq;
using System.Threading;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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

        public Subject CreateOrUpdateRelease(SubjectData subjectData, ImportMessage message,
            StatisticsDbContext context, ContentDbContext contentDbContext)
        {
            // Avoid potential collisions
            Thread.Sleep(new Random().Next(1, 5) * 1000);
            
            var release = CreateOrUpdateRelease(message, context);
            RemoveSubject(subjectData.Name, release, context);
            
            var subject = CreateSubject(message.SubjectId, subjectData.Name, release, context);
            CreateReleaseFileLink(message, contentDbContext, release, subject);
            
            return subject;
        }

        private static void CreateReleaseFileLink(ImportMessage message, ContentDbContext contentDbContext, Release release,
            Subject subject)
        {
            var releaseFileLink = contentDbContext
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .FirstOrDefault(f => f.ReleaseId == release.Id && f.ReleaseFileReference.Filename == message.DataFileName);

            if (releaseFileLink != null)
            {
                releaseFileLink.ReleaseFileReference.SubjectId = subject.Id;
                contentDbContext.Update(releaseFileLink);
                contentDbContext.SaveChanges();
            }
        }

        private void RemoveSubject(string name, Release release, StatisticsDbContext context)
        {
            var releaseSubject = context.ReleaseSubject
                .FirstOrDefault(r => r.Subject.Name == name && r.ReleaseId == release.Id);
            
            // If the subject exists then this must be a reload of the same release/subject so delete & re-create.
            if (releaseSubject != null)
            {
                context.ReleaseSubject.Remove(releaseSubject);
            }
            
            var subject = context.Subject.FirstOrDefault(s => s.Name == name);

            if (subject != null)
            {
                context.Subject.Remove(subject);
            }
            
            context.SaveChanges();
        }

        private Subject CreateSubject(Guid subjectId, string name, Release release, StatisticsDbContext context)
        {
            var newSubject = context.Subject.Add(new Subject
                {
                    Id = subjectId,
                    Name = name,
                    // TODO BAU-384 - remove after this work goes out
                    ReleaseId = release.Id
                }
            ).Entity;
            
            context.ReleaseSubject.Add(
                new ReleaseSubject
                {
                    ReleaseId = release.Id,
                    SubjectId = subjectId
                });
            
            context.SaveChanges();

            return newSubject;
        }

        private Release CreateOrUpdateRelease(ImportMessage message, StatisticsDbContext context)
        {
            Release release;

            if (!context.Release.Any((r => r.Id.Equals(message.Release.Id))))
            {
                release = new Release
                {
                    Id = message.Release.Id,
                    Slug = message.Release.Slug,
                    Publication = CreateOrUpdatePublication(message, context),
                    TimeIdentifier = message.Release.TimeIdentifier,
                    Year = message.Release.Year
                };

                release = context.Release.Add(release).Entity;
            }
            else
            {
                release = _mapper.Map(message.Release, (Release) null);
                release = context.Release.Update(release).Entity;
            }
            context.SaveChanges();
            return release;
        }

        private Publication CreateOrUpdatePublication(ImportMessage message, StatisticsDbContext context)
        {
            Publication publication;

            if (!context.Publication.Any(p => p.Id.Equals(message.Release.Publication.Id)))
            {
                publication = new Publication
                {
                    Id = message.Release.Publication.Id,
                    Title = message.Release.Publication.Title,
                    Slug = message.Release.Publication.Slug,
                    Topic = CreateOrUpdateTopic(message, context)
                };
                publication = context.Publication.Add(publication).Entity;
            }
            else
            {
                publication = _mapper.Map(message.Release.Publication, (Publication) null);
                publication = context.Publication.Update(publication).Entity;
            }
            context.SaveChanges();
            return publication;
        }

        private Topic CreateOrUpdateTopic(ImportMessage message, StatisticsDbContext context)
        {
            Topic topic;
            
            if (!context.Topic.Any(t => t.Id.Equals(message.Release.Publication.Topic.Id)))
            {
                topic = new Topic
                {
                    Id = message.Release.Publication.Topic.Id,
                    Title = message.Release.Publication.Topic.Title,
                    Slug = message.Release.Publication.Topic.Slug,
                    Theme = CreateOrUpdateTheme(message, context)
                };
                topic = context.Topic.Add(topic).Entity;
            }
            else
            {
                topic = _mapper.Map(message.Release.Publication.Topic, (Topic) null);
                topic = context.Topic.Update(topic).Entity;
            }
            context.SaveChanges();
            return topic;
        }

        private Theme CreateOrUpdateTheme(ImportMessage message, StatisticsDbContext context)
        {
            Theme theme;
            if (!context.Theme
                .Any(t => t.Id.Equals(message.Release.Publication.Topic.Theme.Id)))
            {
                theme = new Theme
                {
                    Id = message.Release.Publication.Topic.Theme.Id,
                    Slug = message.Release.Publication.Topic.Theme.Slug,
                    Title = message.Release.Publication.Topic.Theme.Title
                };
                theme = context.Theme.Add(theme).Entity;
            }
            else
            {
                theme = _mapper.Map(message.Release.Publication.Topic.Theme, (Theme) null);
                theme = context.Theme.Update(theme).Entity;  
            }
            context.SaveChanges();
            return theme;
        }
    }
}