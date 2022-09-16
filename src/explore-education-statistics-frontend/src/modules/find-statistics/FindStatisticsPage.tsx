import themeService, { Theme } from '@common/services/themeService';
import {
  PublicationSummaryWithRelease,
  PublicationSortOption,
  publicationSortOptions,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import FindStatisticsPageCurrent from './FindStatisticsPageCurrent';
import FindStatisticsPageNew from './FindStatisticsPageNew';

interface Props {
  newDesign?: boolean; // TODO EES-3517 flag
  paging?: Paging | null; // TODO EES-3517 won't be optional or null
  publications?: PublicationSummaryWithRelease[]; // TODO EES-3517 won't be optional
  sortBy?: PublicationSortOption;
  themes: Theme[];
}

const FindStatisticsPage: NextPage<Props> = ({
  newDesign = false,
  paging,
  publications,
  sortBy,
  themes = [],
}) => {
  // TODO EES-3517 remove these and move FindStatisticsPageNew into here
  if (newDesign && paging && publications) {
    return (
      <FindStatisticsPageNew
        paging={paging}
        publications={publications}
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
  const page = parseNumber(query.page) ?? 1;
  const sortBy = isOneOf(query.sortBy, publicationSortOptions)
    ? query.sortBy
    : 'newest';

  // TODO EES-3517 - fetch publications here, using pagination and filters from query.
  // const publicationsResponse = newDesign ? await publicationService.getPublications({
  //   page,
  //   pageSize: 10,
  //   sortBy
  // }) : []

  // Will need to handle if the requested page doesn't exist.
  // Fake response for now
  const paging: Paging = {
    page,
    pageSize: 10,
    totalResults: 100,
    totalPages: 10,
  };

  const publications: PublicationSummaryWithRelease[] =
    page === 1 ? testPublications : [testPublications[1]]; // faking different page to test pagination

  // TODO EES-3517 - remove themes
  const themes = newDesign
    ? []
    : await themeService.listThemes({
        publicationFilter: 'FindStatistics',
      });

  return {
    props: {
      newDesign: !!newDesign,
      paging: newDesign ? paging : null,
      publications: newDesign ? publications : [],
      sortBy,
      themes,
    },
  };
};

export default FindStatisticsPage;
