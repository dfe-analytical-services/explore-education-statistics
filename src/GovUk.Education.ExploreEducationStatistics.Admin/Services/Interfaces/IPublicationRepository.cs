#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublicationRepository : Content.Model.Repository.Interfaces.IPublicationRepository
{
    IQueryable<Publication> QueryPublicationsForTheme(Guid? themeId = null);

    Task<List<Publication>> ListPublicationsForUser(Guid userId, Guid? themeId = null);
}
