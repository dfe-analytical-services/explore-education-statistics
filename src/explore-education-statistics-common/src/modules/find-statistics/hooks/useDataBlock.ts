import { RenderableChart } from '@common/modules/charts/components/ChartRenderer';
import { GetInfographic } from '@common/modules/charts/components/InfographicBlock';
import getMapInitialBoundaryLevel from '@common/modules/charts/components/utils/getMapInitialBoundaryLevel';
import { Chart } from '@common/modules/charts/types/chart';
import useGetReleaseFile from '@common/modules/release/hooks/useGetReleaseFile';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { DataBlock } from '@common/services/types/blocks';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useState } from 'react';

interface Options {
  dataBlock: Pick<DataBlock, 'id' | 'charts' | 'query' | 'dataBlockParentId'>;
  releaseVersionId: string;
  getInfographic?: GetInfographic;
}

export default function useDataBlock({
  dataBlock,
  releaseVersionId,
  getInfographic,
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
      releaseVersionId,
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
          releaseVersionId,
          dataBlock.dataBlockParentId,
          boundaryLevel,
        ),
        staleTime: Infinity,
      });
      setSelectedBoundaryLevel(boundaryLevel);
    },
    [dataBlock, releaseVersionId, queryClient],
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

  const chartProps = useMemo<RenderableChart | undefined>(() => {
    if (!fullTable || !chart) return undefined;
    if (chart.type === 'infographic') {
      return {
        ...chart,
        data: fullTable.results,
        meta: fullTable.subjectMeta,
        getInfographic: getInfographic ?? getChartFile,
      };
    }
    if (chart.type === 'map') {
      return {
        ...chart,
        data: fullTable.results,
        meta: fullTable.subjectMeta,
        onBoundaryLevelChange,
      };
    }
    return {
      ...chart,
      data: fullTable.results,
      meta: fullTable.subjectMeta,
    };
  }, [chart, fullTable, getInfographic, onBoundaryLevelChange, getChartFile]);

  return {
    chart: chartProps,
    isTableDataLoading,
    isTableDataError,
    fullTable,
    isGeoJsonError,
    isGeoJsonInitialLoading,
  };
}
