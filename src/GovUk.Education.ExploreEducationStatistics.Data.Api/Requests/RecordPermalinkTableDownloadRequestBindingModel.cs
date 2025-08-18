#nullable enable
using System;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;

public record RecordPermalinkTableDownloadRequestBindingModel
{
    public string? PermalinkTitle { get; init; }
    public Guid? PermalinkId { get; init; }
    public TableDownloadFormat? DownloadFormat { get; init; }

    public class Validator : AbstractValidator<RecordPermalinkTableDownloadRequestBindingModel>
    {
        public Validator()
        {
            RuleFor(dto => dto.PermalinkTitle).NotNull().NotEmpty();
            RuleFor(dto => dto.PermalinkId).NotNull().NotEqual(Guid.Empty);
            RuleFor(dto => dto.DownloadFormat).NotNull().IsInEnum();
        }
    }

    public CapturePermaLinkTableDownloadCall ToModel()
    {
        return new CapturePermaLinkTableDownloadCall
        {
            PermalinkTitle = PermalinkTitle,
            PermalinkId = PermalinkId!.Value,
            DownloadFormat = DownloadFormat!.Value
        };
    }
}
