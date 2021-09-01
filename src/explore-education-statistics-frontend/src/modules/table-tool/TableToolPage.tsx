import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { FastTrackTable } from '@common/services/fastTrackService';
import publicationService from '@common/services/publicationService';
import tableBuilderService, {
  SelectedPublication,
  SubjectMeta,
  SubjectsAndHighlights,
} from '@common/services/tableBuilderService';
import themeService, { Theme } from '@common/services/themeService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import dynamic from 'next/dynamic';
import React, { useEffect, useMemo, useState } from 'react';

const TableToolFinalStep = dynamic(
  () => import('@frontend/modules/table-tool/components/TableToolFinalStep'),
);

export interface TableToolPageProps {
  selectedPublication?: SelectedPublication;
  subjectsAndHighlights?: SubjectsAndHighlights;
  fastTrack?: FastTrackTable;
  subjectMeta?: SubjectMeta;
  themeMeta: Theme[];
}

const TableToolPage: NextPage<TableToolPageProps> = ({
  selectedPublication,
  subjectsAndHighlights: initialSubjectsAndHighlights,
  fastTrack,
  subjectMeta,
  themeMeta,
}) => {
  const [loadingFastTrack, setLoadingFastTrack] = useState(false);
  useEffect(() => {
    if (fastTrack && subjectMeta) {
      setLoadingFastTrack(false);
    }
  }, [fastTrack, subjectMeta]);

  const initialState = useMemo<InitialTableToolState | undefined>(() => {
    if (!initialSubjectsAndHighlights) {
      return undefined;
    }

    const { subjects } = initialSubjectsAndHighlights;

    const highlights = initialSubjectsAndHighlights.highlights.filter(
      highlight => highlight.id !== fastTrack?.id,
    );

    if (fastTrack && subjectMeta) {
      const fullTable = mapFullTable(fastTrack.fullTable);
      const tableHeaders = mapTableHeadersConfig(
        fastTrack.configuration.tableHeaders,
        fullTable,
      );

      return {
        initialStep: 6,
        subjects,
        highlights,
        query: {
          ...fastTrack.query,
          releaseId: selectedPublication?.selectedRelease.id,
        },
        selectedPublication,
        subjectMeta,
        response: {
          table: fullTable,
          tableHeaders,
        },
      };
    }

    return {
      initialStep: 2,
      subjects,
      highlights,
      query: {
        publicationId: selectedPublication?.id,
        releaseId: selectedPublication?.selectedRelease.id,
        subjectId: '',
        indicators: [],
        filters: [],
        locations: {},
      },
      selectedPublication,
    };
  }, [
    selectedPublication,
    fastTrack,
    initialSubjectsAndHighlights,
    subjectMeta,
  ]);

  return (
    <Page title="Create your own tables" caption="Table Tool" wide>
      <p>
        Choose the data and area of interest you want to explore and then use
        filters to create your table.
        <br />
        Once you've created your table, you can download the data it contains
        for your own offline analysis.
      </p>

      <TableToolWizard
        key={fastTrack?.id}
        scrollOnMount
        themeMeta={themeMeta}
        initialState={initialState}
        loadingFastTrack={loadingFastTrack}
        renderHighlightLink={highlight => (
          <Link
            to="/data-tables/fast-track/[fastTrackId]"
            as={`/data-tables/fast-track/${highlight.id}`}
            onClick={() => {
              setLoadingFastTrack(true);
              logEvent({
                category: 'Table tool',
                action: 'Clicked to view featured table',
                label: `Featured table name: ${highlight.name}`,
              });
            }}
          >
            {highlight.name}
          </Link>
        )}
        finalStep={({
          query,
          response,
          selectedPublication: selectedPublicationDetails,
        }) => (
          <WizardStep size="l">
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps} isActive>
                  Explore data
                </WizardStepHeading>

                {response && query && selectedPublicationDetails && (
                  <TableToolFinalStep
                    query={query}
                    table={response.table}
                    tableHeaders={response.tableHeaders}
                    selectedPublication={selectedPublicationDetails}
                  />
                )}
              </>
            )}
          </WizardStep>
        )}
        onSubmit={table => {
          logEvent({
            category: 'Table tool',
            action: 'Publication and subject chosen',
            label: `${table.subjectMeta.publicationName}/${table.subjectMeta.subjectName}`,
          });
          logEvent({
            category: 'Table tool',
            action: 'Table created',
            label: window.location.pathname,
          });
        }}
      />
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<TableToolPageProps> = async ({
  query,
}) => {
  const { publicationSlug = '', releaseSlug = '' } = query as Dictionary<
    string
  >;

  const themeMeta = await themeService.listThemes({
    publicationFilter: 'LatestData',
  });

  const selectedPublication = themeMeta
    .flatMap(option => option.topics)
    .flatMap(option => option.publications)
    .find(option => option.slug === publicationSlug);

  if (!selectedPublication) {
    return {
      props: {
        themeMeta,
      },
    };
  }

  const latestRelease = await publicationService.getLatestPublicationReleaseSummary(
    publicationSlug,
  );

  const selectedRelease =
    !releaseSlug || latestRelease.slug === releaseSlug
      ? latestRelease
      : await publicationService.getPublicationReleaseSummary(
          publicationSlug,
          releaseSlug,
        );

  const subjectsAndHighlights = await tableBuilderService.getReleaseSubjectsAndHighlights(
    selectedRelease.id,
  );

  return {
    props: {
      themeMeta,
      selectedPublication: {
        id: selectedPublication.id,
        slug: selectedPublication.slug,
        title: selectedPublication.title,
        selectedRelease: {
          id: selectedRelease.id,
          latestData: selectedRelease.latestRelease,
          slug: selectedRelease.slug,
          title: selectedRelease.title,
        },
        latestRelease: {
          title: latestRelease.title,
        },
      },
      subjectsAndHighlights,
    },
  };
};

export default TableToolPage;
