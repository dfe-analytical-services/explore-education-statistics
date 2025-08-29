import Link from '@admin/components/Link';
import PageTitle from '@admin/components/PageTitle';
import PreReleaseTableToolFinalStep from '@admin/pages/release/pre-release/components/PreReleaseTableToolFinalStep';
import ReleaseTableToolInfoWrapper from '@admin/pages/release/content/components/ReleaseTableToolInfoWrapper';
import {
  preReleaseTableToolRoute,
  PreReleaseTableToolRouteParams,
} from '@admin/routes/preReleaseRoutes';
import dataBlockService from '@admin/services/dataBlockService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService from '@common/services/tableBuilderService';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import { generatePath } from 'react-router-dom';
import releaseQueries from '@admin/queries/releaseQueries';
import publicationQueries from '@admin/queries/publicationQueries';
import { useQuery } from '@tanstack/react-query';

const PreReleaseTableToolPage = ({
  match,
}: RouteComponentProps<PreReleaseTableToolRouteParams>) => {
  const { publicationId, releaseVersionId, dataBlockId } = match.params;

  const { data: publication, isLoading: isPublicationLoading } = useQuery(
    publicationQueries.get(publicationId),
  );

  const { data: release, isLoading: isReleaseLoading } = useQuery(
    releaseQueries.get(releaseVersionId),
  );

  const { value: tableToolState, isLoading: isTableToolStateLoading } =
    useAsyncHandledRetry<InitialTableToolState | undefined>(async () => {
      const [featuredTables, subjects] = await Promise.all([
        tableBuilderService.listReleaseFeaturedTables(releaseVersionId),
        tableBuilderService.listReleaseSubjects(releaseVersionId),
      ]);

      if (dataBlockId) {
        const { table, query } = await dataBlockService.getDataBlock(
          dataBlockId,
        );

        const [subjectMeta, tableData] = await Promise.all([
          tableBuilderService.getSubjectMeta(query.subjectId, releaseVersionId),
          tableBuilderService.getTableData(
            {
              ...query,
            },
            releaseVersionId,
          ),
        ]);

        const fullTable = mapFullTable(tableData);
        const tableHeaders = mapTableHeadersConfig(
          table.tableHeaders,
          fullTable,
        );

        return {
          initialStep: 5,
          subjects,
          featuredTables,
          query: {
            ...query,
            publicationId,
            releaseVersionId,
          },
          subjectMeta,
          response: {
            table: fullTable,
            tableHeaders,
          },
        };
      }

      return {
        initialStep: 1,
        subjects,
        featuredTables,
        query: {
          publicationId,
          releaseVersionId,
          subjectId: '',
          indicators: [],
          filters: [],
          locationIds: [],
        },
      };
    }, [publicationId, releaseVersionId, dataBlockId]);

  const isLoading =
    isReleaseLoading || isTableToolStateLoading || isPublicationLoading;

  return (
    <>
      <PageTitle title="Create your own tables" caption="Table Tool" />

      <p>
        Choose the data and area of interest you want to explore and then use
        filters to create your table.
      </p>

      <p>
        Once you've created your table, you can download the data it contains
        for your own offline analysis.
      </p>

      <LoadingSpinner loading={isLoading}>
        {tableToolState && (
          <TableToolWizard
            themeMeta={[]}
            hidePublicationStep
            scrollOnMount
            initialState={tableToolState}
            renderFeaturedTableLink={featuredTable => (
              <Link
                to={generatePath<PreReleaseTableToolRouteParams>(
                  preReleaseTableToolRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
                    dataBlockId: featuredTable.dataBlockId,
                  },
                )}
              >
                {featuredTable.name}
              </Link>
            )}
            renderRelatedInfo={
              publication &&
              release && (
                <ReleaseTableToolInfoWrapper
                  publication={publication}
                  releaseType={release?.type}
                />
              )
            }
            finalStep={({ query, table, tableHeaders, onReorder }) => (
              <WizardStep>
                {wizardStepProps => (
                  <>
                    <WizardStepHeading {...wizardStepProps}>
                      Explore data
                    </WizardStepHeading>

                    {table && tableHeaders && query && release && (
                      <PreReleaseTableToolFinalStep
                        publication={publication}
                        query={query}
                        table={table}
                        tableHeaders={tableHeaders}
                        onReorderTableHeaders={onReorder}
                      />
                    )}
                  </>
                )}
              </WizardStep>
            )}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default PreReleaseTableToolPage;
