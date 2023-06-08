import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import dataBlockService from '@admin/services/dataBlockService';
import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService from '@common/services/tableBuilderService';
import ButtonText from '@common/components/ButtonText';
import ReleasePreviewTableToolFinalStep from '@admin/pages/release/content/components/ReleasePreviewTableToolFinalStep';
import { Publication } from '@admin/services/publicationService';
import { Publication as ContentPublication } from '@common/services/publicationService';
import React, { useState } from 'react';

interface Props {
  releaseId: string;
  publication: Publication | ContentPublication;
}
const ReleasePreviewTableTool = ({ releaseId, publication }: Props) => {
  const [dataBlockId, setDataBlockId] = useState<string>();

  const { value: initialState, isLoading } = useAsyncHandledRetry<
    InitialTableToolState | undefined
  >(async () => {
    const [featuredTables, subjects] = await Promise.all([
      tableBuilderService.listReleaseFeaturedTables(releaseId),
      tableBuilderService.listReleaseSubjects(releaseId),
    ]);

    if (dataBlockId) {
      const { table, query } = await dataBlockService.getDataBlock(dataBlockId);

      const [subjectMeta, tableData] = await Promise.all([
        tableBuilderService.getSubjectMeta(query.subjectId, releaseId),
        tableBuilderService.getTableData({
          releaseId,
          ...query,
        }),
      ]);

      const fullTable = mapFullTable(tableData);
      const tableHeaders = mapTableHeadersConfig(table.tableHeaders, fullTable);

      return {
        initialStep: 5,
        subjects,
        featuredTables,
        query: {
          ...query,
          publicationId: publication.id,
          releaseId,
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
        publicationId: publication.id,
        releaseId,
        subjectId: '',
        indicators: [],
        filters: [],
        locationIds: [],
      },
    };
  }, [releaseId, dataBlockId]);

  return (
    <LoadingSpinner loading={isLoading}>
      {initialState && (
        <>
          <h2>Table tool</h2>

          <TableToolWizard
            themeMeta={[]}
            hidePublicationStep
            initialState={initialState}
            onSubjectStepBack={() => setDataBlockId(undefined)}
            renderFeaturedTableLink={featuredTable => (
              <ButtonText
                onClick={() => {
                  setDataBlockId(featuredTable.dataBlockId);
                }}
              >
                {featuredTable.name}
              </ButtonText>
            )}
            finalStep={({ query, table, tableHeaders, onReorder }) => (
              <WizardStep>
                {wizardStepProps => (
                  <>
                    <WizardStepHeading {...wizardStepProps}>
                      Explore data
                    </WizardStepHeading>

                    {query && table && tableHeaders && (
                      <ReleasePreviewTableToolFinalStep
                        publication={publication as Publication}
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
        </>
      )}
    </LoadingSpinner>
  );
};

export default ReleasePreviewTableTool;
