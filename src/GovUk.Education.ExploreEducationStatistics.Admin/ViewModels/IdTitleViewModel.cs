using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record IdTitleViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public IdTitleViewModel()
        {
        }

        public IdTitleViewModel(Guid id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
