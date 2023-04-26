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
    /// This is best used with CSVs that are of known small values and should not be used for CSVs that
    /// could be large, as this will load the entire contents into memory.
    /// This method uses and closes the provided Stream.
    /// </remarks>
    public static async Task<List<List<string>>> GetCsvRows(
        Func<Task<Stream>> streamProvider,
        int startingRowIndex = 0)
    {
        return (await Select(
            streamProvider, 
            (cells, _, _) => cells, 
            startingRowIndex))
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
    /// The index is the zero-based index of the data row in the CSV, not including the header. Therefore the first
    /// row of data under the header will have the index of "0".
    /// </param>
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to be iterated over. For instance,
    /// if this is set to 0, the first row being iterated over will be the first line of data, whereas if this is set
    /// to 1, the first row being iterated over will be the 2nd line of data.</param>
    public static async Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool, Task<bool>> func,
        int startingRowIndex = 0)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);

        using var dataFileReader = new StreamReader(await streamProvider.Invoke());
        using var csvReader = new CsvReader(dataFileReader, config);
        using var csvDataReader = new CsvDataReader(csvReader);
        var lastLine = !await csvReader.ReadAsync();

        while (!lastLine)
        {
            var currentRowIndex = csvReader.Parser.Row - 2;

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
    
    /// <summary>
    /// Execute a given function against batches of rows from the provided CSV. The batch
    /// of rows (a list of lists of cells) and the index of the current batch being processed are
    /// provided to the function.
    /// </summary>
    /// <param name="streamProvider">The Stream of the CSV.</param>
    /// <param name="batchSize">The number of rows in each batch.</param>
    /// <param name="func">
    /// The function to execute against each batch of CSV rows. The function takes the rows 
    /// the index of the current batch, and returns "true" to continue iterating, or "false" to finish looping early.
    /// The index is a zero-based index. Therefore the first batch of rows will have the index of "0".
    ///
    /// Note that if using "startingBatchIndex", this will be included in the index provided to the function. For
    /// example, if "startingBatchIndex" is "10", the first batch of rows provided to the first function execution will
    /// be the 11th batch of rows from the CSV, and the index provided to the first function execution will be "10".  
    /// </param>
    /// <param name="startingBatchIndex">Optional parameter to skip a number of batches to be processed by the function.
    /// For instance, if this is set to 0, the first batch being presented to the function will be the 1st X number
    /// of rows (determined by "batchSize"), whereas if this is set to 1, the first batch being presented to the
    /// function will contain the 2nd X number of rows of data.</param>
    public static async Task Batch(
        Func<Task<Stream>> streamProvider,
        int batchSize,
        Func<List<List<string>>, int, Task<bool>> func,
        int startingBatchIndex = 0)
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
                    var result = await func.Invoke(linesInBatch.ToList(), batchIndex + startingBatchIndex);

                    if (!result)
                    {
                        return false;
                    }

                    batchIndex++;
                    linesInBatch.Clear();
                }

                return true;
            },
            startingRowIndex: startingBatchIndex * batchSize);
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
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to be iterated over. For instance,
    /// if this is set to 0, the first row being iterated over will be the first line of data, whereas if this is set
    /// to 1, the first row being iterated over will be the 2nd line of data.</param>
    public static Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool, Task> func,
        int startingRowIndex = 0)
    {
        return ForEachRow(
            streamProvider,
            async (cells, index, lastLine) =>
            {
                await func.Invoke(cells, index, lastLine);
                return true;
            },
            startingRowIndex
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
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to be iterated over. For instance,
    /// if this is set to 0, the first row being iterated over will be the first line of data, whereas if this is set
    /// to 1, the first row being iterated over will be the 2nd line of data.</param>
    public static Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool, bool> action,
        int startingRowIndex = 0)
    {
        return ForEachRow(
            streamProvider,
            (cells, index, lastLine) =>
            {
                var result = action.Invoke(cells, index, lastLine);
                return Task.FromResult(result);
            },
            startingRowIndex
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
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to be iterated over. For instance,
    /// if this is set to 0, the first row being iterated over will be the first line of data, whereas if this is set
    /// to 1, the first row being iterated over will be the 2nd line of data.</param>
    public static Task ForEachRow(
        Func<Task<Stream>> streamProvider,
        Action<List<string>, int, bool> action,
        int startingRowIndex = 0)
    {
        return ForEachRow(
            streamProvider,
            (cells, index, lastLine) =>
            {
                action.Invoke(cells, index, lastLine);
                return Task.FromResult(true);
            },
            startingRowIndex
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
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to be iterated over. For instance,
    /// if this is set to 0, the first row being iterated over will be the first line of data, whereas if this is set
    /// to 1, the first row being iterated over will be the 2nd line of data.</param>
    public static async Task<List<TResult>> Select<TResult>(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool, Task<TResult>> func,
        int startingRowIndex = 0)
    {
        var list = new List<TResult>();

        await ForEachRow(
            streamProvider,
            async (cells, index, lastLine) =>
            {
                list.Add(await func.Invoke(cells, index, lastLine));
            },
            startingRowIndex
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
    /// <param name="startingRowIndex">Optional parameter to skip a number of rows to be iterated over. For instance,
    /// if this is set to 0, the first row being iterated over will be the first line of data, whereas if this is set
    /// to 1, the first row being iterated over will be the 2nd line of data.</param>
    public static Task<List<TResult>> Select<TResult>(
        Func<Task<Stream>> streamProvider,
        Func<List<string>, int, bool, TResult> func,
        int startingRowIndex = 0)
    {
        return Select(
            streamProvider,
            (cells, index, lastLine) =>
            {
                var result = func.Invoke(cells, index, lastLine);
                return Task.FromResult(result);
            },
            startingRowIndex
        );
    }
}