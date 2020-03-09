using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public enum ContentSectionType
    {
        Generic,
        ReleaseSummary,
        KeyStatistics,
        KeyStatisticsSecondary,
        Headlines
    }
    
    public class ContentSection
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<IContentBlock> Content { get; set; }

        public ReleaseContentSection? Release { get; set; }
        
        [JsonIgnore]
        public ContentSectionType Type { get; set; }

        public ContentSection CreateReleaseAmendment(CreateAmendmentContext ctx, ReleaseContentSection newParent)
        {
            var copy = MemberwiseClone() as ContentSection;
            copy.Id = Guid.NewGuid();
            ctx.OldToNewIdContentSectionMappings.Add(this, copy);
            
            copy.Release = newParent;
            
            copy.Content = copy
                .Content
                .Select(content => content.CreateReleaseAmendment(ctx, copy))
                .ToList();

            return copy;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class ContentBlockClassType : Attribute
    {
        public Type Type { get; set; }
    }

    public enum ContentBlockType
    {
        [ContentBlockClassType(Type = typeof(MarkDownBlock))]
        MarkDownBlock,
        
        [ContentBlockClassType(Type = typeof(HtmlBlock))]
        HtmlBlock,
        
        [ContentBlockClassType(Type = typeof(InsetTextBlock))]
        InsetTextBlock,
        
        [ContentBlockClassType(Type = typeof(DataBlock))]
        DataBlock
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

        public List<Comment> Comments { get; set; }

        public IContentBlock CreateReleaseAmendment(CreateAmendmentContext ctx, ContentSection newParent)
        {
            var copy = MemberwiseClone() as IContentBlock;
            copy.Id = Guid.NewGuid();
            ctx.OldToNewIdContentBlockMappings.Add(this, copy);

            if (newParent != null)
            {
                copy.ContentSection = newParent;
                copy.ContentSectionId = newParent.Id;
            }

            // start a new amendment with no comments
            copy.Comments = new List<Comment>();
            return copy;
        }
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

        public ObservationQueryContext DataBlockRequest { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public Summary Summary { get; set; }

        public List<Table> Tables { get; set; }

        public override string Type { get; set; } = ContentBlockType.DataBlock.ToString();
    }

    public class Summary
    {
        public List<Guid> dataKeys { get; set; }

        public List<string> dataSummary { get; set; }

        public List<string> dataDefinition { get; set; }

        public List<string> dataDefinitionTitle { get; set; }
    }

    public class Table
    {
        public TableHeaders tableHeaders { get; set; }
    }

    public class TableHeaders
    {
        public List<List<TableOption>> columnGroups { get; set; }
        public List<TableOption> columns { get; set; }
        public List<List<TableRowGroupOption>> rowGroups { get; set; }
        public List<TableOption> rows { get; set; }
    }

    public class TableRowGroupOption
    {
        public string label { get; set; }
        public string level { get; set; }
        public string value { get; set; }

        public TableRowGroupOption()
        {
        }

        public TableRowGroupOption(string label, string level, string value)
        {
            this.label = label;
            this.level = level;
            this.value = value;
        }
    }
    
    public class TableOption
    {
        public string label { get; set; }
        public string value { get; set; }

        public TableOption()
        {
        }

        public TableOption(string label, string value)
        {
            this.label = label;
            this.value = value;
        }
    }

    public static class ContentBlockUtil
    {
        public static ContentBlockType GetContentBlockTypeEnumValueFromType<T>() 
            where T : IContentBlock
        {
            var enumValues = new List<ContentBlockType>(Enum
                .GetValues(typeof(ContentBlockType)).OfType<ContentBlockType>());

            return enumValues.Find(value => GetContentBlockClassTypeFromEnumValue(value) == typeof(T));
        }
        
        public static Type GetContentBlockClassTypeFromEnumValue(ContentBlockType enumValue) 
        {
            return enumValue.GetEnumAttribute<ContentBlockClassType>().Type;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<CommentState>))]
    public enum CommentState
    {
        open,
        resolved
    }

    public class Comment
    {

        public Guid Id { get; set; }
        public Guid? IContentBlockId { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public string CommentText { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public CommentState State { get; set; }
    }
}