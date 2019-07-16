using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using UserId = System.Guid;
using TopicId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublicationService
    {
        List<Publication> List();

        Publication Get(Guid id);

        Publication Get(string slug);

        List<Publication> GetByTopicAndUser(TopicId topicId, UserId userId);
    }
}
