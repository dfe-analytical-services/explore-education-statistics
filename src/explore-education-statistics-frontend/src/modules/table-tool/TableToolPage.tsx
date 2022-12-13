import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService, {
  FastTrackTable,
  FeaturedTable,
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import publicationService, { Theme } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import dynamic from 'next/dynamic';
import React, { useEffect, useMemo, useState } from 'react';
import { TableQueryErrorCode } from '@common/modules/table-tool/components/FiltersForm';

const TableToolFinalStep = dynamic(
  () => import('@frontend/modules/table-tool/components/TableToolFinalStep'),
);

export interface TableToolPageProps {
  fastTrack?: FastTrackTable;
  featuredTables?: FeaturedTable[];
  selectedPublication?: SelectedPublication;
  subjects?: Subject[];
  subjectMeta?: SubjectMeta;
  themeMeta: Theme[];
}

const TableToolPage: NextPage<TableToolPageProps> = ({
  fastTrack,
  featuredTables = [],
  selectedPublication,
  subjects,
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
    if (!subjects) {
      return undefined;
    }

    const filteredFeaturedTables = featuredTables.filter(
      table => table.id !== fastTrack?.id,
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
        featuredTables: filteredFeaturedTables,
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
      featuredTables: filteredFeaturedTables,
      query: {
        publicationId: selectedPublication?.id,
        releaseId: selectedPublication?.selectedRelease.id,
        subjectId: '',
        indicators: [],
        filters: [],
        locationIds: [],
      },
      selectedPublication,
    };
  }, [selectedPublication, fastTrack, subjects, featuredTables, subjectMeta]);

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
        renderFeaturedTable={highlight => (
          <Link
            to={`/data-tables/fast-track/${highlight.id}`}
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
        onTableQueryError={(
          errorCode: TableQueryErrorCode,
          publicationTitle: string,
          subjectName: string,
        ) => {
          switch (errorCode) {
            case 'QueryExceedsMaxAllowableTableSize': {
              logEvent({
                category: 'Table tool size error',
                action: 'Table exceeded maximum size',
                label: `${publicationTitle}/${subjectName}`,
              });
              break;
            }
            case 'RequestCancelled': {
              logEvent({
                category: 'Table tool query timeout error',
                action: 'Table exceeded maximum timeout duration',
                label: `${publicationTitle}/${subjectName}`,
              });
              break;
            }
            default:
              break;
          }
        }}
        finalStep={({
          query,
          selectedPublication: selectedPublicationDetails,
          table,
          tableHeaders,
          onReorder,
        }) => {
          return (
            <WizardStep size="l">
              {wizardStepProps => (
                <>
                  <WizardStepHeading {...wizardStepProps} isActive>
                    Explore data
                  </WizardStepHeading>

                  {table &&
                    tableHeaders &&
                    query &&
                    selectedPublicationDetails && (
                      <TableToolFinalStep
                        query={query}
                        selectedPublication={selectedPublicationDetails}
                        table={table}
                        tableHeaders={tableHeaders}
                        onReorderTableHeaders={onReorder}
                      />
                    )}
                </>
              )}
            </WizardStep>
          );
        }}
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

  const themeMeta = await publicationService.getPublicationTree({
    publicationFilter: 'DataTables',
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

  const [subjects, featuredTables] = await Promise.all([
    tableBuilderService.listReleaseSubjects(selectedRelease.id),
    tableBuilderService.listReleaseFeaturedTables(selectedRelease.id),
  ]);

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
      subjects,
      featuredTables,
    },
  };
};

export default TableToolPage;
