using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using UserId = System.Guid;
using TopicId = System.Guid;

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

        // TODO it maybe necessary to add authorisation to this method?
        public List<Publication> GetByTopicAndUser(TopicId topicId, UserId userId)
        {
            // TODO This method simply returns all Publications for a Topic as we currently do not have a concept of how
            // TODO a user is connected to Publications for the purpose of administration. Once this has been modelled
            // TODO then this method will need altered reflect this.
            return _context.Publications.Select(p => new Publication
                {
                    Id = p.Id,
                    Title = p.Title,
                    NextUpdate = p.NextUpdate, // Null | Partial Date | Complete Date
                    Contact = p.Contact != null ? new Contact
                    {
                        Id = p.Contact.Id,
                        ContactName = p.Contact.ContactName,
                        TeamEmail = p.Contact.TeamEmail,
                        ContactTelNo = p.Contact.ContactTelNo
                    } : null,
                    TopicId = p.TopicId,
                    Methodologies = p.Methodologies.Select(m => new Methodology
                    {
                        Id = m.Id,
                        Title = m.Title
                    }).ToList(),
                    Releases = p.Releases.Select(r => new Release
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Published = r.Published,
                            ReleaseName = r.ReleaseName
                            // TODO Status
                            // TODO Live? Is this Status? Or a bool or something else? - A question for design
                            // TODO isLatestRelease? latest published? - A question for design
                            // TODO timePeriodCoverage
                            // TODO LastEdited - Auditing
                        }
                        
                    ).ToList()
                }
            ).Where(p => p.TopicId == topicId).ToList();
        }
    }
}