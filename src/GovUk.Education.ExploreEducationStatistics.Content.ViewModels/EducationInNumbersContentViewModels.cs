using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public class EducationInNumbersContentViewModels
{
    public class EinContentSectionViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public List<EinContentBlockViewModel> Content { get; set; } = new();
    }

    public class EinContentBlockViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EinBlockType Type { get; set; }
    }

    public class EinHtmlBlockViewModel : EinContentBlockViewModel
    {
        public string Body { get; set; } = string.Empty;
    }
}
