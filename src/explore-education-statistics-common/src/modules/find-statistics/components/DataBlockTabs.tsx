import ErrorBoundary from '@common/components/ErrorBoundary';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import withLazyLoad from '@common/hocs/withLazyLoad';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import getMapInitialBoundaryLevel from '@common/modules/charts/components/utils/getMapInitialBoundaryLevel';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { DataBlock } from '@common/services/types/blocks';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
import React, { ReactNode, useMemo, useState } from 'react';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';

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
  const queryClient = useQueryClient();
  const [selectedBoundaryLevel, setSelectedBoundaryLevel] = useState(
    dataBlock.charts[0]?.type === 'map'
      ? getMapInitialBoundaryLevel(dataBlock.charts[0])
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

  const getDataBlockGeoJsonQuery = tableBuilderQueries.getDataBlockGeoJson(
    releaseId,
    dataBlock.dataBlockParentId,
    selectedBoundaryLevel ?? -1,
  );

  const {
    data: locationGeoJson,
    error: geoJsonError,
    isFetching,
  } = useQuery({
    queryKey: getDataBlockGeoJsonQuery.queryKey,
    queryFn: selectedBoundaryLevel
      ? getDataBlockGeoJsonQuery.queryFn
      : () => Promise.resolve({}),
    staleTime: Infinity,
    cacheTime: Infinity,
    keepPreviousData: true,
    refetchOnWindowFocus: false,
    refetchOnMount: false,
  });

  const fullTable = useMemo(() => {
    return tableData && !isFetching
      ? mapFullTable({
          ...tableData,
          subjectMeta: {
            ...tableData.subjectMeta,
            locations:
              selectedBoundaryLevel && locationGeoJson
                ? locationGeoJson
                : tableData.subjectMeta.locations,
          },
        })
      : undefined;
  }, [tableData, locationGeoJson, isFetching, selectedBoundaryLevel]);

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

                  const rendererProps: Omit<ChartRendererProps, 'chart'> = {
                    id: `${id}-chart`,
                    source: dataBlock?.source,
                  };

                  if (chart.type === 'infographic') {
                    return (
                      <ChartRenderer
                        {...rendererProps}
                        key={key}
                        chart={{
                          ...chart,
                          data: fullTable?.results,
                          meta: fullTable?.subjectMeta,
                          getInfographic,
                        }}
                      />
                    );
                  }

                  if (chart.type === 'map') {
                    return (
                      <ChartRenderer
                        {...rendererProps}
                        key={key}
                        chart={{
                          ...chart,
                          data: fullTable?.results,
                          meta: fullTable?.subjectMeta,
                          onBoundaryLevelChange: async boundaryLevel => {
                            const { queryKey, queryFn } =
                              tableBuilderQueries.getDataBlockGeoJson(
                                releaseId,
                                dataBlock.dataBlockParentId,
                                boundaryLevel,
                              );
                            const existingGeoJson =
                              queryClient.getQueryData(queryKey);

                            if (!existingGeoJson) {
                              await queryClient.fetchQuery({
                                queryKey,
                                queryFn,
                                staleTime: Infinity,
                                cacheTime: Infinity,
                              });
                            }
                            setSelectedBoundaryLevel(boundaryLevel);
                          },
                        }}
                      />
                    );
                  }

                  return (
                    <ChartRenderer
                      {...rendererProps}
                      key={key}
                      chart={{
                        ...chart,
                        data: fullTable?.results,
                        meta: fullTable?.subjectMeta,
                      }}
                    />
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
