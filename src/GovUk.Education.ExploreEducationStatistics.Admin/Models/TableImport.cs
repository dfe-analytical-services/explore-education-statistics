using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    /**
     * Temporary class based on the old DatafileImport.
     * Used to read Imports from Table Storage.
     */
    public class TableImport : TableEntity
    {
        public string Errors { get; set; }

        public string Message { get; set; }

        public string Status { get; set; }

        public int PercentageComplete { get; set; }

        public int NumberOfRows { get; set; }

        public string DataFilename => RowKey;

        public IEnumerable<TableImportError> ImportErrors => Errors.IsNullOrEmpty()
            ? new List<TableImportError>()
            : JsonConvert.DeserializeObject<IEnumerable<TableImportError>>(Errors);

        public TableImportMessage ImportMessage => Message.IsNullOrEmpty()
            ? null 
            : JsonConvert.DeserializeObject<TableImportMessage>(Message);
        
        public DataImportStatus DataImportStatus => Enum.Parse<DataImportStatus>(Status);

        public Guid ReleaseId => Guid.Parse(PartitionKey);
    }

    /**
     * Temporary class based on the old ValidationError.
     */
    public class TableImportError
    {
        public string Message { get; set; }
    }
    
    /**
     * Temporary class based on the old ImportMessage.
     * Ignores the Processor.Model.Release, BatchNo, and Seeding fields.
     */
    public class TableImportMessage
    {
        public Guid SubjectId { get; set; }
        public string DataFileName { get; set; }
        public string MetaFileName { get; set; }
        public int NumBatches { get; set; }
        public int RowsPerBatch { get; set; }
        public int TotalRows { get; set; }
        public string ArchiveFileName { get; set; }
    }
}