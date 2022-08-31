import themeService, { Theme } from '@common/services/themeService';
import {
  PublicationSummaryWithRelease,
  PublicationSortOptions,
} from '@common/services/publicationService';
import { Paging } from '@common/services/types/pagination';
import { testPublications } from '@frontend/modules/find-statistics/__tests__/__data__/testPublications';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import FindStatisticsPageCurrent from './FindStatisticsPageCurrent';
import FindStatisticsPageNew from './FindStatisticsPageNew';

interface Props {
  newDesign?: boolean; // TODO EES-3517 flag
  paging?: Paging | null; // TODO EES-3517 won't be optional or null
  publications?: PublicationSummaryWithRelease[]; // TODO EES-3517 won't be optional
  sortBy?: PublicationSortOptions;
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
  const { newDesign, page, sortBy } = query;
  const currentPage = typeof page === 'string' ? parseInt(page, 10) : 1;
  const currentSortBy = sortBy as PublicationSortOptions;

  // TODO EES-3517 - fetch publications here, using pagination and filters from query.
  // const publicationsResponse = newDesign ? await publicationService.getPublications({
  //   page: currentPage ?? 1,
  //   pageSize: 10,
  //   sortBy: currentSortBy ?? 'newest'
  // }) : []
  // Will need to handle if the requested page doesn't exist.
  // Fake response for now
  const publicationsResponse = {
    paging: {
      page: currentPage ?? 1,
      pageSize: 10,
      totalResults: 100,
      totalPages: 10,
    },
    results: currentPage === 1 ? testPublications : [testPublications[1]], // faking different page to test pagination
  };

  // TODO EES-3517 - remove themes
  const themes = newDesign
    ? []
    : await themeService.listThemes({
        publicationFilter: 'FindStatistics',
      });

  return {
    props: {
      newDesign: !!newDesign,
      paging: newDesign ? publicationsResponse.paging : null,
      publications: newDesign ? publicationsResponse.results : [],
      sortBy: currentSortBy ?? null,
      themes,
    },
  };
};

export default FindStatisticsPage;
