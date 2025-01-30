import useGetChartFile from '@admin/hooks/useGetChartFile';
import TableTabSection from '@admin/pages/release/datablocks/components/TableTabSection';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import ErrorBoundary from '@common/components/ErrorBoundary';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import useDataBlock from '@common/modules/find-statistics/hooks/useDataBlock';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import React, { useMemo } from 'react';

interface Props {
  releaseVersionId: string;
  dataBlock: ReleaseDataBlock;
}

const testId = (dataBlock: ReleaseDataBlock) =>
  `Data block - ${dataBlock.name}`;

const DataBlockPageReadOnlyTabs = ({ releaseVersionId, dataBlock }: Props) => {
  const getChartFile = useGetChartFile(releaseVersionId);
  const {
    chart,
    isTableDataLoading,
    isTableDataError,
    fullTable,
    isGeoJsonError,
    isGeoJsonInitialLoading,
  } = useDataBlock({
    dataBlock,
    releaseVersionId,
    getInfographic: getChartFile,
  });

  const tableHeaders = useMemo(() => {
    if (!fullTable) return undefined;
    return mapTableHeadersConfig(dataBlock.table.tableHeaders, fullTable);
  }, [dataBlock, fullTable]);

  if (isTableDataError)
    return (
      <WarningMessage>
        There was a problem loading the data block
      </WarningMessage>
    );

  return (
    <LoadingSpinner
      text="Loading data block"
      loading={isTableDataLoading || isGeoJsonInitialLoading}
    >
      <Tabs id="dataBlockTabs">
        {fullTable &&
          tableHeaders && [
            <TabsSection title="Table" key="table" id="dataBlockTabs-table">
              <TableTabSection
                dataBlock={dataBlock}
                table={fullTable}
                tableHeaders={tableHeaders}
              />
            </TabsSection>,
          ]}
        {!!dataBlock.charts.length && [
          <TabsSection
            title="Chart"
            key="chart"
            id="dataBlockTabs-chart"
            testId={`${testId(dataBlock)}-chart-tab`}
          >
            {fullTable && (
              <div className="govuk-width-container">
                {!!isGeoJsonError && (
                  <WarningMessage>Could not load chart</WarningMessage>
                )}
                <ErrorBoundary
                  fallback={
                    <WarningMessage>Could not load chart</WarningMessage>
                  }
                >
                  {chart && (
                    <ChartRenderer
                      id="dataBlockTabs-chart"
                      source={dataBlock.source}
                      chart={chart}
                    />
                  )}
                </ErrorBoundary>
              </div>
            )}
          </TabsSection>,
        ]}
      </Tabs>
    </LoadingSpinner>
  );
};

export default DataBlockPageReadOnlyTabs;
