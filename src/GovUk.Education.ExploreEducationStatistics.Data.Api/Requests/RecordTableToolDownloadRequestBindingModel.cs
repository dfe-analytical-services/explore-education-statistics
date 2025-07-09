#nullable enable
using System;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;

public record RecordTableToolDownloadRequestBindingModel
{
    public Guid? ReleaseVersionId { get; init; }
    public string? PublicationName { get; init; }
    public string? ReleasePeriodAndLabel { get; init; }
    public Guid? SubjectId { get; init; }
    public string? DataSetName { get; init; }
    public TableDownloadFormat? DownloadFormat { get; init; }
    public FullTableQuery? Query { get; init; }
    
    public class Validator : AbstractValidator<RecordTableToolDownloadRequestBindingModel>
    {
        public Validator()
        {
            RuleFor(dto => dto.ReleaseVersionId).NotNull().NotEmpty();
            RuleFor(dto => dto.PublicationName).NotNull().NotEmpty();
            RuleFor(dto => dto.ReleasePeriodAndLabel).NotNull().NotEmpty();
            RuleFor(dto => dto.SubjectId).NotNull().NotEmpty();
            RuleFor(dto => dto.DataSetName).NotNull().NotEmpty();
            RuleFor(dto => dto.DownloadFormat).NotNull();
            RuleFor(dto => dto.Query).NotNull().NotEmpty();
        }
    }

    public CaptureTableToolDownloadCall ToModel()
    {
        return new CaptureTableToolDownloadCall
        {
            ReleaseVersionId = ReleaseVersionId!.Value,
            PublicationName = PublicationName,
            ReleasePeriodAndLabel = ReleasePeriodAndLabel,
            SubjectId = SubjectId!.Value,
            DataSetName = DataSetName,
            DownloadFormat = DownloadFormat!.Value,
            Query = Query
        };
    }
}
