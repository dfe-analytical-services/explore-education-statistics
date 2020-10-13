import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { FastTrackTable } from '@common/services/fastTrackService';
import tableBuilderService, {
  SubjectMeta,
  ThemeMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import orderBy from 'lodash/orderBy';
import { GetServerSideProps, NextPage } from 'next';
import dynamic from 'next/dynamic';
import React, { useMemo } from 'react';

const TableToolFinalStep = dynamic(
  () => import('@frontend/modules/table-tool/components/TableToolFinalStep'),
);

export interface TableToolPageProps {
  publicationSlug?: string;
  fastTrack?: FastTrackTable;
  subjectMeta?: SubjectMeta;
  themeMeta: ThemeMeta[];
}

const TableToolPage: NextPage<TableToolPageProps> = ({
  publicationSlug,
  fastTrack,
  subjectMeta,
  themeMeta,
}) => {
  const initialTableToolState = useMemo<
    Partial<InitialTableToolState> | undefined
  >(() => {
    if (publicationSlug) {
      const publicationId =
        themeMeta
          .flatMap(option => option.topics)
          .flatMap(option => option.publications)
          .find(option => option.slug === publicationSlug)?.id ?? '';

      return {
        initialStep: 1,
        query: {
          publicationId,
          subjectId: '',
          indicators: [],
          filters: [],
          locations: {},
        },
      };
    }

    if (!fastTrack || !subjectMeta) {
      return undefined;
    }

    const fullTable = mapFullTable(fastTrack.fullTable);
    const tableHeaders = mapTableHeadersConfig(
      fastTrack.configuration.tableHeaders,
      fullTable.subjectMeta,
    );

    return {
      initialStep: 6,
      query: fastTrack.query,
      subjectMeta,
      response: {
        table: fullTable,
        tableHeaders,
      },
    };
  }, [fastTrack, publicationSlug, subjectMeta, themeMeta]);

  return (
    <Page title="Create your own tables online" caption="Table Tool" wide>
      <p>
        Choose the data and area of interest you want to explore and then use
        filters to create your table.
      </p>

      <p>
        Once you've created your table, you can download the data it contains
        for your own offline analysis.
      </p>

      <TableToolWizard
        key={fastTrack?.id}
        scrollOnMount
        themeMeta={themeMeta}
        initialState={initialTableToolState}
        renderHighlights={highlights => (
          <aside>
            <h3>Table highlights</h3>

            <p>View popular tables related to this publication:</p>

            <ul>
              {orderBy(highlights, ['label'], ['asc'])
                .filter(highlight => highlight.id !== fastTrack?.id)
                .map(highlight => (
                  <li key={highlight.id}>
                    <Link
                      to="/data-tables/fast-track/[fastTrackId]"
                      as={`/data-tables/fast-track/${highlight.id}`}
                    >
                      {highlight.label}
                    </Link>
                  </li>
                ))}
            </ul>
          </aside>
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
              </>
            )}
          </WizardStep>
        )}
      />
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<TableToolPageProps> = async ({
  query,
}) => {
  const { publicationSlug = '' } = query as Dictionary<string>;

  const themeMeta = await tableBuilderService.getThemes();

  return {
    props: {
      themeMeta,
      publicationSlug,
    },
  };
};

export default TableToolPage;
