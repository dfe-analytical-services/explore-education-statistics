using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class TitleAndIdViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public TitleAndIdViewModel()
        {
        }

        public TitleAndIdViewModel(Guid id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
