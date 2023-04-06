#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;


namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IFrontendService
{
    Task<Stream> CreateUniversalTableFormat(PermalinkTableCreateRequest request, CancellationToken cancellationToken);
}