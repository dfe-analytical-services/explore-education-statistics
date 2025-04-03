import ErrorBoundary from '@common/components/ErrorBoundary';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import { RefContextProvider } from '@common/contexts/RefContext';
import withLazyLoad from '@common/hocs/withLazyLoad';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import useDataBlock from '@common/modules/find-statistics/hooks/useDataBlock';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { DataBlock } from '@common/services/types/blocks';
import React, { ReactNode, useRef } from 'react';

const testId = (dataBlock: DataBlock) => `Data block - ${dataBlock.name}`;

export interface DataBlockTabsProps {
  additionalTabContent?:
    | ((props: { dataBlock: DataBlock }) => ReactNode)
    | ReactNode;
  dataBlock: DataBlock;
  firstTabs?: ReactNode;
  lastTabs?: ReactNode;
  getInfographic?: GetInfographic;
  id?: string;
  releaseVersionId: string;
  onToggle?: (section: { id: string; title: string }) => void;
}

const DataBlockTabs = ({
  additionalTabContent,
  dataBlock,
  firstTabs,
  lastTabs,
  getInfographic,
  id = `dataBlock-${dataBlock.id}`,
  releaseVersionId,
  onToggle,
}: DataBlockTabsProps) => {
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
    getInfographic,
  });

  const dataTableRef = useRef<HTMLElement>(null);

  const errorMessage = <WarningMessage>Could not load content</WarningMessage>;
  if (isTableDataError) return errorMessage;

  const additionTabContentElement =
    typeof additionalTabContent === 'function'
      ? additionalTabContent({ dataBlock })
      : additionalTabContent;
  return (
    <LoadingSpinner loading={isTableDataLoading || isGeoJsonInitialLoading}>
      <Tabs id={id} testId={testId(dataBlock)} onToggle={onToggle}>
        {firstTabs}

        {!!dataBlock.charts.length && (
          <TabsSection
            id={`${id}-charts`}
            lazy
            tabLabel={
              <>
                Chart
                <span className="govuk-visually-hidden">
                  {` for ${dataBlock.charts[0].title}`}
                </span>
              </>
            }
            title="Chart"
          >
            {isGeoJsonError && errorMessage}
            {fullTable && (
              <ErrorBoundary fallback={errorMessage}>
                {chart && (
                  <RefContextProvider>
                    <ChartRenderer
                      id="dataBlockTabs-chart"
                      source={dataBlock.source}
                      chart={chart}
                    />
                  </RefContextProvider>
                )}
                {additionTabContentElement}
              </ErrorBoundary>
            )}
          </TabsSection>
        )}

        {dataBlock.table && (
          <TabsSection
            id={`${id}-tables`}
            testId={`${testId(dataBlock)}-table-tab`}
            tabLabel={
              dataBlock.charts?.length ? (
                <>
                  Table
                  <span className="govuk-visually-hidden">
                    {` for ${dataBlock.heading}`}
                  </span>
                </>
              ) : undefined
            }
            title="Table"
          >
            {fullTable && (
              <ErrorBoundary fallback={errorMessage}>
                <TimePeriodDataTable
                  key={dataBlock.id}
                  captionTitle={dataBlock?.heading}
                  dataBlockId={dataBlock.id}
                  ref={dataTableRef}
                  footnotesHeadingHiddenText={`for ${dataBlock?.heading}`}
                  fullTable={fullTable}
                  source={dataBlock?.source}
                  tableHeadersConfig={
                    dataBlock.table.tableHeaders
                      ? mapTableHeadersConfig(
                          dataBlock.table.tableHeaders,
                          fullTable,
                        )
                      : getDefaultTableHeaderConfig(fullTable)
                  }
                />

                {additionTabContentElement}
              </ErrorBoundary>
            )}
          </TabsSection>
        )}

        {lastTabs}
      </Tabs>
    </LoadingSpinner>
  );
};

export default withLazyLoad(DataBlockTabs, {
  offset: 100,
  placeholder: ({ dataBlock, id }) => (
    <span id={id} data-testid={testId(dataBlock)} />
  ),
});
