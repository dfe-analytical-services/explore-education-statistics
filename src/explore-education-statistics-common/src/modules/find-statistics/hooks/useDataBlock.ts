import { RenderableChart } from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import getMapInitialBoundaryLevel from '@common/modules/charts/components/utils/getMapInitialBoundaryLevel';
import { Chart } from '@common/modules/charts/types/chart';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { DataBlock } from '@common/services/types/blocks';
import logger from '@common/services/logger';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useState } from 'react';

interface Options {
  dataBlock: Pick<DataBlock, 'id' | 'charts' | 'query' | 'dataBlockId'>;
  dataBlockStaleTime?: number;
  getInfographic?: GetInfographic;
  releaseVersionId: string;
}

export default function useDataBlock({
  dataBlock,
  dataBlockStaleTime,
  getInfographic,
  releaseVersionId,
}: Options) {
  const getChartFile = useGetReleaseFile(releaseVersionId);
  const queryClient = useQueryClient();

  const chart = dataBlock.charts[0] as Chart | undefined;
  const isMapChart = chart?.type === 'map';

  const [selectedBoundaryLevel, setSelectedBoundaryLevel] = useState(
    isMapChart ? getMapInitialBoundaryLevel(chart) : undefined,
  );

  const {
    data: tableData,
    isError: isTableDataError,
    isLoading: isTableDataLoading,
  } = useQuery({
    ...tableBuilderQueries.getDataBlockTable(
      releaseVersionId,
      dataBlock.dataBlockId,
    ),
    staleTime: dataBlockStaleTime,
  });

  const {
    data: geoJson,
    isError: isGeoJsonError,
    isInitialLoading: isGeoJsonInitialLoading,
  } = useQuery({
    ...tableBuilderQueries.getDataBlockGeoJson(
      releaseVersionId,
      dataBlock.dataBlockId,
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
          releaseVersionId,
          dataBlock.dataBlockId,
          boundaryLevel,
        ),
        staleTime: Infinity,
      });
      setSelectedBoundaryLevel(boundaryLevel);
    },
    [dataBlock, releaseVersionId, queryClient],
  );

  const fullTable = useMemo(() => {
    if (!tableData) return undefined;
    try {
      return mapFullTable(tableData);
    } catch (error) {
      logger.error(error);
      return undefined;
    }
  }, [tableData]);

  const chartFullTable = useMemo(() => {
    if (!tableData || !fullTable) return undefined;
    if (!selectedBoundaryLevel || !geoJson) return fullTable;
    try {
      return mapFullTable({
        ...tableData,
        subjectMeta: {
          ...tableData.subjectMeta,
          locations: geoJson,
        },
      });
    } catch (error) {
      logger.error(error);
      return undefined;
    }
  }, [tableData, selectedBoundaryLevel, geoJson, fullTable]);

  // Mapping errors are caught and logged above, so are reported as errors
  // here rather than leaving the table or chart to silently not render.
  const isTableMappingError = !!tableData && !fullTable;
  const isGeoJsonMappingError = !!fullTable && !chartFullTable;

  const chartProps = useMemo<RenderableChart | undefined>(() => {
    if (!chartFullTable || !chart) return undefined;
    if (chart.type === 'infographic') {
      return {
        ...chart,
        data: chartFullTable.results,
        meta: chartFullTable.subjectMeta,
        getInfographic: getInfographic ?? getChartFile,
      };
    }
    if (chart.type === 'map') {
      return {
        ...chart,
        data: chartFullTable.results,
        meta: chartFullTable.subjectMeta,
        onBoundaryLevelChange,
      };
    }
    return {
      ...chart,
      data: chartFullTable.results,
      meta: chartFullTable.subjectMeta,
    };
  }, [
    chart,
    chartFullTable,
    getInfographic,
    onBoundaryLevelChange,
    getChartFile,
  ]);

  return {
    chart: chartProps,
    isTableDataLoading,
    isTableDataError: isTableDataError || isTableMappingError,
    fullTable,
    isGeoJsonError: isGeoJsonError || isGeoJsonMappingError,
    isGeoJsonInitialLoading,
  };
}
