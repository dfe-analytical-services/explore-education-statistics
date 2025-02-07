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
  releaseVersionId,
  sortBy,
  onFetchReleases,
}: {
  filterType: DataSetFileFilter;
  nextValue: string;
  query: ParsedUrlQuery;
  releaseVersionId?: string;
  sortBy?: DataSetFileSortOption;
  onFetchReleases?: () => Promise<ReleaseSummary[]>;
}): Promise<DataCataloguePageQuery> {
  if (filterType === 'releaseVersionId') {
    const filterByReleaseVersionId =
      nextValue !== 'latest' && nextValue !== 'all';

    return {
      ...omit(query, [
        'page',
        ...(filterByReleaseVersionId
          ? ['latest', 'latestOnly', 'sortBy']
          : ['releaseVersionId']),
      ]),
      ...(!filterByReleaseVersionId && {
        latestOnly: nextValue === 'latest' ? 'true' : 'false',
      }),
      ...(filterByReleaseVersionId && { [filterType]: nextValue }),
    };
  }

  if (nextValue === 'all') {
    return {
      ...omit(query, [
        filterType,
        'page',
        'releaseVersionId',
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
      releaseVersionId: releaseData?.find(release =>
        releaseVersionId
          ? release.id === releaseVersionId
          : release.latestRelease,
      )?.id,
      ...(sortBy !== 'newest' && { sortBy }),
    };
  }

  return {
    ...omit(query, [
      'page',
      ...(filterType === 'themeId'
        ? ['publicationId', 'releaseVersionId']
        : []),
    ]),
    [filterType]: nextValue,
    sortBy: filterType === 'searchTerm' ? 'relevance' : sortBy,
  };
}
