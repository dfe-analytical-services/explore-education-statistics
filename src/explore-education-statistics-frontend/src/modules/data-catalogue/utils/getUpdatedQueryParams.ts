import { DataSetFileFilter } from '@frontend/modules/data-catalogue/utils/dataSetFileFilters';
import { DataSetFileSortOption } from '@frontend/modules/data-catalogue/utils/dataSetFileSortOptions';
import { DataCataloguePageQuery } from '@frontend/modules/data-catalogue/DataCataloguePage';
import omit from 'lodash/omit';
import { ParsedUrlQuery } from 'querystring';
import { ReleaseSummary } from '@common/services/publicationService';

export default async function getUpdatedQueryParams({
  filterType,
  nextValue,
  query,
  releaseId,
  sortBy,
  onFetchReleases,
}: {
  filterType: DataSetFileFilter;
  nextValue: string;
  query: ParsedUrlQuery;
  releaseId?: string;
  sortBy?: DataSetFileSortOption;
  onFetchReleases?: () => Promise<ReleaseSummary[]>;
}): Promise<DataCataloguePageQuery> {
  if (filterType === 'releaseId') {
    const filterByReleaseId = nextValue !== 'latest' && nextValue !== 'all';

    return {
      ...omit(query, [
        'page',
        ...(filterByReleaseId
          ? ['latest', 'latestOnly', 'sortBy']
          : ['releaseId']),
      ]),
      ...(!filterByReleaseId && {
        latestOnly: nextValue === 'latest' ? 'true' : 'false',
      }),
      ...(filterByReleaseId && { [filterType]: nextValue }),
    };
  }

  if (nextValue === 'all') {
    return {
      ...omit(query, [
        filterType,
        'page',
        'releaseId',
        ...(filterType === 'themeId' ? ['publicationId'] : []),
      ]),
      sortBy,
    };
  }

  if (filterType === 'publicationId') {
    const releaseData = await onFetchReleases?.();

    return {
      ...omit(query, 'page', 'latestOnly'),
      [filterType]: nextValue,
      releaseId: releaseData?.find(release =>
        releaseId ? release.id === releaseId : release.latestRelease,
      )?.id,
      ...(sortBy !== 'newest' && { sortBy }),
    };
  }

  return {
    ...omit(query, [
      'page',
      ...(filterType === 'themeId' ? ['publicationId', 'releaseId'] : []),
    ]),
    [filterType]: nextValue,
    sortBy: filterType === 'searchTerm' ? 'relevance' : sortBy,
  };
}
