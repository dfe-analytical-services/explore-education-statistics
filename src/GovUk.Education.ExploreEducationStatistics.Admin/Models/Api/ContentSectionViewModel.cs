using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ContentSectionViewModel
    {
        public ContentSectionViewModel()
        {
        }

        public static ContentSectionViewModel ToViewModel(ContentSection section)
        {
            var model = new ContentSectionViewModel
            {
                Id = section.Id,
                Caption = section.Caption,
                Content = section.Content?.OrderBy(contentBlock => contentBlock.Order).ToList(),
                Heading = section.Heading,
                Order = section.Order
            };
            
            return model;
        }

        public Guid Id { get; set; }
        
        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<IContentBlock> Content { get; set; }
    }

}