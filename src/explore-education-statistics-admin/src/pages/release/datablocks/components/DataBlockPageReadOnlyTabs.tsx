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
  const isMapChart = dataBlock.charts[0]?.type === 'map';
  const queryClient = useQueryClient();
  const [selectedBoundaryLevel, setSelectedBoundaryLevel] = useState(
    isMapChart
      ? getMapInitialBoundaryLevel(dataBlock.charts[0] as MapChart)
      : undefined,
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

  const { value: tableData, isLoading } = useAsyncRetry(
    async () =>
      tableBuilderService.getTableData(
        dataBlock.query,
        releaseId,
        isMapChart
          ? (dataBlock.charts[0] as MapChart).boundaryLevel
          : undefined,
      ),
    [releaseId, dataBlock],
  );

  const { fullTable, tableHeaders } = useMemo(() => {
    if (!tableData || isFetching) {
      return {};
    }

    const table = mapFullTable({
      ...tableData,
      subjectMeta: {
        ...tableData.subjectMeta,
        locations:
          selectedBoundaryLevel && locationGeoJson
            ? locationGeoJson
            : tableData.subjectMeta.locations,
      },
    });

    return {
      fullTable: table,
      tableHeaders: mapTableHeadersConfig(dataBlock.table.tableHeaders, table),
    };
  }, [
    tableData,
    locationGeoJson,
    isFetching,
    dataBlock.table.tableHeaders,
    selectedBoundaryLevel,
  ]);

  return (
    <LoadingSpinner text="Loading data block" loading={isLoading || isFetching}>
      {!!geoJsonError && (
        <WarningMessage>Could not load content</WarningMessage>
      )}

      {fullTable && !geoJsonError ? (
        <Tabs id="dataBlockTabs">
          <TabsSection title="Table" key="table" id="dataBlockTabs-table">
            <TableTabSection
              dataBlock={dataBlock}
              table={fullTable}
              tableHeaders={tableHeaders}
            />
          </TabsSection>
          {dataBlock.charts.length > 0 && [
            <TabsSection
              title="Chart"
              key="chart"
              id="dataBlockTabs-chart"
              testId={`${testId(dataBlock)}-chart-tab`}
            >
              <div className="govuk-width-container">
                {dataBlock.charts.map((chart, index) => {
                  const key = index;

                  const rendererProps: Omit<ChartRendererProps, 'chart'> = {
                    id: `dataBlockTabs-chart-${index}`,
                    source: dataBlock?.source,
                  };

                  if (chart.type === 'map') {
                    return (
                      <ChartRenderer
                        {...rendererProps}
                        key={key}
                        chart={{
                          ...chart,
                          data: fullTable.results,
                          meta: fullTable.subjectMeta,
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
                      chart={{
                        ...chart,
                        data: fullTable.results,
                        meta: fullTable.subjectMeta,
                      }}
                      key={key}
                    />
                  );
                })}
              </div>
            </TabsSection>,
          ]}
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
