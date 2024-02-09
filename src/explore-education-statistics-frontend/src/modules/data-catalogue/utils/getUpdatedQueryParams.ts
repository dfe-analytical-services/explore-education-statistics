import {
  DataSetFilter,
  DataSetOrderOption,
} from '@frontend/services/dataSetService';
import { DataCataloguePageQuery } from '@frontend/modules/data-catalogue/DataCataloguePageNew';
import omit from 'lodash/omit';
import { ParsedUrlQuery } from 'querystring';
import { ReleaseSummary } from '@common/services/publicationService';

export default async function getUpdatedQueryParams({
  filterType,
  nextValue,
  orderBy,
  query,
  releaseId,
  onFetchReleases,
}: {
  filterType: DataSetFilter;
  nextValue: string;
  orderBy?: DataSetOrderOption;
  query: ParsedUrlQuery;
  releaseId?: string;
  onFetchReleases?: () => Promise<ReleaseSummary[]>;
}): Promise<DataCataloguePageQuery> {
  if (filterType === 'releaseId') {
    const filterByReleaseId = nextValue !== 'latest' && nextValue !== 'all';

    return {
      ...omit(query, [
        'page',
        ...(filterByReleaseId ? ['latest'] : ['releaseId']),
      ]),
      ...(!filterByReleaseId && {
        latest: nextValue === 'latest' ? 'true' : 'false',
      }),
      ...(filterByReleaseId && { [filterType]: nextValue }),
      orderBy,
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
      orderBy,
    };
  }

  if (filterType === 'publicationId') {
    const releaseData = await onFetchReleases?.();

    return {
      ...omit(query, 'page', 'latest'),
      [filterType]: nextValue,
      releaseId: releaseData?.find(release =>
        releaseId ? release.id === releaseId : release.latestRelease,
      )?.id,
      orderBy,
    };
  }

  return {
    ...omit(query, [
      'page',
      ...(filterType === 'themeId' ? ['publicationId', 'releaseId'] : []),
    ]),
    [filterType]: nextValue,
    orderBy: filterType === 'searchTerm' ? 'relevance' : orderBy,
  };
}
