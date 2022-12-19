import publicationService, {
  PublicationSortOption,
  Theme,
} from '@common/services/publicationService';
import { ReleaseType } from '@common/services/types/releaseType';
import publicationQueries from '@frontend/queries/publicationQueries';
import themeQueries from '@frontend/queries/themeQueries';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import { dehydrate, QueryClient } from '@tanstack/react-query';
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
  themes: Theme[]; // TODO EES-3517 remove
}

const FindStatisticsPage: NextPage<Props> = ({ newDesign = false, themes }) => {
  // TODO EES-3517 remove these and move FindStatisticsPageNew into here
  if (newDesign) {
    return <FindStatisticsPageNew />;
  }
  return <FindStatisticsPageCurrent themes={themes as Theme[]} />;
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { newDesign } = query;

  // TODO EES-3517 - remove this
  const themes = newDesign
    ? []
    : await publicationService.getPublicationTree({
        publicationFilter: 'FindStatistics',
      });

  const queryClient = new QueryClient();

  // TODO EES-3517 - remove newDesign check
  if (newDesign) {
    await Promise.all([
      queryClient.prefetchQuery(publicationQueries.list(query)),
      queryClient.prefetchQuery(themeQueries.list()),
    ]);
  }

  return {
    props: {
      newDesign: !!newDesign,
      themes,
      dehydratedState: dehydrate(queryClient),
    },
  };
};

export default FindStatisticsPage;
