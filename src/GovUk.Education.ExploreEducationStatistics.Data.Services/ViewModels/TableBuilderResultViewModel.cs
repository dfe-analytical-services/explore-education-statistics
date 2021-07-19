using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class TableBuilderResultViewModel
    {
        public ResultSubjectMetaViewModel SubjectMeta { get; set; }

        public IEnumerable<ObservationViewModel> Results { get; set; }

        public TableBuilderResultViewModel()
        {
            Results = new List<ObservationViewModel>();
        }
    }

    public class TableBuilderResultCacheKey : ICacheKey<TableBuilderResultViewModel>
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }
        private Guid DataBlockId { get; }

        public TableBuilderResultCacheKey(ReleaseContentBlock releaseContentBlock)
        {
            if (!(releaseContentBlock.ContentBlock is DataBlock))
            {
                throw new ArgumentException("Attempting to build data block cache key with incorrect type of content block");
            }

            var release = releaseContentBlock.Release;
            PublicationSlug = release.Publication.Slug;
            ReleaseSlug = release.Slug;
            DataBlockId = releaseContentBlock.ContentBlockId;
        }

        public string Key => FileStoragePathUtils.PublicContentDataBlockPath(
            PublicationSlug,
            ReleaseSlug,
            DataBlockId
        );
    }
}
