import { PublicationListSummary } from '@common/services/publicationService';
import { latestReleases } from '@frontend/modules/home/data/prototypeHomeReleasesData';

const mockSearchLatestReleases = () => {
  // this is a little grandiose for what should be a static list, but it's to match the API response
  const results: PublicationListSummary[] = [...latestReleases]
    .filter(release =>
      [
        'OfficialStatistics',
        'AccreditedOfficialStatistics',
        'OfficialStatisticsInDevelopment',
      ].includes(release.type),
    )
    .sort((a, b) => b.publishedDate.localeCompare(a.publishedDate))
    .map(release => ({
      id: `${release.publicationSlug}/${release.releaseSlug}`,
      slug: release.publicationSlug,
      latestReleaseSlug: release.releaseSlug,
      title: release.publicationTitle,
      summary: release.publicationSummary,
      published: release.publishedDate,
      theme: release.theme,
      type: release.type,
      rank: 0,
    }));

  return {
    paging: {
      page: 1,
      pageSize: 10,
      totalPages: 1,
      totalResults: results.length,
    },
    results,
  };
};

export default mockSearchLatestReleases;
