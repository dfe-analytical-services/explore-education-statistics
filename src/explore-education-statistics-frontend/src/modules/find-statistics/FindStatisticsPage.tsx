import publicationService, {
  PublicationListRequest,
  PublicationListSummary,
  PublicationOrderParam,
  PublicationSortOption,
  publicationSortOptions,
  PublicationSortParam,
  Theme,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import FindStatisticsPageCurrent from './FindStatisticsPageCurrent';
import FindStatisticsPageNew from './FindStatisticsPageNew';

interface Props {
  newDesign?: boolean; // TODO EES-3517 flag
  paging?: Paging | null; // TODO EES-3517 won't be optional or null
  publications?: PublicationListSummary[]; // TODO EES-3517 won't be optional
  search?: string;
  sortBy?: PublicationSortOption;
  themes: Theme[];
}

const FindStatisticsPage: NextPage<Props> = ({
  newDesign = false,
  paging,
  publications,
  search,
  sortBy,
  themes = [],
}) => {
  // TODO EES-3517 remove these and move FindStatisticsPageNew into here
  if (newDesign && paging && publications) {
    return (
      <FindStatisticsPageNew
        paging={paging}
        publications={publications}
        searchTerm={search}
        sortBy={sortBy}
      />
    );
  }
  return <FindStatisticsPageCurrent themes={themes} />;
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { newDesign } = query;
  const sortBy = isOneOf(query.sortBy, publicationSortOptions)
    ? query.sortBy
    : 'newest';
  const { order, sort } = getSortParams(sortBy);
  const minSearchCharacters = 3;
  const searchQuery = Array.isArray(query.search)
    ? query.search[0]
    : query.search;
  const search =
    searchQuery && searchQuery.length >= minSearchCharacters ? searchQuery : '';

  const params: PublicationListRequest = {
    order,
    page: parseNumber(query.page) ?? 1,
    sort,
    ...(search && { search }),
  };

  // TODO EES-3517 - remove newDesign check
  const publicationsResponse = newDesign
    ? await publicationService.listPublications(params)
    : {
        results: [],
        paging: {
          page: 0,
          pageSize: 0,
          totalResults: 0,
          totalPages: 0,
        },
      };

  // TODO EES-3517 - remove themes
  const themes = newDesign
    ? []
    : await publicationService.getPublicationTree({
        publicationFilter: 'FindStatistics',
      });

  return {
    props: {
      newDesign: !!newDesign,
      paging: newDesign ? publicationsResponse.paging : null,
      publications: newDesign ? publicationsResponse.results : [],
      search,
      sortBy,
      themes,
    },
  };
};

export default FindStatisticsPage;

function getSortParams(
  sortBy: PublicationSortOption,
): {
  order?: PublicationOrderParam;
  sort?: PublicationSortParam;
} {
  if (sortBy === 'title') {
    return {
      order: 'asc',
      sort: 'title',
    };
  }
  if (sortBy === 'oldest') {
    return {
      order: 'asc',
      sort: 'published',
    };
  }
  return {
    order: 'desc',
    sort: 'published',
  };
}
