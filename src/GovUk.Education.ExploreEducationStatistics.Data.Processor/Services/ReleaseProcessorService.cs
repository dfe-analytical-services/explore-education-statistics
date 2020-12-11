using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using Publication = GovUk.Education.ExploreEducationStatistics.Data.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Data.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Data.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ReleaseProcessorService : IReleaseProcessorService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ReleaseProcessorService> _logger;

        public ReleaseProcessorService(IMapper mapper, ILogger<ReleaseProcessorService> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public Subject CreateOrUpdateRelease(SubjectData subjectData, ImportMessage message,
            StatisticsDbContext statisticsDbContext, ContentDbContext contentDbContext)
        {
            var theme = CreateOrUpdateTheme(message, statisticsDbContext).Result;
            var topic = CreateOrUpdateTopic(message, statisticsDbContext, theme).Result;
            var publication = CreateOrUpdatePublication(message, statisticsDbContext, topic).Result;
            var release = CreateOrUpdateRelease(message, statisticsDbContext, publication).Result;

            RemoveSubjectIfExisting(message, statisticsDbContext);

            var subject = CreateSubject(message.SubjectId,
                subjectData.DataBlob.FileName,
                subjectData.Name,
                release,
                statisticsDbContext);

            if (!UpdateReleaseFileReferenceLinks(message, contentDbContext, release, subject))
            {
                throw new Exception(
                    "Unable to create release file links when importing : Check file references are correct");
            }

            return subject;
        }

        private static bool UpdateReleaseFileReferenceLinks(ImportMessage message, ContentDbContext contentDbContext, Release release,
            Subject subject)
        {
            var releaseFileLinks = contentDbContext
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == release.Id);

            // Make sure the filename predicate is case sensitive by executing in memory rather than in the db
            var releaseDataFileLink = releaseFileLinks.ToList()
                .FirstOrDefault(file => file.ReleaseFileReference.Filename == message.DataFileName);

            if (releaseDataFileLink != null)
            {
                releaseDataFileLink.ReleaseFileReference.SubjectId = subject.Id;
                contentDbContext.Update(releaseDataFileLink);

                var associatedMetaReference = contentDbContext.ReleaseFileReferences
                    .FirstOrDefault(rfr => rfr.ReleaseId == releaseDataFileLink.ReleaseFileReference.ReleaseId
                                  && rfr.ReleaseFileType == Metadata
                                  && rfr.Filename == message.MetaFileName);

                if (associatedMetaReference == null)
                {
                    return false;
                }

                associatedMetaReference.SubjectId = subject.Id;
                contentDbContext.Update(associatedMetaReference);

                contentDbContext.SaveChanges();
                return true;
            }

            return false;
        }

        private static void RemoveSubjectIfExisting(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            var releaseSubject = statisticsDbContext.ReleaseSubject
                .Include(rs => rs.Subject)
                .FirstOrDefault(r => r.Subject.Id == message.SubjectId && r.ReleaseId == message.Release.Id);

            // If the subject exists then this must be a reload of the same release/subject so delete & re-create.
            if (releaseSubject != null)
            {
                var subject = releaseSubject.Subject;
                statisticsDbContext.ReleaseSubject.Remove(releaseSubject);
                statisticsDbContext.Subject.Remove(subject);
                statisticsDbContext.SaveChanges();
            }
        }

        private static Subject CreateSubject(Guid subjectId,
            string filename,
            string name,
            Release release,
            StatisticsDbContext statisticsDbContext)
        {
            var newSubject = statisticsDbContext.Subject.Add(new Subject
                {
                    Id = subjectId,
                    Filename = filename,
                    Name = name
                }
            ).Entity;

            statisticsDbContext.ReleaseSubject.Add(
                new ReleaseSubject
                {
                    ReleaseId = release.Id,
                    SubjectId = subjectId
                });

            statisticsDbContext.SaveChanges();

            return newSubject;
        }

        private Task<Release> CreateOrUpdateRelease(ImportMessage message,
            StatisticsDbContext statisticsDbContext, Publication publication)
        {
            return CreateOrUpdateWithExclusiveLock(
                $"Release {message.Release.Slug} (for Publication \"{message.Release.Publication.Title}\")",
                statisticsDbContext,
                message.Release,
                message.Release.Id,
                new Release
                    {
                        Id = message.Release.Id,
                        Slug = message.Release.Slug,
                        Publication = publication,
                        TimeIdentifier = message.Release.TimeIdentifier,
                        Year = message.Release.Year,
                        PreviousVersionId = message.Release.PreviousVersionId
                    });
        }

        private Task<Publication> CreateOrUpdatePublication(ImportMessage message,
            StatisticsDbContext statisticsDbContext, Topic topic)
        {
            return CreateOrUpdateWithExclusiveLock(
                message.Release.Publication.Title,
                statisticsDbContext,
                message.Release.Publication,
                message.Release.Publication.Id,
                new Publication
                    {
                        Id = message.Release.Publication.Id,
                        Title = message.Release.Publication.Title,
                        Slug = message.Release.Publication.Slug,
                        Topic = topic
                    });
        }

        private Task<Topic> CreateOrUpdateTopic(ImportMessage message, StatisticsDbContext statisticsDbContext,
            Theme theme)
        {
            return CreateOrUpdateWithExclusiveLock(
                message.Release.Publication.Topic.Title,
                statisticsDbContext,
                message.Release.Publication.Topic,
                message.Release.Publication.Topic.Id,
                new Topic
                    {
                        Id = message.Release.Publication.Topic.Id,
                        Title = message.Release.Publication.Topic.Title,
                        Slug = message.Release.Publication.Topic.Slug,
                        Theme = theme
                    });
        }

        private Task<Theme> CreateOrUpdateTheme(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            return CreateOrUpdateWithExclusiveLock(
                message.Release.Publication.Topic.Theme.Title,
                statisticsDbContext,
                message.Release.Publication.Topic.Theme,
                message.Release.Publication.Topic.Theme.Id,
                new Theme 
                {
                    Id = message.Release.Publication.Topic.Theme.Id,
                    Slug = message.Release.Publication.Topic.Theme.Slug,
                    Title = message.Release.Publication.Topic.Theme.Title
                });
        }
        
        /**
         * This method allows for a concurrency-safe pattern to check for the existence of a given entity and, if it
         * does not exist to create it, or if it does exist, to update it.
         *
         * This is concurrency-safe as it ensures that any two concurrent importer processes that are trying to create
         * or update the same entity type with the same ID will not be able to do so at the same time.  Rather, the
         * first thread to reach this code will acquire a lock and prevent the second thread from being able to check
         * for the existence of the entity until the first thread has finished creating it.  At this point, the second
         * thread is freed up to check if the entity already exists, which by this point, it will.
         */
        private Task<TEntity> CreateOrUpdateWithExclusiveLock<TEntity, TMessageObject>(
            string entityName,
            StatisticsDbContext statisticsDbContext,
            TMessageObject objectFromMessage,
            Guid entityId,
            TEntity entityToCreate)
            where TEntity : class 
            where TMessageObject : class
        {
            var typeName = typeof(TEntity).Name;
            
            return DbUtils.ExecuteWithExclusiveLock(
                statisticsDbContext, 
                $"CreateOrUpdate{typeName}-{entityId}", 
                async (context) =>
            {
                _logger.LogInformation(
                    $"Checking for existence of {typeName} \"{entityName}\"");

                var existing = await statisticsDbContext.Set<TEntity>().FindAsync(entityId);
                
                if (existing == null)
                {
                    _logger.LogInformation($"Creating {typeName} \"{entityName}\"");
                    TEntity created = (await statisticsDbContext.Set<TEntity>().AddAsync(entityToCreate)).Entity;
                    await statisticsDbContext.SaveChangesAsync();
                    return created;
                }
                
                _logger.LogInformation($"{typeName} \"{entityName}\" already exists - updating");
                TEntity mappedEntity = _mapper.Map(objectFromMessage, existing);
                TEntity updated = statisticsDbContext.Set<TEntity>().Update(mappedEntity).Entity;
                await statisticsDbContext.SaveChangesAsync();
                return updated;
            });
        }
    }
}