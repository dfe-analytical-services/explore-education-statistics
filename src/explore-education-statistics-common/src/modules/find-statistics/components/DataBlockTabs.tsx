import ErrorBoundary from '@common/components/ErrorBoundary';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import withLazyLoad from '@common/hocs/withLazyLoad';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import { AxesConfiguration } from '@common/modules/charts/types/chart';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { DataBlock } from '@common/services/types/blocks';
import { useQuery } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
import React, { ReactNode, useMemo, useState } from 'react';
import tableBuilderQueries from '../queries/tableBuilderQueries';

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
  releaseId: string;
  onToggle?: (section: { id: string; title: string }) => void;
}

const DataBlockTabs = ({
  additionalTabContent,
  dataBlock,
  firstTabs,
  lastTabs,
  getInfographic,
  id = `dataBlock-${dataBlock.id}`,
  releaseId,
  onToggle,
}: DataBlockTabsProps) => {
  const [selectedBoundaryLevel, setSelectedBoundaryLevel] = useState(
    dataBlock.charts[0]?.type === 'map'
      ? dataBlock.charts[0].boundaryLevel
      : undefined,
  );

  const {
    data: tableData,
    error: tableDataError,
    isLoading: tableDataIsLoading,
  } = useQuery(
    tableBuilderQueries.getDataBlockTable(
      releaseId,
      dataBlock.dataBlockParentId,
    ),
  );

  const { data: locationGeoJson, error: geoJsonError } = useQuery(
    tableBuilderQueries.getDataBlockGeoJson(
      releaseId,
      dataBlock.dataBlockParentId,
      selectedBoundaryLevel,
    ),
  );

  const fullTable = useMemo(() => {
    return !tableData
      ? undefined
      : mapFullTable({
          ...tableData,
          subjectMeta: {
            ...tableData.subjectMeta,
            locations: locationGeoJson ?? tableData.subjectMeta.locations,
          },
        });
  }, [tableData, locationGeoJson]);

  const errorMessage = <WarningMessage>Could not load content</WarningMessage>;

  if (
    tableDataError &&
    isAxiosError(tableDataError) &&
    tableDataError.response?.status === 403
  ) {
    return null;
  }

  const additionTabContentElement =
    typeof additionalTabContent === 'function'
      ? additionalTabContent({ dataBlock })
      : additionalTabContent;
  return (
    <LoadingSpinner loading={tableDataIsLoading}>
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
            {(!!tableDataError || !!geoJsonError) && errorMessage}

            {fullTable && (
              <ErrorBoundary fallback={errorMessage}>
                {dataBlock.charts.map((chart, index) => {
                  const key = index;

                  const commonChartProps = {
                    id: `${id}-chart`,
                    axes: { ...chart.axes } as Required<AxesConfiguration>,
                    data: fullTable?.results,
                    meta: fullTable?.subjectMeta,
                    source: dataBlock?.source,
                  };

                  if (chart.type === 'infographic') {
                    return (
                      <ChartRenderer
                        {...chart}
                        {...commonChartProps}
                        key={key}
                        getInfographic={getInfographic}
                      />
                    );
                  }
                  if (chart.type === 'map') {
                    return (
                      <ChartRenderer
                        {...chart}
                        {...commonChartProps}
                        key={key}
                        onBoundaryLevelChange={async boundaryLevel => {
                          setSelectedBoundaryLevel(boundaryLevel);
                        }}
                      />
                    );
                  }

                  return (
                    <ChartRenderer {...chart} {...commonChartProps} key={key} />
                  );
                })}

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
            {!!tableDataError && errorMessage}

            {fullTable && (
              <ErrorBoundary fallback={errorMessage}>
                <TimePeriodDataTable
                  key={dataBlock.id}
                  captionTitle={dataBlock?.heading}
                  dataBlockId={dataBlock.id}
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
