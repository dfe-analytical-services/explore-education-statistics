using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IValidationService
    {
        Task<bool> ValidateAsync(ValidateReleaseMessage message);
    }
}