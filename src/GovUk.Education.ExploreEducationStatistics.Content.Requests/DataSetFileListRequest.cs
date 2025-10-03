using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record DataSetFileListRequest(
    Guid? ThemeId = null,
    Guid? PublicationId = null,
    Guid? ReleaseId = null,
    string? GeographicLevel = null,
    bool? LatestOnly = null,
    DataSetType? DataSetType = null,
    string? SearchTerm = null,
    DataSetsListRequestSortBy? Sort = null,
    SortDirection? SortDirection = null,
    int Page = 1,
    int PageSize = 10
)
{
    public GeographicLevel? GeographicLevelEnum =>
        GeographicLevel == null ? null : EnumUtil.GetFromEnumValue<GeographicLevel>(GeographicLevel);

    public class Validator : AbstractValidator<DataSetFileListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.SearchTerm).MinimumLength(3);
            RuleFor(request => request.ReleaseId)
                .NotEmpty()
                .When(request => request.Sort == DataSetsListRequestSortBy.Natural);
            RuleFor(request => request.GeographicLevel)
                .AllowedValue(EnumUtil.GetEnumValues<GeographicLevel>())
                .When(request => request.GeographicLevel != null);
            RuleFor(request => request.SearchTerm)
                .NotEmpty()
                .When(request => request.Sort == DataSetsListRequestSortBy.Relevance);
            RuleFor(request => request.Page).InclusiveBetween(1, 9999);
            RuleFor(request => request.PageSize).InclusiveBetween(1, 40);
        }
    }
}
