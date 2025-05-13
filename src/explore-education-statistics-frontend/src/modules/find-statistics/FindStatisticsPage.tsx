import React from 'react';
import { GetServerSideProps, NextPage } from 'next';
import { dehydrate, QueryClient } from '@tanstack/react-query';
import { ReleaseType } from '@common/services/types/releaseType';
import { PublicationSortOption } from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import publicationQueries from '@frontend/queries/publicationQueries';
import themeQueries from '@frontend/queries/themeQueries';
import azurePublicationQueries from '@frontend/queries/azurePublicationQueries';
import FindStatisticsPageCurrent from './FindStatisticsPageCurrent';
import FindStatisticsPageAzure from './FindStatisticsPageAzure';

export interface FindStatisticsPageQuery {
  page?: number;
  releaseType?: ReleaseType;
  search?: string;
  sortBy?: PublicationSortOption;
  themeId?: string;
}

interface Props {
  useAzureSearch?: boolean;
}

const FindStatisticsPage: NextPage<Props> = ({ useAzureSearch = false }) => {
  return useAzureSearch ? (
    <FindStatisticsPageAzure />
  ) : (
    <FindStatisticsPageCurrent />
  );
};

export const getServerSideProps: GetServerSideProps = async ({ query }) => {
  const { azsearch } = query;
  const queryClient = new QueryClient();

  await Promise.all([
    azsearch
      ? queryClient.prefetchQuery(azurePublicationQueries.listAzure(query))
      : queryClient.prefetchQuery(publicationQueries.list(query)),
    queryClient.prefetchQuery(themeQueries.list()),
  ]);

  return {
    props: {
      useAzureSearch: !!azsearch,
      dehydratedState: dehydrate(queryClient),
    },
  };
};

export default FindStatisticsPage;
