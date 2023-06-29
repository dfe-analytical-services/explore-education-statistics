#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.FilterMigrationReportErrorItem;
using static GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.FilterMigrationReportInfoItem;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record FilterMigrationReport
{
    public List<Guid> FiltersUpdated { get; } = new();
    public List<FilterMigrationReportInfoItem> Information { get; } = new();
    public List<FilterMigrationReportErrorItem> Errors { get; } = new();

    public void AddExceptionGettingMetaFilters(Guid subjectId,
        File file,
        Exception exception)
    {
        Errors.Add(ExceptionGettingMetaFilters(subjectId, file, exception.Message));
    }

    public void AddAllFiltersHaveGroupCsvColumnValues(Guid subjectId)
    {
        Information.Add(AllFiltersHaveGroupCsvColumnValues(subjectId));
    }

    public void AddFilterAlreadyHasGroupCsvColumnValue(Guid subjectId,
        File file,
        Filter filter)
    {
        Information.Add(FilterAlreadyHasGroupCsvColumnValue(subjectId, file, filter));
    }

    public void AddMetaFileNotFound(Guid subjectId)
    {
        Errors.Add(MetaFileNotFound(subjectId));
    }

    public void AddMetaFilterNotFound(Guid subjectId,
        File file,
        Filter filter)
    {
        Errors.Add(MetaFilterNotFound(subjectId, file, filter));
    }

    public void AddFilterLabelMismatch(Guid subjectId,
        File file,
        Filter filter)
    {
        Errors.Add(FilterLabelMismatch(subjectId, file, filter));
    }
}

public record FilterMigrationReportInfoItem(FilterMigrationReportInfo Value,
    Guid SubjectId,
    Guid? FilterId = null,
    string? FilterName = null,
    Guid? FileId = null,
    string? FilePath = null)
{
    private FilterMigrationReportInfoItem(FilterMigrationReportInfo value,
        Guid subjectId,
        Filter? filter = null,
        File? file = null) : this(
        value,
        SubjectId: subjectId,
        FilterId: filter?.Id,
        FilterName: filter?.Name,
        FileId: file?.Id,
        FilePath: file?.Path())
    {
    }

    public override string ToString()
    {
        var s = $"{Value} ({nameof(SubjectId)}={SubjectId}";

        if (FilterId.HasValue)
        {
            s += $", {nameof(FilterId)}={FilterId}";
        }

        if (FilterName != null)
        {
            s += $", {nameof(FilterName)}={FilterName}";
        }

        if (FileId.HasValue)
        {
            s += $", {nameof(FileId)}={FileId}";
        }

        if (FilePath != null)
        {
            s += $", {nameof(FilePath)}={FilePath}";
        }

        return s + ")";
    }

    public static FilterMigrationReportInfoItem AllFiltersHaveGroupCsvColumnValues(
        Guid subjectId)
    {
        return new FilterMigrationReportInfoItem(
            value: FilterMigrationReportInfo.AllFiltersHaveGroupCsvColumnValues,
            subjectId: subjectId);
    }

    public static FilterMigrationReportInfoItem FilterAlreadyHasGroupCsvColumnValue(
        Guid subjectId,
        File file,
        Filter filter)
    {
        return new FilterMigrationReportInfoItem(
            value: FilterMigrationReportInfo.FilterAlreadyHasGroupCsvColumnValue,
            subjectId: subjectId,
            file: file,
            filter: filter);
    }
}

public record FilterMigrationReportErrorItem(FilterMigrationReportError Value,
    Guid SubjectId,
    Guid? FilterId = null,
    string? FilterName = null,
    Guid? FileId = null,
    string? FilePath = null,
    string? Exception = null)
{
    public FilterMigrationReportErrorItem(FilterMigrationReportError value,
        Guid subjectId,
        Filter? filter = null,
        File? file = null,
        string? exception = null) : this(
        value,
        SubjectId: subjectId,
        FilterId: filter?.Id,
        FilterName: filter?.Name,
        FileId: file?.Id,
        FilePath: file?.Path(),
        Exception: exception)
    {
    }

    public override string ToString()
    {
        var s = $"{Value} ({nameof(SubjectId)}={SubjectId}";

        if (FilterId.HasValue)
        {
            s += $", {nameof(FilterId)}={FilterId}";
        }

        if (FilterName != null)
        {
            s += $", {nameof(FilterName)}={FilterName}";
        }

        if (FileId.HasValue)
        {
            s += $", {nameof(FileId)}={FileId}";
        }

        if (FilePath != null)
        {
            s += $", {nameof(FilePath)}={FilePath}";
        }

        if (Exception != null)
        {
            s += $", {nameof(Exception)}={Exception}";
        }

        return s + ")";
    }

    public static FilterMigrationReportErrorItem MetaFileNotFound(Guid subjectId)
    {
        return new FilterMigrationReportErrorItem(
            value: FilterMigrationReportError.MetaFileNotFound,
            subjectId: subjectId);
    }

    public static FilterMigrationReportErrorItem MetaFilterNotFound(Guid subjectId,
        File file,
        Filter filter)
    {
        return new FilterMigrationReportErrorItem(
            value: FilterMigrationReportError.MetaFilterNotFound,
            subjectId: subjectId,
            file: file,
            filter: filter);
    }

    public static FilterMigrationReportErrorItem FilterLabelMismatch(Guid subjectId,
        File file, Filter filter)
    {
        return new FilterMigrationReportErrorItem(
            value: FilterMigrationReportError.FilterLabelMismatch,
            subjectId: subjectId,
            file: file,
            filter: filter);
    }

    public static FilterMigrationReportErrorItem ExceptionGettingMetaFilters(Guid subjectId,
        File file,
        string exception)
    {
        return new FilterMigrationReportErrorItem(
            value: FilterMigrationReportError.ExceptionGettingMetaFilters,
            subjectId: subjectId,
            file: file,
            exception: exception);
    }
}

public enum FilterMigrationReportInfo
{
    AllFiltersHaveGroupCsvColumnValues,
    FilterAlreadyHasGroupCsvColumnValue,
}

public enum FilterMigrationReportError
{
    MetaFileNotFound,
    ExceptionGettingMetaFilters,
    MetaFilterNotFound,
    FilterLabelMismatch,
}
