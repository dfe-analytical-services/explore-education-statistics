#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class DataGuidanceFileWriter : IDataGuidanceFileWriter
    {
        private const string VariableSeparator = "  |  ";

        private readonly ContentDbContext _contentDbContext;
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;

        public DataGuidanceFileWriter(
            ContentDbContext contentDbContext,
            IDataGuidanceSubjectService dataGuidanceSubjectService)
        {
            _contentDbContext = contentDbContext;
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
        }

        public async Task<Stream> WriteToStream(Stream stream, Release release, IEnumerable<Guid>? subjectIds = null)
        {
            // Make sure publication has been hydrated
            await _contentDbContext.Entry(release)
                .Reference(r => r.Publication)
                .LoadAsync();

            var subjects = await ListSubjects(release, subjectIds);

            await using var file = new StreamWriter(stream, leaveOpen: stream.CanRead);

            await DoWrite(file, release, subjects);

            await file.FlushAsync();

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return stream;
        }

        private async Task<List<DataGuidanceSubjectViewModel>> ListSubjects(
            Release release,
            IEnumerable<Guid>? subjectIds = null)
        {
            var subjects = await _dataGuidanceSubjectService.GetSubjects(release.Id, subjectIds);

            if (subjects.IsLeft)
            {
                throw new ArgumentException($"Could not find subjects for release: {release.Id}");
            }

            return subjects.Right;
        }

        private static async Task DoWrite(
            TextWriter file,
            Release release,
            IList<DataGuidanceSubjectViewModel> subjects)
        {
            // Add header information including publication/release title
            await file.WriteLineAsync(release.Publication.Title);
            await file.WriteLineAsync(
                TimePeriodLabelFormatter.Format(
                    release.Year,
                    release.TimePeriodCoverage,
                    TimePeriodLabelFormat.FullLabel
                )
            );

            if (!release.DataGuidance.IsNullOrWhitespace())
            {
                await file.WriteLineAsync();

                // Add the release's guidance content
                var guidance = await HtmlToTextUtils.HtmlToText(release.DataGuidance);
                await file.WriteAsync(guidance);
                await file.WriteLineAsync();
            }

            await WriteDataFiles(file, subjects);
        }

        private static async Task WriteDataFiles(TextWriter file, IList<DataGuidanceSubjectViewModel> subjects)
        {
            if (subjects.Count == 0)
            {
                return;
            }

            // Add 'Data files' section
            await file.WriteLineAsync();
            await file.WriteLineAsync("Data files");
            await file.WriteLineAsync();

            await subjects
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async (subject, index) =>
                    {
                        await file.WriteLineAsync(subject.Name);
                        await file.WriteLineAsync();

                        await file.WriteLineAsync("Filename: " + subject.Filename);

                        if (subject.GeographicLevels.Any())
                        {
                            await file.WriteLineAsync("Geographic levels: " +
                                                      string.Join("; ", subject.GeographicLevels));
                        }

                        var timePeriodsLabel = subject.TimePeriods.ToLabel();

                        if (!timePeriodsLabel.IsNullOrWhitespace())
                        {
                            await file.WriteLineAsync($"Time period: {timePeriodsLabel}");
                        }

                        if (!subject.Content.IsNullOrWhitespace())
                        {
                            var content = await HtmlToTextUtils.HtmlToText(subject.Content);
                            await file.WriteLineAsync($"Content summary: {content}");
                        }

                        var variables = subject.Variables
                            .Where(
                                variable =>
                                    !variable.Label.IsNullOrWhitespace()
                                    || !variable.Value.IsNullOrWhitespace()
                            )
                            .ToList();

                        if (variables.Any())
                        {
                            await file.WriteLineAsync();
                            await file.WriteLineAsync(
                                "Variable names and descriptions for this file are provided below:");
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
                                string.Empty.PadRight(
                                    padding.Value,
                                    '-'
                                ) + VariableSeparator + string.Empty.PadRight(padding.Label, '-')
                            );

                            // Add table body for variable names/descriptions
                            await variables
                                .ToAsyncEnumerable()
                                .ForEachAwaitAsync(
                                    async variable =>
                                    {
                                        await file.WriteLineAsync(
                                            variable.Value.PadRight(padding.Value) + VariableSeparator + variable.Label
                                        );
                                    }
                                );
                        }

                        var footnotes = subject.Footnotes
                            .Where(footnote => !footnote.Label.IsNullOrWhitespace())
                            .ToList();

                        if (footnotes.Any())
                        {
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

                                        await footnote.Label
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

                        // Add some extra lines between data files
                        if (index < subjects.Count - 1)
                        {
                            await file.WriteLineAsync();
                            await file.WriteLineAsync();
                        }
                    }
                );
        }
    }
}
