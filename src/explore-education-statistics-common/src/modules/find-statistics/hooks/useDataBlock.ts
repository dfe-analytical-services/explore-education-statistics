import { HorizontalBarProps } from '@common/modules/charts/components/HorizontalBarBlock';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import { LineChartProps } from '@common/modules/charts/components/LineChartBlock';
import getMapInitialBoundaryLevel from '@common/modules/charts/components/utils/getMapInitialBoundaryLevel';
import { VerticalBarProps } from '@common/modules/charts/components/VerticalBarBlock';
import {
  ChartConfig,
  RenderableChart,
  WithChartConfigType,
} from '@common/modules/charts/types/chart';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { DataBlock } from '@common/services/types/blocks';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useState } from 'react';

interface Options {
  dataBlock: Pick<DataBlock, 'id' | 'charts' | 'query' | 'dataBlockParentId'>;
  releaseId: string;
  getInfographic?: GetInfographic;
}

export default function useDataBlock({
  dataBlock,
  releaseId,
  getInfographic,
}: Options) {
  const getChartFile = useGetReleaseFile(releaseId);
  const queryClient = useQueryClient();

  const chartConfig = dataBlock.charts[0] as ChartConfig | undefined;
  const isMapChart = chartConfig?.type === 'map';

  const [selectedBoundaryLevel, setSelectedBoundaryLevel] = useState(
    isMapChart ? getMapInitialBoundaryLevel(chartConfig) : undefined,
  );

  const {
    data: tableData,
    isError: isTableDataError,
    isLoading: isTableDataLoading,
  } = useQuery({
    ...tableBuilderQueries.getDataBlockTable(
      releaseId,
      dataBlock.dataBlockParentId,
    ),
    staleTime: Infinity,
  });

  const {
    data: geoJson,
    isError: isGeoJsonError,
    isInitialLoading: isGeoJsonInitialLoading,
  } = useQuery({
    ...tableBuilderQueries.getDataBlockGeoJson(
      releaseId,
      dataBlock.dataBlockParentId,
      selectedBoundaryLevel ?? -1,
    ),
    enabled: isMapChart,
    staleTime: Infinity,
    keepPreviousData: true,
  });

  const onBoundaryLevelChange = useCallback(
    async (boundaryLevel: number) => {
      await queryClient.prefetchQuery({
        ...tableBuilderQueries.getDataBlockGeoJson(
          releaseId,
          dataBlock.dataBlockParentId,
          boundaryLevel,
        ),
        staleTime: Infinity,
      });
      setSelectedBoundaryLevel(boundaryLevel);
    },
    [dataBlock, releaseId, queryClient],
  );

  const fullTable = useMemo(() => {
    if (!tableData || isGeoJsonInitialLoading) return undefined;
    return mapFullTable({
      ...tableData,
      subjectMeta: {
        ...tableData.subjectMeta,
        locations:
          selectedBoundaryLevel && geoJson
            ? geoJson
            : tableData.subjectMeta.locations,
      },
    });
  }, [tableData, isGeoJsonInitialLoading, selectedBoundaryLevel, geoJson]);

  const fullChart = useMemo<RenderableChart | undefined>(() => {
    if (!fullTable || !chartConfig) return undefined;

    switch (chartConfig.type) {
      case 'infographic':
        return {
          type: chartConfig.type,
          chartConfig,
          data: fullTable.results,
          meta: fullTable.subjectMeta,
          getInfographic: getInfographic ?? getChartFile,
        };
      case 'map':
        return {
          type: chartConfig.type,
          chartConfig,
          data: fullTable.results,
          meta: fullTable.subjectMeta,
          onBoundaryLevelChange,
        };
      default:
        return {
          type: chartConfig.type,
          chartConfig,
          data: fullTable.results,
          meta: fullTable.subjectMeta,
        } as
          | WithChartConfigType<HorizontalBarProps>
          | WithChartConfigType<LineChartProps>
          | WithChartConfigType<VerticalBarProps>;
    }
  }, [
    chartConfig,
    fullTable,
    getInfographic,
    onBoundaryLevelChange,
    getChartFile,
  ]);

  return {
    fullChart,
    isTableDataLoading,
    isTableDataError,
    fullTable,
    isGeoJsonError,
    isGeoJsonInitialLoading,
  };
}