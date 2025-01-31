using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ImageFileViewModel : Dictionary<string, string>
    {
        public ImageFileViewModel(string path)
        {
            Add("default", path);
        }
    }
}
