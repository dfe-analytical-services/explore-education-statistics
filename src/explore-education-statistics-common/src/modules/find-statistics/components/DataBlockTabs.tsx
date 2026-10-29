import ErrorBoundary from '@common/components/ErrorBoundary';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import withLazyLoad from '@common/hocs/withLazyLoad';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import useDataBlock from '@common/modules/find-statistics/hooks/useDataBlock';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { DataBlock } from '@common/services/types/blocks';
import logger from '@common/services/logger';
import { OmitStrict } from '@common/types';
import { useQueryClient } from '@tanstack/react-query';
import React, { ReactNode, useMemo, useRef, useState } from 'react';

const testId = (dataBlock: DataBlock) => `Data block - ${dataBlock.name}`;

export interface DataBlockTabsProps {
  additionalTabContent?:
    ((props: { dataBlock: DataBlock }) => ReactNode) | ReactNode;
  dataBlock: DataBlock;
  dataBlockStaleTime?: number;
  firstTabs?: ReactNode;
  lastTabs?: ReactNode;
  getInfographic?: GetInfographic;
  id?: string;
  /**
   * Wait until the data block is scrolled into view before rendering it.
   * Defaults to true, unless its table data is already in the query cache
   * (e.g. fetched server-side), so that it's server-rendered.
   */
  lazy?: boolean;
  releaseVersionId: string;
  onToggle?: (section: { id: string; title: string }) => void;
}

interface DataBlockTabsContentProps extends OmitStrict<
  DataBlockTabsProps,
  'lazy'
> {
  lazyChartTab: boolean;
}

const DataBlockTabsContent = ({
  additionalTabContent,
  dataBlock,
  dataBlockStaleTime,
  firstTabs,
  lastTabs,
  getInfographic,
  id = `dataBlock-${dataBlock.id}`,
  lazyChartTab,
  releaseVersionId,
  onToggle,
}: DataBlockTabsContentProps) => {
  const {
    chart,
    isTableDataLoading,
    isTableDataError,
    fullTable,
    isGeoJsonError,
    isGeoJsonInitialLoading,
  } = useDataBlock({
    dataBlock,
    dataBlockStaleTime,
    releaseVersionId,
    getInfographic,
  });

  const dataTableRef = useRef<HTMLElement>(null);

  const tableHeadersConfig = useMemo(() => {
    if (!fullTable || !dataBlock.table) return undefined;
    try {
      return dataBlock.table.tableHeaders
        ? mapTableHeadersConfig(dataBlock.table.tableHeaders, fullTable)
        : getDefaultTableHeaderConfig(fullTable);
    } catch (error) {
      logger.error(error);
      return undefined;
    }
  }, [dataBlock.table, fullTable]);

  const isTableHeadersError = !!fullTable && !tableHeadersConfig;
  const isTableError = isTableDataError || isTableHeadersError;
  const isChartError = isTableDataError || isGeoJsonError;

  const errorMessage = <WarningMessage>Could not load content</WarningMessage>;

  // Other tabs (e.g. the release headlines' summary) don't depend on the data
  // block's data, so keep showing them, with the error in the data's own tabs.
  if (isTableDataError && !firstTabs && !lastTabs) return errorMessage;

  const additionTabContentElement =
    typeof additionalTabContent === 'function'
      ? additionalTabContent({ dataBlock })
      : additionalTabContent;
  return (
    <Tabs id={id} testId={testId(dataBlock)} onToggle={onToggle}>
      {firstTabs}

      {!!dataBlock.charts.length && (
        <TabsSection
          id={`${id}-charts`}
          lazy={lazyChartTab}
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
          <LoadingSpinner
            loading={isTableDataLoading || isGeoJsonInitialLoading}
          >
            {isChartError && errorMessage}
            {chart && (
              <ErrorBoundary fallback={errorMessage}>
                <ChartRenderer
                  id="dataBlockTabs-chart"
                  source={dataBlock.source}
                  chart={chart}
                />
                {additionTabContentElement}
              </ErrorBoundary>
            )}
          </LoadingSpinner>
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
          <LoadingSpinner loading={isTableDataLoading}>
            {isTableError && errorMessage}
            {fullTable && tableHeadersConfig && (
              <ErrorBoundary fallback={errorMessage}>
                <TimePeriodDataTable
                  key={dataBlock.id}
                  captionTitle={dataBlock?.heading}
                  dataBlockVersionId={dataBlock.id}
                  ref={dataTableRef}
                  footnotesHeadingHiddenText={`for ${dataBlock?.heading}`}
                  fullTable={fullTable}
                  query={dataBlock.query}
                  releaseVersionId={releaseVersionId}
                  source={dataBlock?.source}
                  tableHeadersConfig={tableHeadersConfig}
                />

                {additionTabContentElement}
              </ErrorBoundary>
            )}
          </LoadingSpinner>
        </TabsSection>
      )}

      {lastTabs}
    </Tabs>
  );
};

const LazyDataBlockTabsContent = withLazyLoad(DataBlockTabsContent, {
  offset: 100,
  placeholder: ({ dataBlock, id }) => (
    <span id={id} data-testid={testId(dataBlock)} />
  ),
});

export default function DataBlockTabs({ lazy, ...props }: DataBlockTabsProps) {
  const queryClient = useQueryClient();

  // `useState` here to check whether the data has been prefetched during SSR on first render.
  // `<Hydrate>` adds server-fetched data to the client's cache before this first render.
  const [hasTableData] = useState(
    () =>
      queryClient.getQueryData(
        tableBuilderQueries.getDataBlockTable(
          props.releaseVersionId,
          props.dataBlock.dataBlockId,
        ).queryKey,
      ) !== undefined,
  );

  if (lazy ?? !hasTableData) {
    return <LazyDataBlockTabsContent {...props} lazyChartTab />;
  }

  // Only render the chart tab before it's selected if there's data to show.
  return <DataBlockTabsContent {...props} lazyChartTab={!hasTableData} />;
}
