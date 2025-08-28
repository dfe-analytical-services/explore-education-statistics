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
import publicationService, {
  ReleaseVersion,
  Theme,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import TableToolInfoWrapper from '@frontend/modules/table-tool/components/TableToolInfoWrapper';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import dynamic from 'next/dynamic';
import React, { useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/router';

const defaultPageTitle = 'Create your own tables';

const TableToolFinalStep = dynamic(
  () => import('@frontend/modules/table-tool/components/TableToolFinalStep'),
);

export interface TableToolPageProps {
  fastTrack?: FastTrackTable;
  featuredTables?: FeaturedTable[];
  selectedPublication?: SelectedPublication;
  fullPublication?: ReleaseVersion;
  selectedSubjectId?: string;
  subjects?: Subject[];
  subjectMeta?: SubjectMeta;
  themeMeta: Theme[];
}

const TableToolPage: NextPage<TableToolPageProps> = ({
  fastTrack,
  featuredTables = [],
  selectedPublication,
  fullPublication,
  selectedSubjectId,
  subjects,
  subjectMeta,
  themeMeta,
}) => {
  const router = useRouter();
  const [loadingFastTrack, setLoadingFastTrack] = useState(false);
  const [currentStep, setCurrentStep] = useState<number | undefined>(undefined);
  const [basePageTitle, setBasePageTitle] = useState<string>(
    selectedPublication?.title
      ? `${defaultPageTitle} on ${selectedPublication?.title.toLocaleLowerCase()}`
      : defaultPageTitle,
  );
  const [pageTitle, setPageTitle] = useState<string>(basePageTitle);

  useEffect(() => {
    const newBasePageTitle = selectedPublication?.title
      ? `${defaultPageTitle} on ${selectedPublication?.title.toLocaleLowerCase()}`
      : defaultPageTitle;
    setBasePageTitle(newBasePageTitle);
    setPageTitle(newBasePageTitle);
  }, [selectedPublication]);

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
          releaseVersionId: selectedPublication?.selectedRelease.id,
        },
        selectedPublication,
        subjectMeta,
        response: {
          table: fullTable,
          tableHeaders,
        },
      };
    }

    if (selectedSubjectId && subjectMeta) {
      return {
        initialStep: 3,
        subjects,
        featuredTables,
        query: {
          publicationId: selectedPublication?.id,
          releaseVersionId: selectedPublication?.selectedRelease.id,
          subjectId: selectedSubjectId,
          indicators: [],
          filters: [],
          locationIds: [],
        },
        selectedPublication,
        subjectMeta,
      };
    }

    return {
      initialStep: 2,
      subjects,
      featuredTables,
      query: {
        publicationId: selectedPublication?.id,
        releaseVersionId: selectedPublication?.selectedRelease.id,
        subjectId: '',
        indicators: [],
        filters: [],
        locationIds: [],
      },
      selectedPublication,
    };
  }, [
    subjects,
    fastTrack,
    subjectMeta,
    selectedSubjectId,
    featuredTables,
    selectedPublication,
  ]);

  return (
    <Page
      // Don't include the default meta title after intitial step to prevent too much screen reader noise.
      includeDefaultMetaTitle={pageTitle === basePageTitle}
      metaTitle={pageTitle}
      title={basePageTitle}
      description={
        selectedPublication?.title
          ? `Create and download your own custom data tables by choosing your areas of interest using filters to build your table from ${selectedPublication.title.toLocaleLowerCase()}`
          : undefined
      }
      caption="Table Tool"
      wide
    >
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
            to={`/data-tables/fast-track/${featuredTable.dataBlockParentId}`}
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
        renderRelatedInfo={
          selectedPublication && (
            <TableToolInfoWrapper
              selectedPublication={selectedPublication}
              fullPublication={fullPublication}
            />
          )
        }
        currentStep={currentStep}
        finalStep={({
          query,
          selectedPublication: selectedPublicationDetails,
          stepTitle,
          table,
          tableHeaders,
          onReorder,
        }) => {
          return (
            <WizardStep size="l">
              {wizardStepProps => (
                <>
                  <WizardStepHeading {...wizardStepProps} isActive>
                    {stepTitle}
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
        onPublicationFormSubmit={publication => {
          router.push(`/data-tables/${publication.slug}`, undefined, {
            shallow: true,
            scroll: false,
          });
        }}
        onPublicationStepBack={async () => {
          await router.push('/data-tables', undefined, { shallow: true });
        }}
        onStepChange={() => {
          setCurrentStep(undefined);
        }}
        onStepSubmit={({ nextStepNumber, nextStepTitle }) =>
          setPageTitle(
            `Step ${nextStepNumber}: ${nextStepTitle} - ${basePageTitle}`,
          )
        }
        onSubjectFormSubmit={async ({ publication, release, subjectId }) => {
          await router.push(
            `/data-tables/${publication.slug}/${release.slug}?subjectId=${subjectId}`,
            undefined,
            {
              shallow: true,
              scroll: false,
            },
          );
        }}
        onSubjectStepBack={async publication => {
          await router.push(`/data-tables/${publication?.slug}`, undefined, {
            shallow: true,
            scroll: false,
          });
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

export const getServerSideProps: GetServerSideProps<
  TableToolPageProps
> = async ({ query }) => {
  const {
    publicationSlug = '',
    releaseSlug = '',
    subjectId = '',
  } = query as Dictionary<string>;

  const themeMeta = await publicationService.getPublicationTree({
    publicationFilter: 'DataTables',
  });

  const selectedPublication = themeMeta
    .flatMap(option => option.publications)
    .find(option => option.slug === publicationSlug);

  if (!selectedPublication) {
    return {
      props: {
        themeMeta,
      },
    };
  }

  const latestRelease =
    await publicationService.getLatestPublicationReleaseSummary(
      publicationSlug,
    );

  const selectedRelease =
    !releaseSlug || latestRelease.slug === releaseSlug
      ? latestRelease
      : await publicationService.getPublicationReleaseSummary(
          publicationSlug,
          releaseSlug,
        );

  const [subjects, featuredTables, fullPublication] = await Promise.all([
    tableBuilderService.listReleaseSubjects(selectedRelease.id),
    tableBuilderService.listReleaseFeaturedTables(selectedRelease.id),
    publicationService.getLatestPublicationRelease(publicationSlug),
  ]);

  if (subjectId && subjects.some(subject => subject.id === subjectId)) {
    const subjectMeta = await tableBuilderService.getSubjectMeta(
      subjectId,
      selectedRelease.id,
    );

    return {
      props: {
        featuredTables,
        fullPublication,
        selectedPublication: {
          ...selectedPublication,
          selectedRelease: {
            id: selectedRelease.id,
            latestData: selectedRelease.latestRelease,
            slug: selectedRelease.slug,
            title: selectedRelease.title,
            type: selectedRelease.type,
          },
          latestRelease: {
            title: latestRelease.title,
            slug: latestRelease.slug,
          },
        },
        selectedSubjectId: subjectId,
        subjects,
        subjectMeta,
        themeMeta,
      },
    };
  }

  return {
    props: {
      themeMeta,
      fullPublication,
      selectedPublication: {
        ...selectedPublication,
        selectedRelease: {
          id: selectedRelease.id,
          latestData: selectedRelease.latestRelease,
          slug: selectedRelease.slug,
          title: selectedRelease.title,
          type: selectedRelease.type,
        },
        latestRelease: {
          title: latestRelease.title,
          slug: latestRelease.slug,
        },
      },
      subjects,
      featuredTables,
    },
  };
};

export default TableToolPage;
