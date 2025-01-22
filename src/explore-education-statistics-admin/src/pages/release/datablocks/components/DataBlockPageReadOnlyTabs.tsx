import TableTabSection from '@admin/pages/release/datablocks/components/TableTabSection';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import getMapInitialBoundaryLevel from '@common/modules/charts/components/utils/getMapInitialBoundaryLevel';
import useDataBlock from '@common/modules/find-statistics/hooks/useDataBlock';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import tableBuilderService from '@common/services/tableBuilderService';
import { MapChart } from '@common/services/types/blocks';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import React, { useMemo, useState } from 'react';

interface Props {
  releaseId: string;
  dataBlock: ReleaseDataBlock;
}

const testId = (dataBlock: ReleaseDataBlock) =>
  `Data block - ${dataBlock.name}`;

const DataBlockPageReadOnlyTabs = ({ releaseId, dataBlock }: Props) => {
  const { chart, fullTable, isTableDataLoading, onBoundaryLevelChange } =
    useDataBlock({
      dataBlock,
      releaseId,
    });

  const tableHeaders = useMemo(
    () =>
      fullTable
        ? mapTableHeadersConfig(dataBlock.table.tableHeaders, fullTable)
        : undefined,
    [dataBlock.table.tableHeaders, fullTable],
  );

  return (
    <LoadingSpinner text="Loading data block" loading={isTableDataLoading}>
      {fullTable ? (
        <Tabs id="dataBlockTabs">
          <TabsSection title="Table" key="table" id="dataBlockTabs-table">
            {tableHeaders && (
              <TableTabSection
                dataBlock={dataBlock}
                table={fullTable}
                tableHeaders={tableHeaders}
              />
            )}
          </TabsSection>
          {chart && (
            <TabsSection
              title="Chart"
              key="chart"
              id="dataBlockTabs-chart"
              testId={`${testId(dataBlock)}-chart-tab`}
            >
              <div className="govuk-width-container">
                <LoadingSpinner loading={fullTable || isInitialGeoJsonLoading}>
                  <ChartRenderer
                    id="dataBlockTabs-chart"
                    source={dataBlock.source}
                    chart={chart}
                  />
                </LoadingSpinner>
              </div>
            </TabsSection>
          )}
        </Tabs>
      ) : (
        <WarningMessage>
          There was a problem loading the data block
        </WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default DataBlockPageReadOnlyTabs;
