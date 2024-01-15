import publicationService, {
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
import dataSetQueries from '@frontend/queries/dataSetQueries';
import { QueryClient, dehydrate } from '@tanstack/react-query';
import publicationQueries from '@frontend/queries/publicationQueries';

interface Props {
  releases?: ReleaseSummary[];
  selectedPublication?: PublicationTreeSummary;
  selectedRelease?: ReleaseSummary;
  subjects?: Subject[];
  themes?: Theme[];
  newDesign?: boolean;
}

const DataCataloguePage: NextPage<Props> = ({
  releases = [],
  selectedPublication,
  selectedRelease,
  subjects = [],
  themes = [],
  newDesign,
}) => {
  // TO DO EES-4781 - remove old version
  if (newDesign) {
    return <DataCataloguePageNew />;
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

  const queryClient = new QueryClient();

  if (newDesign) {
    await queryClient.prefetchQuery(dataSetQueries.list(context.query));
    await queryClient.prefetchQuery(
      publicationQueries.getPublicationTree({
        publicationFilter: 'DataCatalogue',
      }),
    );
  }

  const themes = newDesign
    ? []
    : await publicationService.getPublicationTree({
        publicationFilter: 'DataCatalogue',
      });

  const selectedPublication = themes
    .flatMap(option => option.topics)
    .flatMap(option => option.publications)
    .find(option => option.slug === publicationSlug);

  let releases: ReleaseSummary[] = [];

  if (selectedPublication) {
    releases = newDesign
      ? []
      : await publicationService.listReleases(publicationSlug);
  }

  let selectedRelease: ReleaseSummary | undefined;

  if (releaseSlug) {
    selectedRelease = releases.find(rel => rel.slug === releaseSlug);
  }

  let subjects: Subject[] = [];

  if (selectedPublication && selectedRelease) {
    subjects = await tableBuilderService.listReleaseSubjects(
      selectedRelease.id,
    );
  }

  const props: Props = {
    releases,
    subjects,
    themes,
    newDesign: !!newDesign,
  };

  if (selectedPublication) {
    props.selectedPublication = selectedPublication;

    if (selectedRelease) {
      props.selectedRelease = selectedRelease;
    }
  }

  return {
    props: { ...props, dehydratedState: dehydrate(queryClient) },
  };
};

export default DataCataloguePage;
