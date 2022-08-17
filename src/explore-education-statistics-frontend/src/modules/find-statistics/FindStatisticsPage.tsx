import themeService, { Theme } from '@common/services/themeService';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import FindStatisticsPageCurrent from './FindStatisticsPageCurrent';
import FindStatisticsPageNew from './FindStatisticsPageNew';

interface Props {
  newDesign?: boolean; // TODO EES-3517 flag
  paging?: {
    // TODO EES-3517 - not optional
    page: number;
    pageSize: number;
    totalResults: number;
    totalPages: number;
  };
  themes: Theme[];
}

const FindStatisticsPage: NextPage<Props> = ({
  newDesign = false,
  paging,
  themes = [],
}) => {
  // TODO EES-3517 remove these and move FindStatisticsPageNew into here
  if (!newDesign) {
    return <FindStatisticsPageCurrent themes={themes} />;
  }
  return <FindStatisticsPageNew paging={paging} />;
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const initialPage =
    typeof query.page === 'string' ? parseInt(query.page, 10) : null;

  // TODO EES-3517 - fetch publications here, using pagination and filters from query.
  // const publicationsResponse = query.newDesign ? await publicationService.getPublications({
  //   page: initialPage ?? 1,
  //   pageSize: 10,
  // }) : []
  // Fake response for now
  const publicationsResponse = {
    paging: {
      page: initialPage ?? 1,
      pageSize: 10,
      totalResults: 100,
      totalPages: 10,
    },
    results: [],
  };

  // TODO EES-3517 - remove themes
  const themes = query.newDesign
    ? []
    : await themeService.listThemes({
        publicationFilter: 'FindStatistics',
      });

  return {
    props: {
      newDesign: !!query.newDesign,
      paging: publicationsResponse.paging,
      // publications: publicationsResponse.results, // TODO EES-3517
      themes,
    },
  };
};

export default FindStatisticsPage;
