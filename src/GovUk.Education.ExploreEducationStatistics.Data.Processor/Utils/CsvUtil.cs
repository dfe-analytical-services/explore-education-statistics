using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class CsvUtil
    {
        public static async Task<List<string>> GetCsvHeaders(
            Func<Task<Stream>> streamProvider)
        {
            using var dataFileReader = new StreamReader(await streamProvider.Invoke());
            using var csvReader = new CsvReader(dataFileReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            return csvReader.Context.Record.ToList();
        }
        
        public static async Task<List<List<string>>> GetCsvRows(
            Func<Task<Stream>> streamProvider,
            bool skipHeaderRow = true)
        {
            return (await Select(streamProvider, (cells, _) => cells, skipHeaderRow)).ToList();
        }
        
        public static async Task<int> GetTotalRows(
            Func<Task<Stream>> streamProvider,
            bool skipHeaderRow = true)
        {
            using var streamReader = new StreamReader(await streamProvider.Invoke());

            var totalRows = 0;

            while (!streamReader.EndOfStream)
            {
                await streamReader.ReadLineAsync();
                totalRows++;
            }

            return skipHeaderRow ? totalRows - 1 : totalRows;
        }

        // TODO EES-3798 - CancellationToken?
        public static async Task ForEachRow(
            Func<Task<Stream>> streamProvider,
            Func<List<string>, int, Task<bool>> action,
            bool skipHeaderRow = true)
        {
            using var dataFileReader = new StreamReader(await streamProvider.Invoke());
            using var csvReader = new CsvReader(dataFileReader, CultureInfo.InvariantCulture);
            
            // Skip the first record of the CSV when attaching the CsvDataReader to the CsvReader if we want to omit
            // the header row from the rows being iterated over.
            csvReader.Configuration.HasHeaderRecord = skipHeaderRow;
            using var csvDataReader = new CsvDataReader(csvReader);
            
            while (await csvReader.ReadAsync())
            {
                var cellCount = csvDataReader.FieldCount;
                var cells = Enumerable
                    .Range(0, cellCount)
                    .Select(csvReader.GetField<string>)
                    .ToList();

                var result = await action.Invoke(cells, csvReader.Context.Row);

                if (!result)
                {
                    break;
                }
            }
        }
        
        public static Task ForEachRow(
            Func<Task<Stream>> streamProvider,
            Func<List<string>, int, Task> action,
            bool skipHeaderRow = true)
        {
            return ForEachRow(streamProvider, async (cells, index) =>
            {
                await action.Invoke(cells, index);
                return true;
            }, skipHeaderRow);
        }
        
        public static Task ForEachRow(
            Func<Task<Stream>> streamProvider,
            Func<List<string>, int, bool> action,
            bool skipHeaderRow = true)
        {
            return ForEachRow(streamProvider, (cells, index) =>
            {
                var result = action.Invoke(cells, index);
                return Task.FromResult(result);
            }, skipHeaderRow);
        }
        
        public static Task ForEachRow(
            Func<Task<Stream>> streamProvider,
            Action<List<string>, int> action,
            bool skipHeaderRow = true)
        {
            return ForEachRow(streamProvider, (cells, index) =>
            {
                action.Invoke(cells, index);
                return Task.FromResult(true);
            }, skipHeaderRow);
        }

        public static async Task<List<TResult>> Select<TResult>(
            Func<Task<Stream>> streamProvider,
            Func<List<string>, int, Task<TResult>> action,
            bool skipHeaderRow = true)
        {
            var list = new List<TResult>();

            await ForEachRow(streamProvider, async (cells, index) =>
            {
                list.Add(await action.Invoke(cells, index));
            }, skipHeaderRow);

            return list;
        }
        
        public static Task<List<TResult>> Select<TResult>(
            Func<Task<Stream>> streamProvider,
            Func<List<string>, int, TResult> action,
            bool skipHeaderRow = true)
        {
            return Select(streamProvider, (cells, index) =>
            {
                var result = action.Invoke(cells, index);
                return Task.FromResult(result);
            }, skipHeaderRow);
        }
            
        public static T BuildType<T>(IReadOnlyList<string> rowValues, List<string> colValues, string column,
            Func<string, T> func)
        {
            var value = Value(rowValues, colValues, column);
            return value == null ? default(T) : func(value);
        }

        public static T BuildType<T>(IReadOnlyList<string> rowValues, List<string> colValues, IEnumerable<string> columns,
            Func<string[], T> func)
        {
            var values = Values(rowValues, colValues, columns);
            return values.All(value => value == null) ? default(T) : func(values);
        }

        public static string[] Values(IReadOnlyList<string> rowValues, List<string> colValues, IEnumerable<string> columns)
        {
            return columns.Select(c => Value(rowValues, colValues, c)).ToArray();
        }

        public static string Value(IReadOnlyList<string> rowValues, List<string> colValues, string column)
        {
            return colValues.Contains(column) ? rowValues[colValues.FindIndex(h => h.Equals(column))].Trim().NullIfWhiteSpace() : null;
        }

        public static GeographicLevel GetGeographicLevel(IReadOnlyList<string> rowValues, List<string> colValues)
        {
            var value = Value(rowValues, colValues, "geographic_level");
            foreach (var val in (GeographicLevel[]) Enum.GetValues(typeof(GeographicLevel)))
            {
                if (val.GetEnumLabel().ToLower().Equals(value.ToLower()))
                {
                    return val;
                }
            }

            throw new InvalidGeographicLevelException(value);
        }

        /// <summary>
        /// Determines if a row should be imported based on geographic level.
        /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' importable levels are ignored. 
        /// </summary>
        public static bool IsRowAllowed(bool soleGeographicLevel,
            IReadOnlyList<string> rowValues,
            List<string> colValues)
        {
            return soleGeographicLevel ||
                   !GetGeographicLevel(rowValues, colValues).IsSoloImportableLevel();
        }
    }
}
