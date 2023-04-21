#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class CsvUtils
{
    public static Task<List<string>> GetCsvHeaders(Stream stream)
        => GetCsvHeaders(() => Task.FromResult(stream));

    public static Task<List<string>> GetCsvHeaders(Task<Stream> stream)
        => GetCsvHeaders(() => stream);

    /// <summary>
    /// Gets the header values of the first line of the provided CSV.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    public static async Task<List<string>> GetCsvHeaders(Func<Task<Stream>> streamProvider)
    {
        using var dataFileReader = new StreamReader(await streamProvider.Invoke());
        using var csvReader = new CsvReader(dataFileReader, CultureInfo.InvariantCulture);
        await csvReader.ReadAsync();
        csvReader.ReadHeader();
        return csvReader.HeaderRecord?.ToList() ?? new List<string>();
    }

    /// <summary>
    /// Gets the lines of the provided CSV as lists of cell values.
    /// </summary>
    /// <remarks>
    /// This is best used with CSVs that are of known small values.
    /// This method uses and closes the provided Stream.
    /// </remarks>
    public static async Task<List<List<string>>> GetCsvRows(
        Func<Task<Stream>> streamProvider,
        int startingBatchIndex = 0,
        bool skipHeaderRow = true)
    {
        return (await Select(
            streamProvider, 
            (cells, _) => cells, 
            startingBatchIndex,
            skipHeaderRow))
            .ToList();
    }

    /// <summary>
    /// Counts the total rows on the provided CSV.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the count.</param>
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

    /// <summary>
    /// Execute a given function against each row of the provided CSV.  The cells of the current row and the index
    /// of the row in the CSV are provided to the function.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="func">The function to execute against each row of the CSV. The function takes the cells and
    /// the index of the current row, and returns "true" to continue iterating, or "false" to finish looping early.
    /// The index is the zero-based index of the row in the CSV, minus 1 if the CSV header was skipped.
    /// </param>
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to execute. This value will
    /// take into account the header and whether or not we have chosen to skip it. For instance, if a header isn't
    /// being skipped and this is set to 0, the first line executed will be the header row, whereas if this is set to 1,
    /// the first line being executed will be the first row of data. And if a header is being skipped and this is set to
    /// 0, the first row being executed will be the first line of data, whereas if this is set to 1, the first row being
    /// executed will be the second line of data.</param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the rows being executed.</param>
    public static async Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool, Task<bool>> func,
        int startingRowIndex = 0,
        bool skipHeaderRow = true)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Skip the first record of the CSV when attaching the CsvDataReader to the
            // CsvReader if we want to omit the header row from the rows being iterated over.
            HasHeaderRecord = skipHeaderRow
        };

        using var dataFileReader = new StreamReader(await streamProvider.Invoke());
        using var csvReader = new CsvReader(dataFileReader, config);
        using var csvDataReader = new CsvDataReader(csvReader);
        var lastLine = !await csvReader.ReadAsync();

        while (!lastLine)
        {
            var currentRowIndex = csvReader.Parser.Row + (skipHeaderRow ? -1 : 0);

            if (currentRowIndex < startingRowIndex)
            {
                // Skipping this row as it is below startingRowIndex
                lastLine = !await csvReader.ReadAsync();
                continue;
            }
            
            var cellCount = csvDataReader.FieldCount;

            var cells = Enumerable
                .Range(0, cellCount)
                .Select(csvReader.GetField<string>)
                .OfType<string>()
                .ToList();
            
            lastLine = !await csvReader.ReadAsync();
            
            var result = await func.Invoke(cells, currentRowIndex, lastLine);

            if (!result)
            {
                break;
            }
        }
    }
    
    public static async Task Batch(
        Func<Task<Stream>> streamProvider,
        int batchSize,
        Func<List<List<string>>, int, Task<bool>> func,
        int startingBatchIndex,
        bool skipHeaderRow = true)
    {
        var linesInBatch = new List<List<string>>();
        var batchIndex = 0;
        var rowsProcessed = 0;
        
        await ForEachRow(
            streamProvider,
            async (cells, _, lastLine) =>
            {
                rowsProcessed++;
                linesInBatch.Add(cells);

                if (lastLine || rowsProcessed % batchSize == 0)
                {
                    var result = await func.Invoke(linesInBatch, batchIndex + startingBatchIndex);

                    if (!result)
                    {
                        return false;
                    }

                    batchIndex++;
                    linesInBatch.Clear();
                }

                return true;
            },
            startingRowIndex: (startingBatchIndex * batchSize) + 1,
            skipHeaderRow);
    }

    /// <summary>
    /// Execute a given function against each row of the provided CSV.  The cells of the current row and the index
    /// of the row in the CSV are provided to the function.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="func">The function to execute against each row of the CSV. The function takes the cells and
    /// the index of the current row.
    /// The index is the zero-based index of the row in the CSV, minus 1 if the CSV header was skipped.
    /// </param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the executions.</param>
    public static Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, Task> func,
        int startingBatchIndex = 0,
        bool skipHeaderRow = true)
    {
        return ForEachRow(
            streamProvider,
            async (cells, index, _) =>
            {
                await func.Invoke(cells, index);
                return true;
            },
            startingBatchIndex,
            skipHeaderRow
        );
    }

    /// <summary>
    /// Execute a given function against each row of the provided CSV.  The cells of the current row and the index
    /// of the row in the CSV are provided to the function.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="action">The action to execute against each row of the CSV. The function takes the cells and
    /// the index of the current row, and returns "true" to continue iterating, or "false" to finish looping early.
    /// The index is the zero-based index of the row in the CSV, minus 1 if the CSV header was skipped.
    /// </param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the executions.</param>
    public static Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool> action,
        int startingBatchIndex = 0,
        bool skipHeaderRow = true)
    {
        return ForEachRow(
            streamProvider,
            (cells, index) =>
            {
                var result = action.Invoke(cells, index);
                return Task.FromResult(result);
            },
            startingBatchIndex,
            skipHeaderRow
        );
    }

    /// <summary>
    /// Execute a given function against each row of the provided CSV.  The cells of the current row and the index
    /// of the row in the CSV are provided to the function.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="action">The action to execute against each row of the CSV. The function takes the cells and
    /// the index of the current row.
    /// The index is the zero-based index of the row in the CSV, minus 1 if the CSV header was skipped.
    /// </param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the executions.</param>
    public static Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Action<List<string>, int> action,
        int startingBatchIndex = 0,
        bool skipHeaderRow = true)
    {
        return ForEachRow(
            streamProvider,
            (cells, index) =>
            {
                action.Invoke(cells, index);
                return Task.FromResult(true);
            },
            startingBatchIndex,
            skipHeaderRow
        );
    }

    /// <summary>
    /// Execute a given function against each row of the provided CSV and uses the return value to build a new
    /// List of results of type <typeparam name="TResult" />.  The cells of the current row and the index
    /// of the row in the CSV are provided to the function.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="func">The function to execute against each row of the CSV. The function takes the cells and
    /// the index of the current row, and returns a new value per line of the CSV to generate a new List.
    /// The index is the zero-based index of the row in the CSV, minus 1 if the CSV header was skipped.
    /// </param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the executions.</param>
    public static async Task<List<TResult>> Select<TResult>(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, Task<TResult>> func,
        int startingBatchIndex = 0,
        bool skipHeaderRow = true)
    {
        var list = new List<TResult>();

        await ForEachRow(
            streamProvider,
            async (cells, index) => { list.Add(await func.Invoke(cells, index)); },
            startingBatchIndex,
            skipHeaderRow
        );

        return list;
    }

    /// <summary>
    /// Execute a given function against each row of the provided CSV and uses the return value to build a new
    /// List of results of type <typeparam name="TResult" />.  The cells of the current row and the index
    /// of the row in the CSV are provided to the function.
    /// </summary>
    /// <remarks>
    /// This method uses and closes the provided Stream.
    /// </remarks>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="func">The function to execute against each row of the CSV. The function takes the cells and
    /// the index of the current row, and returns a new value per line of the CSV to generate a new List.
    /// The index is the zero-based index of the row in the CSV, minus 1 if the CSV header was skipped.
    /// </param>
    /// <param name="skipHeaderRow">Choose whether or not to skip a first header row in the executions.</param>
    public static Task<List<TResult>> Select<TResult>(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, TResult> func,
        int startingBatchIndex = 0,
        bool skipHeaderRow = true)
    {
        return Select(
            streamProvider,
            (cells, index) =>
            {
                var result = func.Invoke(cells, index);
                return Task.FromResult(result);
            },
            startingBatchIndex,
            skipHeaderRow
        );
    }

    public static T? BuildType<T>(
        IReadOnlyList<string> rowValues,
        List<string> colValues,
        string column,
        Func<string, T> func)
    {
        var value = Value(rowValues, colValues, column);
        return value == null ? default : func(value);
    }

    public static T? BuildType<T>(
        IReadOnlyList<string> rowValues,
        List<string> colValues,
        IEnumerable<string> columns,
        Func<string[], T> func)
    {
        var values = Values(rowValues, colValues, columns);
        return values.All(value => value == null) ? default : func(values!);
    }

    public static string?[] Values(IReadOnlyList<string> rowValues, List<string> colValues, IEnumerable<string> columns)
    {
        return columns.Select(c => Value(rowValues, colValues, c)).ToArray();
    }

    public static string? Value(
        IReadOnlyList<string> rowValues,
        List<string> colValues,
        string column,
        string? defaultValue = null)
    {
        if (!colValues.Contains(column))
        {
            return defaultValue;
        }

        var cellValue = rowValues[colValues.FindIndex(h => h.Equals(column))].Trim().NullIfWhiteSpace();

        return cellValue ?? defaultValue;
    }
}
