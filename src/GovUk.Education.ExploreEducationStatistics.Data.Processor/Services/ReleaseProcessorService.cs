using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils;
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

        private async Task<Release> CreateOrUpdateRelease(ImportMessage message,
            StatisticsDbContext statisticsDbContext, Publication publication)
        {
            return await DbUtils.ExecuteWithExclusiveLock(statisticsDbContext, "CreateOrUpdateRelease", async (context) =>
            {
                Release release;

                _logger.LogInformation($"Checking for existence of Release {message.Release.Slug}");

                var existing = await statisticsDbContext.Release.FindAsync(message.Release.Id);

                if (existing == null)
                {
                    release = new Release
                    {
                        Id = message.Release.Id,
                        Slug = message.Release.Slug,
                        Publication = publication,
                        TimeIdentifier = message.Release.TimeIdentifier,
                        Year = message.Release.Year,
                        PreviousVersionId = message.Release.PreviousVersionId
                    };
                    _logger.LogInformation($"Creating Release {release.Slug}");
                    release = (await statisticsDbContext.Release.AddAsync(release)).Entity;
                }
                else
                {
                    release = _mapper.Map(message.Release, existing);
                    _logger.LogInformation($"Release {release.Slug} already exists - updating");
                    release = statisticsDbContext.Release.Update(release).Entity;
                }

                await statisticsDbContext.SaveChangesAsync();
                return release;
            });
        }

        private async Task<Publication> CreateOrUpdatePublication(ImportMessage message,
            StatisticsDbContext statisticsDbContext, Topic topic)
        {
            return await DbUtils.ExecuteWithExclusiveLock(statisticsDbContext, "CreateOrUpdatePublication", async (context) =>
            {
                Publication publication;

                _logger.LogInformation($"Checking for existence of Publication {message.Release.Publication.Title}");

                var existing = await statisticsDbContext.Publication.FindAsync(message.Release.Publication.Id);

                if (existing == null)
                {
                    publication = new Publication
                    {
                        Id = message.Release.Publication.Id,
                        Title = message.Release.Publication.Title,
                        Slug = message.Release.Publication.Slug,
                        Topic = topic
                    };
                    _logger.LogInformation($"Creating Publication {publication.Title}");
                    publication = (await statisticsDbContext.Publication.AddAsync(publication)).Entity;
                }
                else
                {
                    publication = _mapper.Map(message.Release.Publication, existing);
                    _logger.LogInformation($"Publication {publication.Title} already exists - updating");
                    publication = statisticsDbContext.Publication.Update(publication).Entity;
                }

                await statisticsDbContext.SaveChangesAsync();
                return publication;
            });
        }

        private async Task<Topic> CreateOrUpdateTopic(ImportMessage message, StatisticsDbContext statisticsDbContext,
            Theme theme)
        {
            return await DbUtils.ExecuteWithExclusiveLock(statisticsDbContext, "CreateOrUpdateTopic", async (context) =>
            {
                Topic topic;

                _logger.LogInformation($"Checking for existence of Topic {message.Release.Publication.Topic.Title}");

                var existing = await statisticsDbContext.Topic.FindAsync(message.Release.Publication.Topic.Id);

                if (existing == null)
                {
                    topic = new Topic
                    {
                        Id = message.Release.Publication.Topic.Id,
                        Title = message.Release.Publication.Topic.Title,
                        Slug = message.Release.Publication.Topic.Slug,
                        Theme = theme
                    };
                    _logger.LogInformation($"Creating Topic {topic.Title}");
                    topic = (await statisticsDbContext.Topic.AddAsync(topic)).Entity;
                }
                else
                {
                    topic = _mapper.Map(message.Release.Publication.Topic, existing);
                    _logger.LogInformation($"Topic {topic.Title} already exists - updating");
                    topic = statisticsDbContext.Topic.Update(topic).Entity;
                }

                await statisticsDbContext.SaveChangesAsync();
                return topic;
            });
        }

        private Task<Theme> CreateOrUpdateTheme(ImportMessage message, StatisticsDbContext statisticsDbContext)
        {
            return DbUtils.ExecuteWithExclusiveLock(statisticsDbContext, "CreateOrUpdateTheme", async (context) =>
            {
                Theme theme;

                _logger.LogInformation(
                    $"Checking for existence of Theme {message.Release.Publication.Topic.Theme.Title}");

                var existing = await statisticsDbContext.Theme
                    .FindAsync(message.Release.Publication.Topic.Theme.Id);
                
                if (existing == null)
                {
                    theme = new Theme
                    {
                        Id = message.Release.Publication.Topic.Theme.Id,
                        Slug = message.Release.Publication.Topic.Theme.Slug,
                        Title = message.Release.Publication.Topic.Theme.Title
                    };
                    _logger.LogInformation($"Creating Theme {theme.Title}");
                    theme = (await statisticsDbContext.Theme.AddAsync(theme)).Entity;
                }
                else
                {
                    theme = _mapper.Map(message.Release.Publication.Topic.Theme, existing);
                    _logger.LogInformation($"Theme {theme.Title} already exists - updating");
                    theme = statisticsDbContext.Theme.Update(theme).Entity;
                }

                await statisticsDbContext.SaveChangesAsync();
                return theme;
            });
        }
    }
}