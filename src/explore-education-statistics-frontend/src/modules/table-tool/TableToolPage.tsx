import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { FastTrackTable } from '@common/services/fastTrackService';
import tableBuilderService, {
  Publication,
  SubjectMeta,
  Theme,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import { GetServerSideProps, NextPage } from 'next';
import dynamic from 'next/dynamic';
import React, { useMemo } from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';

const TableToolFinalStep = dynamic(
  () => import('@frontend/modules/table-tool/components/TableToolFinalStep'),
);

export interface TableToolPageProps {
  publication?: Publication;
  fastTrack?: FastTrackTable;
  subjectMeta?: SubjectMeta;
  themeMeta: Theme[];
}

const TableToolPage: NextPage<TableToolPageProps> = ({
  publication: initialPublication,
  fastTrack,
  subjectMeta,
  themeMeta,
}) => {
  const initialState = useMemo<InitialTableToolState | undefined>(() => {
    if (!initialPublication) {
      return undefined;
    }

    const { id: publicationId, subjects } = initialPublication;

    const highlights = initialPublication.highlights.filter(
      highlight => highlight.id !== fastTrack?.id,
    );

    if (fastTrack && subjectMeta) {
      const fullTable = mapFullTable(fastTrack.fullTable);
      const tableHeaders = mapTableHeadersConfig(
        fastTrack.configuration.tableHeaders,
        fullTable.subjectMeta,
      );

      return {
        initialStep: 6,
        subjects,
        highlights,
        query: fastTrack.query,
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
        publicationId,
        subjectId: '',
        indicators: [],
        filters: [],
        locations: {},
      },
    };
  }, [fastTrack, initialPublication, subjectMeta]);

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
        renderHighlightLink={highlight => (
          <Link
            to="/data-tables/fast-track/[fastTrackId]"
            as={`/data-tables/fast-track/${highlight.id}`}
            onClick={() => {
              logEvent({
                category: 'Table tool',
                action: 'Clicked to view Table highlight',
                label: `Table highlight name: ${highlight.name}`,
              });
            }}
          >
            {highlight.name}
          </Link>
        )}
        finalStep={({ publication, query, response }) => (
          <WizardStep>
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  Explore data
                </WizardStepHeading>

                {response && query && (
                  <TableToolFinalStep
                    publication={publication}
                    query={query}
                    table={response.table}
                    tableHeaders={response.tableHeaders}
                  />
                )}
                <hr className="govuk-!-margin-top-9" />
                <h2
                  className="govuk-heading-m govuk-!-margin-top-9"
                  id="edit-table"
                >
                  Update your table by editing any of the steps below
                </h2>
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
  const { publicationSlug = '' } = query as Dictionary<string>;

  const themeMeta = await tableBuilderService.getThemes();

  const publicationId =
    themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.slug === publicationSlug)?.id ?? '';

  const publication = publicationId
    ? await tableBuilderService.getPublication(publicationId)
    : undefined;

  if (publication) {
    return {
      props: {
        themeMeta,
        publication,
      },
    };
  }

  return {
    props: {
      themeMeta,
    },
  };
};

export default TableToolPage;
