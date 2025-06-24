import { odata } from '@azure/search-documents';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { FindStatisticsPageQuery } from '@frontend/modules/find-statistics/FindStatisticsPage';
import {
  PublicationSortOption,
  publicationSortOptions,
} from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import {
  AzurePublicationListRequest,
  AzurePublicationOrderByParam,
} from '@frontend/services/azurePublicationService';
import omitBy from 'lodash/omitBy';

const escapeSpecialCharacters = (providedString: string) => {
  // eslint-disable-next-line no-useless-escape
  const specialCharactersToEscape = /([`"<>#%{}+-;/?:@=+&~\*\|\\\/\^\[\]])/g;

  return providedString.replaceAll(specialCharactersToEscape, match => {
    // If the matched character is a backslash, we just return a single escaped backslash (\\)
    // Otherwise, we prepend a backslash to the matched character.
    return match === '\\' ? '\\\\' : `\\${match}`;
  });
};

export default function createAzurePublicationListRequest(
  query: FindStatisticsPageQuery,
): AzurePublicationListRequest {
  const {
    releaseType,
    search: searchParam,
    sortBy,
    themeId,
  } = getParamsFromQuery(query);

  const orderBy = getSortParam(sortBy);

  let filter: string | undefined;
  if (releaseType && themeId) {
    filter = odata`releaseType eq ${releaseType} and themeId eq ${themeId}`;
  } else if (releaseType) {
    filter = odata`releaseType eq ${releaseType}`;
  } else if (themeId) {
    filter = odata`themeId eq ${themeId}`;
  }

  const search =
    searchParam
      ?.split(' ')
      // We need to escape unsafe/special characters,
      .map(escapeSpecialCharacters)
      // then append ~ to enable fuzzy matching
      // https://learn.microsoft.com/en-gb/azure/search/query-lucene-syntax
      .map(safeWord => (safeWord.length < 3 ? safeWord : `${safeWord}~`))
      .join(' OR ') ?? '';

  return omitBy(
    {
      filter,
      page: parseNumber(query.page) ?? 1,
      releaseType,
      search,
      orderBy,
      themeId,
    },
    value => typeof value === 'undefined',
  );
}

function getSortParam(
  sortBy: PublicationSortOption,
): AzurePublicationOrderByParam {
  switch (sortBy) {
    case 'relevance':
      return undefined;
    case 'title':
      return 'title asc';
    case 'oldest':
      return 'published asc';
    default:
      return 'published desc';
  }
}

export function getParamsFromQuery(query: FindStatisticsPageQuery) {
  return {
    page: getFirst(query.page),
    releaseType:
      query.releaseType &&
      isOneOf(query.releaseType, Object.keys(releaseTypes) as ReleaseType[])
        ? query.releaseType
        : undefined,
    search: getFirst(query.search),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, publicationSortOptions)
        ? query.sortBy
        : 'newest',
    themeId: getFirst(query.themeId),
  };
}
