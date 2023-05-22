using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class CsvUtilsTest
{
    private static readonly List<string> ExpectedHeaders = ListOf(
        "time_period","time_identifier","geographic_level","country_code",
        "country_name","old_la_code","new_la_code","la_name","region_code","region_name",
        "lad_code","lad_name","local_enterprise_partnership_code",
        "local_enterprise_partnership_name","rsc_region_lead_name","opportunity_area_code",
        "opportunity_area_name","pcon_code","pcon_name","ward_code","ward_name",
        "admission_numbers"
    );
    
    [Fact]
    public async Task GetCsvHeaders()
    {
        var headers = await CsvUtils.GetCsvHeaders(GetFileStreamSupplier());
        Assert.Equal(ExpectedHeaders, headers);
    }
    
    [Fact]
    public async Task GetCsvHeaders_TaskStream()
    {
        var headers = await CsvUtils.GetCsvHeaders(Task.FromResult(GetFileStream()));
        Assert.Equal(ExpectedHeaders, headers);
    }
    
    [Fact]
    public async Task GetCsvHeaders_Stream()
    {
        var headers = await CsvUtils.GetCsvHeaders(GetFileStream());
        Assert.Equal(ExpectedHeaders, headers);
    }
    
    [Fact]
    public async Task GetCsvRows()
    {
        var rows = await CsvUtils.GetCsvRows(GetFileStreamSupplier());
        
        // We should have been provided 160 rows of data, each containing 22 cells.
        Assert.Equal(160, rows.Count);
        rows.ForEach(row => Assert.Equal(22, row.Count));

        // Check the very first and very last cells of the first and last rows provided.
        Assert.Equal("2018", rows[0][0]);
        Assert.Equal("3962", rows[0][21]);
        Assert.Equal("2007", rows[159][0]);
        Assert.Equal("9884", rows[159][21]);
    }
    
    [Fact]
    public async Task GetTotalRows()
    {
        Assert.Equal(160, await CsvUtils.GetTotalRows(GetFileStreamSupplier()));
    }

    [Fact]
    public async Task ForEachRow()
    {
        var rowsProvided = new List<List<string>>();
        var rowIndexesProvided = new List<int>();
        var lastLinesProvided = new List<bool>();
        
        await CsvUtils.ForEachRow(
            GetFileStreamSupplier(),
            (cells, rowIndex, lastLine) =>
            {
                rowsProvided.Add(cells);
                rowIndexesProvided.Add(rowIndex);
                lastLinesProvided.Add(lastLine);
                return Task.FromResult(true);
            });
        
        // We should have been provided 160 rows of data, each containing 22 cells.
        Assert.Equal(160, rowsProvided.Count);
        rowsProvided.ForEach(row => Assert.Equal(22, row.Count));
        
        // Check the very first and very last cells of the first and last rows provided.
        Assert.Equal("2018", rowsProvided[0][0]);
        Assert.Equal("3962", rowsProvided[0][21]);
        Assert.Equal("2007", rowsProvided[159][0]);
        Assert.Equal("9884", rowsProvided[159][21]);

        // We should have received row indexes from 0 to 159. 
        Assert.Equal(160, rowIndexesProvided.Count);
        rowIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i, indexProvided));

        // We should have received "false" for the lastLine variable value until the final line. 
        Assert.Equal(160, lastLinesProvided.Count);
        lastLinesProvided.ForEach((lastLine, i) => 
            Assert.Equal(i == 159, lastLine));
    }
    
    [Fact]
    public async Task ForEachRow_Task()
    {
        var rowsProvided = new List<List<string>>();
        var rowIndexesProvided = new List<int>();
        var lastLinesProvided = new List<bool>();
        
        await CsvUtils.ForEachRow(
            GetFileStreamSupplier(),
            (cells, rowIndex, lastLine) =>
            {
                rowsProvided.Add(cells);
                rowIndexesProvided.Add(rowIndex);
                lastLinesProvided.Add(lastLine);
                return Task.CompletedTask;
            });
        
        // We should have been provided 160 rows of data, each containing 22 cells.
        Assert.Equal(160, rowsProvided.Count);
        rowsProvided.ForEach(row => Assert.Equal(22, row.Count));
        
        // Check the very first and very last cells of the first and last rows provided.
        Assert.Equal("2018", rowsProvided[0][0]);
        Assert.Equal("3962", rowsProvided[0][21]);
        Assert.Equal("2007", rowsProvided[159][0]);
        Assert.Equal("9884", rowsProvided[159][21]);

        // We should have received row indexes from 0 to 159. 
        Assert.Equal(160, rowIndexesProvided.Count);
        rowIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i, indexProvided));

        // We should have received "false" for the lastLine variable value until the final line. 
        Assert.Equal(160, lastLinesProvided.Count);
        lastLinesProvided.ForEach((lastLine, i) => 
            Assert.Equal(i == 159, lastLine));
    }
    
    [Fact]
    public async Task ForEachRow_Action()
    {
        var rowsProvided = new List<List<string>>();
        var rowIndexesProvided = new List<int>();
        var lastLinesProvided = new List<bool>();
        
        await CsvUtils.ForEachRow(
            GetFileStreamSupplier(),
            (cells, rowIndex, lastLine) =>
            {
                rowsProvided.Add(cells);
                rowIndexesProvided.Add(rowIndex);
                lastLinesProvided.Add(lastLine);
            });
        
        // We should have been provided 160 rows of data, each containing 22 cells.
        Assert.Equal(160, rowsProvided.Count);
        rowsProvided.ForEach(row => Assert.Equal(22, row.Count));
        
        // Check the very first and very last cells of the first and last rows provided.
        Assert.Equal("2018", rowsProvided[0][0]);
        Assert.Equal("3962", rowsProvided[0][21]);
        Assert.Equal("2007", rowsProvided[159][0]);
        Assert.Equal("9884", rowsProvided[159][21]);

        // We should have received row indexes from 0 to 159. 
        Assert.Equal(160, rowIndexesProvided.Count);
        rowIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i, indexProvided));

        // We should have received "false" for the lastLine variable value until the final line. 
        Assert.Equal(160, lastLinesProvided.Count);
        lastLinesProvided.ForEach((lastLine, i) => 
            Assert.Equal(i == 159, lastLine));
    }
    
    [Fact]
    public async Task ForEachRow_Bool()
    {
        var rowsProvided = new List<List<string>>();
        var rowIndexesProvided = new List<int>();
        var lastLinesProvided = new List<bool>();
        
        await CsvUtils.ForEachRow(
            GetFileStreamSupplier(),
            (cells, rowIndex, lastLine) =>
            {
                rowsProvided.Add(cells);
                rowIndexesProvided.Add(rowIndex);
                lastLinesProvided.Add(lastLine);
                return true;
            });
        
        // We should have been provided 160 rows of data, each containing 22 cells.
        Assert.Equal(160, rowsProvided.Count);
        rowsProvided.ForEach(row => Assert.Equal(22, row.Count));
        
        // Check the very first and very last cells of the first and last rows provided.
        Assert.Equal("2018", rowsProvided[0][0]);
        Assert.Equal("3962", rowsProvided[0][21]);
        Assert.Equal("2007", rowsProvided[159][0]);
        Assert.Equal("9884", rowsProvided[159][21]);

        // We should have received row indexes from 0 to 159. 
        Assert.Equal(160, rowIndexesProvided.Count);
        rowIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i, indexProvided));

        // We should have received "false" for the lastLine variable value until the final line. 
        Assert.Equal(160, lastLinesProvided.Count);
        lastLinesProvided.ForEach((lastLine, i) => 
            Assert.Equal(i == 159, lastLine));
    }
    
    [Fact]
    public async Task ForEachRow_ReturnFalse()
    {
        var rowsProvided = new List<List<string>>();
        
        // Return false to exit the ForEachRow early.
        await CsvUtils.ForEachRow(
            GetFileStreamSupplier(),
            (cells, rowIndex, _) =>
            {
                if (rowIndex == 10)
                {
                    return Task.FromResult(false);
                }
                
                rowsProvided.Add(cells);

                return Task.FromResult(true);
            });
        
        // We should have been provided 10 rows of data before we chose to exit the iterations.
        Assert.Equal(10, rowsProvided.Count);
    }
    
    [Fact]
    public async Task ForEachRow_StartingRowIndex()
    {
        var rowsProvided = new List<List<string>>();
        var rowIndexesProvided = new List<int>();
        var lastLinesProvided = new List<bool>();
        
        // We are iterating over rows but skipping the first 20.
        await CsvUtils.ForEachRow(
            GetFileStreamSupplier(),
            (cells, rowIndex, lastLine) =>
            {
                rowsProvided.Add(cells);
                rowIndexesProvided.Add(rowIndex);
                lastLinesProvided.Add(lastLine);
                return Task.FromResult(true);
            },
            startingRowIndex: 20);
        
        // We should have been provided 140 rows of data (160 minus the first 20).
        Assert.Equal(140, rowsProvided.Count);
        rowsProvided.ForEach(row => Assert.Equal(22, row.Count));
        
        // Check the very first and very last cells of the first and last rows provided.
        Assert.Equal("2008", rowsProvided[0][0]);
        Assert.Equal("6765", rowsProvided[0][21]);
        Assert.Equal("2007", rowsProvided[139][0]);
        Assert.Equal("9884", rowsProvided[139][21]);

        // We should have received row indexes from 20 to 159. 
        Assert.Equal(140, rowIndexesProvided.Count);
        rowIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i + 20, indexProvided));

        // We should have received "false" for the lastLine variable value until the final line. 
        Assert.Equal(140, lastLinesProvided.Count);
        lastLinesProvided.ForEach((lastLine, i) => 
            Assert.Equal(i == 139, lastLine));
    }
    
    [Fact]
    public async Task Batch()
    {
        var batchesProvided = new List<List<List<string>>>();
        var batchIndexesProvided = new List<int>();
        
        await CsvUtils.Batch(
            GetFileStreamSupplier(),
            batchSize: 50,
            (batchOfRows, batchIndex) =>
            {
                batchesProvided.Add(batchOfRows);
                batchIndexesProvided.Add(batchIndex);
                return Task.FromResult(true);
            });
        
        // The test csv has 160 data rows, so we expect 4 batches of 50, 50, 50 and 10 rows respectively
        // to have been provided to our function.
        Assert.Equal(4, batchesProvided.Count);
        Assert.Equal(50, batchesProvided[0].Count);
        Assert.Equal(50, batchesProvided[1].Count);
        Assert.Equal(50, batchesProvided[2].Count);
        Assert.Equal(10, batchesProvided[3].Count);
        
        // Check that the batch index provided is correct.
        batchIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i, indexProvided));
        
        // Check the very first and very last cells of each batch.
        Assert.Equal("2018", batchesProvided[0][0][0]);
        Assert.Equal("3962", batchesProvided[0][0][21]);
        Assert.Equal("2006", batchesProvided[0][49][0]);
        Assert.Equal("7360", batchesProvided[0][49][21]);
        
        Assert.Equal("2017", batchesProvided[1][0][0]);
        Assert.Equal("6385", batchesProvided[1][0][21]);
        Assert.Equal("2018", batchesProvided[1][49][0]);
        Assert.Equal("8630", batchesProvided[1][49][21]);
        
        Assert.Equal("2018", batchesProvided[2][0][0]);
        Assert.Equal("5549", batchesProvided[2][0][21]);
        Assert.Equal("2014", batchesProvided[2][49][0]);
        Assert.Equal("3646", batchesProvided[2][49][21]);
        
        Assert.Equal("2010", batchesProvided[3][0][0]);
        Assert.Equal("9304", batchesProvided[3][0][21]);
        Assert.Equal("2007", batchesProvided[3][9][0]);
        Assert.Equal("9884", batchesProvided[3][9][21]);
    }

    [Fact]
    public async Task Batch_StartingBatchIndex()
    {
        var batchesProvided = new List<List<List<string>>>();
        var batchIndexesProvided = new List<int>();
        
        // Here we are choosing to skip the first 2 batches.
        await CsvUtils.Batch(
            GetFileStreamSupplier(),
            batchSize: 50,
            (batchOfRows, batchIndex) =>
            {
                batchesProvided.Add(batchOfRows);
                batchIndexesProvided.Add(batchIndex);
                return Task.FromResult(true);
            },
            startingBatchIndex: 2);
        
        // The test csv has 160 data rows but we have chosen to skip the first 20, so we expect 3 batches
        // of 50, 50, 40 rows respectively to have been provided to our function.
        Assert.Equal(2, batchesProvided.Count);
        Assert.Equal(50, batchesProvided[0].Count);
        Assert.Equal(10, batchesProvided[1].Count);
        
        // Check that the batch index provided is correct, starting at 2 as we have skipped the first 2 batches.
        batchIndexesProvided.ForEach((indexProvided, i) => 
            Assert.Equal(i + 2, indexProvided));
    }
    
    [Fact]
    public async Task Batch_ReturnFalse()
    {
        var batchesProvided = new List<List<List<string>>>();
        
        await CsvUtils.Batch(
            GetFileStreamSupplier(),
            batchSize: 50,
            (batchOfRows, batchIndex) =>
            {
                if (batchIndex == 2)
                {
                    return Task.FromResult(false);
                }
                
                batchesProvided.Add(batchOfRows);
                return Task.FromResult(true);
            });
        
        // We should have been provided 2 batches of rows before we chose to exit the iterations.
        Assert.Equal(2, batchesProvided.Count);
    }

    private static Stream GetFileStream()
    {
        var csv = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            $"Resources{Path.DirectorySeparatorChar}test.csv");

        return new StreamReader(File.OpenRead(csv)).BaseStream;
    }

    private static Func<Task<Stream>> GetFileStreamSupplier()
    {
        return () => Task.FromResult(GetFileStream());
    }
}