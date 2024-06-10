import {
  PublicationTreeSummary,
  ReleaseSummary,
  Theme,
} from '@common/services/publicationService';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import DataCataloguePageCurrent from '@frontend/modules/data-catalogue/DataCataloguePageCurrent';
import DataCataloguePageNew from '@frontend/modules/data-catalogue/DataCataloguePageNew';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import { QueryClient, dehydrate } from '@tanstack/react-query';
import publicationQueries from '@frontend/queries/publicationQueries';

interface Props {
  showTypeFilter?: boolean;
  releases?: ReleaseSummary[];
  selectedPublication?: PublicationTreeSummary;
  selectedRelease?: ReleaseSummary;
  subjects?: Subject[];
  themes?: Theme[];
  newDesign?: boolean;
}

const DataCataloguePage: NextPage<Props> = ({
  showTypeFilter,
  releases = [],
  selectedPublication,
  selectedRelease,
  subjects = [],
  themes = [],
  newDesign,
}) => {
  // TO DO EES-4781 - remove old version
  if (newDesign) {
    return <DataCataloguePageNew showTypeFilter={showTypeFilter} />;
  }
  return (
    <DataCataloguePageCurrent
      releases={releases}
      selectedPublication={selectedPublication}
      selectedRelease={selectedRelease}
      subjects={subjects}
      themes={themes}
    />
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async context => {
  const {
    publicationSlug = '',
    releaseSlug = '',
    newDesign,
  } = context.query as Dictionary<string>;

  let showTypeFilter = context.query.dataSetType === 'api';

  const queryClient = new QueryClient();

  const themes = await queryClient.fetchQuery(
    publicationQueries.getPublicationTree({
      publicationFilter: 'DataCatalogue',
    }),
  );

  const selectedPublication = themes
    .flatMap(option => option.topics)
    .flatMap(option => option.publications)
    .find(option => option.slug === publicationSlug);

  let releases: ReleaseSummary[] = [];
  let selectedRelease: ReleaseSummary | undefined;

  if (selectedPublication) {
    releases = await queryClient.fetchQuery(
      publicationQueries.listReleases(selectedPublication.slug),
    );

    selectedRelease = releaseSlug
      ? releases.find(rel => rel.slug === releaseSlug)
      : undefined;

    // Redirect old slug based links to new format.
    if (newDesign) {
      const redirectUrlQuery = selectedRelease
        ? `publicationId=${selectedPublication.id}&releaseId=${selectedRelease.id}`
        : `publicationId=${selectedPublication.id}`;
      return {
        redirect: {
          destination: `/data-catalogue?${redirectUrlQuery}&newDesign=true`,
          permanent: true,
        },
      };
    }
  }

  if (newDesign) {
    await queryClient.prefetchQuery(dataSetFileQueries.list(context.query));

    if (!showTypeFilter) {
      const apiDataSets = await queryClient.fetchQuery(
        dataSetFileQueries.list({ dataSetType: 'api' }),
      );
      showTypeFilter = !!apiDataSets.results.length;
    }
  }

  let subjects: Subject[] = [];

  if (selectedPublication && selectedRelease) {
    subjects = await tableBuilderService.listReleaseSubjects(
      selectedRelease.id,
    );
  }

  const props: Props = {
    showTypeFilter,
    releases,
    subjects,
    themes,
    ...(selectedPublication && { selectedPublication }),
    ...(selectedRelease && { selectedRelease }),
    newDesign: !!newDesign,
  };

  return {
    props: { ...props, dehydratedState: dehydrate(queryClient) },
  };
};

export default DataCataloguePage;
