#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;
using System.Collections.Generic;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

/// <summary>
/// Represents an unvalidated data set.
/// </summary>
public record DataSetDto
{
    public Guid ReleaseVersionId { get; init; }

    public string Title { get; init; } = string.Empty;

    public FileDto? DataFile { get; init; }

    public FileDto? MetaFile { get; init; }

    public File? ReplacingFile { get; init; }

    public class Validator : AbstractValidator<DataSetDto>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(dto => dto.ReleaseVersionId)
                .NotEmpty();

            RuleFor(dto => dto.Title)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong, "{PropertyValue}", "{MaxLength}");

            RuleFor(dto => dto.DataFile)
                .MustBeValidDataFile();

            RuleFor(dto => dto.MetaFile)
                .MustBeValidMetaFile();

            // TODO: Test this with a random type, then add to ValidationMessages (inc. a frontend mapping for the Code)
            RuleFor(dto => dto.ReplacingFile)
                .Must(file => file is null || file.Type == FileType.Data)
                    .WithMessage("replacingFile.Type should equal FileType.Data");
        }
    }
}

/// <summary>
/// Represents a validated data set.
/// </summary>
public record DataSet
{
    public required string Title { get; init; }

    public required FileDto DataFile { get; init; }

    public required FileDto MetaFile { get; init; }

    public File? ReplacingFile { get; init; }
}

public record DataSetIndex()
{
    public required Guid ReleaseVersionId { get; init; }

    public List<DataSetIndexItem> DataSetIndexItems { get; init; } = [];
}

public record DataSetIndexItem
{
    public required string DataSetTitle { get; init; }

    public required string DataFileName { get; init; }

    public required string MetaFileName { get; init; }

    public File? ReplacingFile { get; init; }
}
