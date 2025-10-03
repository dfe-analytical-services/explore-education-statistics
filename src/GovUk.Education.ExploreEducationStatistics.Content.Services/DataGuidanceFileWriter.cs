using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class DataGuidanceFileWriter : IDataGuidanceFileWriter
{
    private const string VariableSeparator = "  |  ";

    private readonly ContentDbContext _contentDbContext;
    private readonly IDataGuidanceDataSetService _dataGuidanceDataSetService;

    public DataGuidanceFileWriter(
        ContentDbContext contentDbContext,
        IDataGuidanceDataSetService dataGuidanceDataSetService
    )
    {
        _contentDbContext = contentDbContext;
        _dataGuidanceDataSetService = dataGuidanceDataSetService;
    }

    public async Task<Stream> WriteToStream(
        Stream stream,
        ReleaseVersion releaseVersion,
        IList<Guid>? dataFileIds = null
    )
    {
        // Make sure publication has been hydrated
        await _contentDbContext.Entry(releaseVersion).Reference(rv => rv.Publication).LoadAsync();

        var dataSets = await ListDataSets(releaseVersion, dataFileIds);

        await using var file = new StreamWriter(stream, leaveOpen: stream.CanRead);

        await DoWrite(file, releaseVersion, dataSets);

        await file.FlushAsync();

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return stream;
    }

    private async Task<List<DataGuidanceDataSetViewModel>> ListDataSets(
        ReleaseVersion releaseVersion,
        IList<Guid>? dataFileIds = null
    )
    {
        var dataSets = await _dataGuidanceDataSetService.ListDataSets(releaseVersion.Id, dataFileIds);

        if (dataSets.IsLeft)
        {
            throw new ArgumentException($"Could not find data sets for release version: {releaseVersion.Id}");
        }

        return dataSets.Right;
    }

    private static async Task DoWrite(
        TextWriter file,
        ReleaseVersion releaseVersion,
        IList<DataGuidanceDataSetViewModel> dataSets
    )
    {
        // Add header information including publication/release title
        await file.WriteLineAsync(releaseVersion.Release.Publication.Title);
        await file.WriteLineAsync(releaseVersion.Release.Title);

        if (!releaseVersion.DataGuidance.IsNullOrWhitespace())
        {
            await file.WriteLineAsync();

            // Add the release's guidance content
            var guidance = await HtmlToTextUtils.HtmlToText(releaseVersion.DataGuidance);
            await file.WriteAsync(guidance);
            await file.WriteLineAsync();
        }

        await WriteDataFiles(file, dataSets);
    }

    private static async Task WriteDataFiles(TextWriter file, IList<DataGuidanceDataSetViewModel> dataSets)
    {
        if (dataSets.Count == 0)
        {
            return;
        }

        // Add 'Data files' section
        await file.WriteLineAsync();
        await file.WriteLineAsync("Data files");
        await file.WriteLineAsync();

        await dataSets
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async (dataSet, index) =>
                {
                    await file.WriteLineAsync(dataSet.Name);
                    await file.WriteLineAsync();

                    await file.WriteLineAsync("Filename: " + dataSet.Filename);

                    if (dataSet.GeographicLevels.Any())
                    {
                        await file.WriteLineAsync("Geographic levels: " + string.Join("; ", dataSet.GeographicLevels));
                    }

                    var timePeriodsLabel = dataSet.TimePeriods.ToLabel();

                    if (!timePeriodsLabel.IsNullOrWhitespace())
                    {
                        await file.WriteLineAsync($"Time period: {timePeriodsLabel}");
                    }

                    if (!dataSet.Content.IsNullOrWhitespace())
                    {
                        var content = await HtmlToTextUtils.HtmlToText(dataSet.Content);
                        await file.WriteLineAsync($"Content summary: {content}");
                    }

                    var variables = dataSet
                        .Variables.Where(variable =>
                            !variable.Label.IsNullOrWhitespace() || !variable.Value.IsNullOrWhitespace()
                        )
                        .ToList();

                    if (variables.Any())
                    {
                        await file.WriteLineAsync();
                        await file.WriteLineAsync("Variable names and descriptions for this file are provided below:");
                        await file.WriteLineAsync();

                        var padding = variables.Aggregate(
                            (Value: 0, Label: 0),
                            (acc, variable) =>
                            {
                                if (variable.Value.Length > acc.Value)
                                {
                                    acc.Value = variable.Value.Length;
                                }

                                if (variable.Label.Length > acc.Label)
                                {
                                    acc.Label = variable.Label.Length;
                                }

                                return acc;
                            }
                        );

                        // Adds a table header for variable names/descriptions
                        await file.WriteLineAsync(
                            "Variable name".PadRight(padding.Value) + VariableSeparator + "Variable description"
                        );
                        await file.WriteLineAsync(
                            string.Empty.PadRight(padding.Value, '-')
                                + VariableSeparator
                                + string.Empty.PadRight(padding.Label, '-')
                        );

                        // Add table body for variable names/descriptions
                        await variables
                            .ToAsyncEnumerable()
                            .ForEachAwaitAsync(async variable =>
                            {
                                await file.WriteLineAsync(
                                    variable.Value.PadRight(padding.Value) + VariableSeparator + variable.Label
                                );
                            });
                    }

                    await WriteFootnotes(file, dataSet);

                    // Add some extra lines between data files
                    if (index < dataSets.Count - 1)
                    {
                        await file.WriteLineAsync();
                        await file.WriteLineAsync();
                    }
                }
            );
    }

    private static async Task WriteFootnotes(TextWriter file, DataGuidanceDataSetViewModel dataSet)
    {
        var footnotes = dataSet.Footnotes.Where(footnote => !footnote.Label.IsNullOrWhitespace()).ToList();

        if (!footnotes.Any())
        {
            return;
        }

        await file.WriteLineAsync();
        await file.WriteLineAsync("Footnotes:");
        await file.WriteLineAsync();

        await footnotes
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(
                async (footnote, footnoteIndex) =>
                {
                    var listItemStart = $"{footnoteIndex + 1}. ";

                    await file.WriteAsync(listItemStart);

                    var indent = string.Empty.PadRight(listItemStart.Length);
                    var label = await HtmlToTextUtils.HtmlToText(footnote.Label);

                    await label
                        .ToLines()
                        .ToAsyncEnumerable()
                        .ForEachAwaitAsync(
                            async (line, lineIndex) =>
                            {
                                if (lineIndex == 0)
                                {
                                    await file.WriteLineAsync(line);
                                    return;
                                }

                                await file.WriteLineAsync(indent + line);
                            }
                        );
                }
            );
    }
}
