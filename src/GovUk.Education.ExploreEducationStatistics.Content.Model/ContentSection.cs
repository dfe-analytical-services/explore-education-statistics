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

    public enum ContentBlockType
    {
        MarkDownBlock,
        HtmlBlock,
        InsetTextBlock,
        DataBlock,
    }

    [JsonConverter(typeof(ContentBlockConverter))]
    [KnownType(typeof(MarkDownBlock))]
    [KnownType(typeof(InsetTextBlock))]
    [KnownType(typeof(DataBlock))]
    [KnownType(typeof(HtmlBlock))]
    public abstract class IContentBlock
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public ContentSection ContentSection { get; set; }

        [JsonIgnore]
        public Guid? ContentSectionId { get; set; }
        
        public int Order { get; set; }

        public abstract string Type { get; set; }
    }

    public class MarkDownBlock : IContentBlock
    {
        public MarkDownBlock()
        {
            
        }
        
        public string Body { get; set; }

        public override string Type { get; set; } = ContentBlockType.MarkDownBlock.ToString();
    }

    public class HtmlBlock : IContentBlock
    {
        public HtmlBlock()
        {
            
        }
        
        public string Body { get; set; }

        public override string Type { get; set; } = ContentBlockType.HtmlBlock.ToString();
    }

    public class InsetTextBlock : IContentBlock
    {
        public InsetTextBlock()
        {
            
        }
        
        public string Heading { get; set; }

        public string Body { get; set; }

        public override string Type { get; set; } = ContentBlockType.InsetTextBlock.ToString();
    }

    public class DataBlock : IContentBlock
    {
        public DataBlock()
        {
            
        }
        
        public string Heading { get; set; }
        
        public string CustomFootnotes { get; set; }
        
        public string Name { get; set; }
        
        public string Source { get; set; }

        public DataBlockRequest DataBlockRequest { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public Summary Summary { get; set; }

        public List<Table> Tables { get; set; }

        public override string Type { get; set; } = ContentBlockType.DataBlock.ToString();
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
        public string label { get; set; }
        public string value { get; set; }
    }
}