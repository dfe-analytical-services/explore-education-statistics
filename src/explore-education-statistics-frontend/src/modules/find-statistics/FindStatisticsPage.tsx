import publicationService, {
  PublicationListSummary,
  PublicationSortOption,
  Theme,
} from '@common/services/publicationService';
import themeService, { ThemeSummary } from '@common/services/themeService';
import { Paging } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import FindStatisticsPageCurrent from './FindStatisticsPageCurrent';
import FindStatisticsPageNew from './FindStatisticsPageNew';

export interface FindStatisticsPageQuery {
  page?: number;
  releaseType?: ReleaseType;
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string;
}

interface Props {
  newDesign?: boolean; // TODO EES-3517 flag
  paging?: Paging | null; // TODO EES-3517 won't be optional or null
  publications?: PublicationListSummary[]; // TODO EES-3517 won't be optional
  query: FindStatisticsPageQuery;
  themes: Theme[] | ThemeSummary[];
}

const FindStatisticsPage: NextPage<Props> = ({
  newDesign = false,
  paging,
  publications,
  query,
  themes,
}) => {
  // TODO EES-3517 remove these and move FindStatisticsPageNew into here
  if (newDesign && paging && publications) {
    return (
      <FindStatisticsPageNew
        paging={paging}
        publications={publications}
        query={query}
        themes={themes as ThemeSummary[]} // TODO EES-3517 won't need `as ThemeSummary[]` when only have one theme type
      />
    );
  }
  return <FindStatisticsPageCurrent themes={themes as Theme[]} />;
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { newDesign } = query;

  // TODO EES-3517 - remove newDesign check
  const publicationsResponse = newDesign
    ? await publicationService.listPublications(
        createPublicationListRequest(query),
      )
    : {
        results: [],
        paging: {
          page: 0,
          pageSize: 0,
          totalResults: 0,
          totalPages: 0,
        },
      };

  // TODO EES-3517 - remove newDesign check
  const themes = newDesign
    ? await themeService.listThemes()
    : await publicationService.getPublicationTree({
        publicationFilter: 'FindStatistics',
      });

  return {
    props: {
      newDesign: !!newDesign,
      paging: newDesign ? publicationsResponse.paging : null,
      publications: newDesign ? publicationsResponse.results : [],
      query,
      themes,
    },
  };
};

export default FindStatisticsPage;
