using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ContentSection
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<IContentBlock> Content { get; set; }

        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }
    }

    [JsonConverter(typeof(ContentBlockConverter))]
    [KnownType(typeof(MarkDownBlock))]
    [KnownType(typeof(InsetTextBlock))]
    [KnownType(typeof(DataBlock))]
    [KnownType(typeof(HtmlBlock))]
    public abstract class IContentBlock
    {
        public Guid Id { get; set; }

        public ContentSection ContentSection { get; set; }

        public Guid? ContentSectionId { get; set; }

        public abstract string Type { get; set; }
    }

    public class MarkDownBlock : IContentBlock
    {
        public string Body { get; set; }

        public override string Type { get; set; } = "MarkDownBlock";
    }

    public class HtmlBlock : IContentBlock
    {
        public string Body { get; set; }

        public override string Type { get; set; } = "HtmlBlock";
    }

    public class InsetTextBlock : IContentBlock
    {
        public string Heading { get; set; }

        public string Body { get; set; }

        public override string Type { get; set; } = "InsetTextBlock";
    }

    public class DataBlock : IContentBlock
    {
        public string Heading { get; set; }

        public DataBlockRequest DataBlockRequest { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public Summary Summary { get; set; }

        public List<Table> Tables { get; set; }

        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public override string Type { get; set; } = "DataBlock";
    }

    public class Summary
    {
        public List<string> dataKeys { get; set; }

        public List<string> dataSummary { get; set; }

        public List<string> dataDefinition { get; set; }

        public MarkDownBlock description { get; set; }
    }

    public class Table
    {
        public List<string> indicators { get; set; }

        public TableHeaders tableHeaders { get; set; }
    }

    public class TableHeaders
    {
        public List<List<TableOption>> columnGroups { get; set; }
        public List<TableOption> columns { get; set; }
        public List<List<TableOption>> rowGroups { get; set; }
        public List<TableOption> rows { get; set; }
    }

    public class TableOption
    {
        public string _class { get; set; }
        public string _construct { get; set; }
        
        public string label { get; set; }
        public string value { get; set; }
    }
}