using System;
using System.Linq;
using System.Threading;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Publication = GovUk.Education.ExploreEducationStatistics.Data.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Data.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Data.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ReleaseProcessorService : IReleaseProcessorService
    {
        private readonly IMapper _mapper;

        public ReleaseProcessorService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Subject CreateOrUpdateRelease(SubjectData subjectData, ImportMessage message,
            StatisticsDbContext statisticsDbContext, ContentDbContext contentDbContext)
        {
            // Avoid potential collisions
            Thread.Sleep(new Random().Next(1, 5) * 1000);

            var release = CreateOrUpdateRelease(message, statisticsDbContext);
            RemoveSubjectIfExisting(message, statisticsDbContext);

            var subject = CreateSubject(message.SubjectId, subjectData.DataBlob.FileName, subjectData.Name, release,
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
                                  && rfr.ReleaseFileType == ReleaseFileTypes.Metadata
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

        private Release CreateOrUpdateRelease(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            Release release;

            if (!statisticsDbContext.Release.Any(r => r.Id.Equals(message.Release.Id)))
            {
                release = new Release
                {
                    Id = message.Release.Id,
                    Slug = message.Release.Slug,
                    Publication = CreateOrUpdatePublication(message, statisticsDbContext),
                    TimeIdentifier = message.Release.TimeIdentifier,
                    Year = message.Release.Year,
                    PreviousVersionId = message.Release.PreviousVersionId
                };

                release = statisticsDbContext.Release.Add(release).Entity;
            }
            else
            {
                release = _mapper.Map(message.Release, (Release) null);
                release = statisticsDbContext.Release.Update(release).Entity;
            }
            statisticsDbContext.SaveChanges();
            return release;
        }

        private Publication CreateOrUpdatePublication(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            Publication publication;

            if (!statisticsDbContext.Publication.Any(p => p.Id.Equals(message.Release.Publication.Id)))
            {
                publication = new Publication
                {
                    Id = message.Release.Publication.Id,
                    Title = message.Release.Publication.Title,
                    Slug = message.Release.Publication.Slug,
                    Topic = CreateOrUpdateTopic(message, statisticsDbContext)
                };
                publication = statisticsDbContext.Publication.Add(publication).Entity;
            }
            else
            {
                publication = _mapper.Map(message.Release.Publication, (Publication) null);
                publication = statisticsDbContext.Publication.Update(publication).Entity;
            }
            statisticsDbContext.SaveChanges();
            return publication;
        }

        private Topic CreateOrUpdateTopic(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            Topic topic;

            if (!statisticsDbContext.Topic.Any(t => t.Id.Equals(message.Release.Publication.Topic.Id)))
            {
                topic = new Topic
                {
                    Id = message.Release.Publication.Topic.Id,
                    Title = message.Release.Publication.Topic.Title,
                    Slug = message.Release.Publication.Topic.Slug,
                    Theme = CreateOrUpdateTheme(message, statisticsDbContext)
                };
                topic = statisticsDbContext.Topic.Add(topic).Entity;
            }
            else
            {
                topic = _mapper.Map(message.Release.Publication.Topic, (Topic) null);
                topic = statisticsDbContext.Topic.Update(topic).Entity;
            }
            statisticsDbContext.SaveChanges();
            return topic;
        }

        private Theme CreateOrUpdateTheme(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            Theme theme;
            if (!statisticsDbContext.Theme
                .Any(t => t.Id.Equals(message.Release.Publication.Topic.Theme.Id)))
            {
                theme = new Theme
                {
                    Id = message.Release.Publication.Topic.Theme.Id,
                    Slug = message.Release.Publication.Topic.Theme.Slug,
                    Title = message.Release.Publication.Topic.Theme.Title
                };
                theme = statisticsDbContext.Theme.Add(theme).Entity;
            }
            else
            {
                theme = _mapper.Map(message.Release.Publication.Topic.Theme, (Theme) null);
                theme = statisticsDbContext.Theme.Update(theme).Entity;
            }
            statisticsDbContext.SaveChanges();
            return theme;
        }
    }
}