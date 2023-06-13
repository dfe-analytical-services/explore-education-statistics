import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { SelectedPublication } from '@common/modules/table-tool/types/selectedPublication';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService, {
  FastTrackTable,
  FeaturedTable,
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
import { useRouter } from 'next/router';

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
  newPermalinks?: boolean; // TO DO - EES-4259 remove `newPermalinks` param and tidy up
}

const TableToolPage: NextPage<TableToolPageProps> = ({
  fastTrack,
  featuredTables = [],
  selectedPublication,
  subjects,
  subjectMeta,
  themeMeta,
  newPermalinks,
}) => {
  const router = useRouter();
  const [loadingFastTrack, setLoadingFastTrack] = useState(false);
  const [currentStep, setCurrentStep] = useState<number | undefined>(undefined);

  useEffect(() => {
    // Intercept the back button and activate the appropriate step
    router.beforePopState(({ url }) => {
      if (url === '/data-tables') {
        // going back to publication step
        setCurrentStep(1);
      } else if (url.startsWith('/data-tables/[publicationSlug]')) {
        // clicking back on any step after step 2 should take you to step 2
        setCurrentStep(2);
      }
      return true;
    });
  }, [router]);

  useEffect(() => {
    if (fastTrack && subjectMeta) {
      setLoadingFastTrack(false);
    }
  }, [fastTrack, subjectMeta]);

  const initialState = useMemo<InitialTableToolState | undefined>(() => {
    if (!subjects) {
      return undefined;
    }

    if (fastTrack && subjectMeta) {
      const fullTable = mapFullTable(fastTrack.fullTable);
      const tableHeaders = mapTableHeadersConfig(
        fastTrack.configuration.tableHeaders,
        fullTable,
      );

      return {
        initialStep: 6,
        subjects,
        featuredTables,
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
      featuredTables,
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
        renderFeaturedTableLink={featuredTable => (
          <Link
            to={`/data-tables/fast-track/${featuredTable.dataBlockId}`}
            onClick={() => {
              setLoadingFastTrack(true);
              setCurrentStep(undefined);
              logEvent({
                category: 'Table tool',
                action: 'Clicked to view featured table',
                label: `Featured table name: ${featuredTable.name}`,
              });
            }}
          >
            {featuredTable.name}
          </Link>
        )}
        currentStep={currentStep}
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
                        newPermalinks={newPermalinks}
                      />
                    )}
                </>
              )}
            </WizardStep>
          );
        }}
        onPublicationFormSubmit={publication => {
          router.push(
            {
              pathname: `/data-tables/${publication.slug}`,
              query: newPermalinks ? { newPermalinks } : undefined,
            },
            undefined,
            {
              shallow: true,
              scroll: false,
            },
          );
        }}
        onPublicationStepBack={async () => {
          await router.push('/data-tables', undefined, { shallow: true });
        }}
        onStepChange={() => setCurrentStep(undefined)}
        onSubjectFormSubmit={async ({ publication, release, subjectId }) => {
          await router.push(
            {
              pathname: `/data-tables/${publication.slug}/${release.slug}`,
              query: newPermalinks
                ? { subjectId, newPermalinks }
                : { subjectId },
            },
            undefined,
            {
              shallow: true,
              scroll: false,
            },
          );
        }}
        onSubjectStepBack={async publication => {
          await router.push(
            {
              pathname: `/data-tables/${publication?.slug}`,
              query: newPermalinks ? { newPermalinks } : undefined,
            },
            undefined,
            {
              shallow: true,
              scroll: false,
            },
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
        onTableQueryError={(errorCode, publicationTitle, subjectName) => {
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
      />
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<TableToolPageProps> = async ({
  query,
}) => {
  const {
    publicationSlug = '',
    releaseSlug = '',
    newPermalinks,
  } = query as Dictionary<string>;

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
        newPermalinks: !!newPermalinks,
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
      newPermalinks: !!newPermalinks,
    },
  };
};

export default TableToolPage;
