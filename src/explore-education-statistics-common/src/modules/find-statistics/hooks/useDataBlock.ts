import getMapInitialBoundaryLevel from '@common/modules/charts/components/utils/getMapInitialBoundaryLevel';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { DataBlock } from '@common/services/types/blocks';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';

interface Options {
  dataBlock: DataBlock;
  releaseId: string;
}

export default function useDataBlock({ dataBlock, releaseId }: Options) {
  const queryClient = useQueryClient();

  const chart = dataBlock.charts[0];
  const isMapChart = chart?.type === 'map';

  const [selectedBoundaryLevel, setSelectedBoundaryLevel] = useState(
    isMapChart ? getMapInitialBoundaryLevel(chart) : undefined,
  );

  const {
    data: tableData,
    error: tableDataError,
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
    error: geoJsonError,
    isInitialLoading: isInitial,
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

  const onBoundaryLevelChange = (boundaryLevel: number) => {};

  const fullTable = useMemo(() => {
    if (!tableData) {
      return undefined;
    }

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
  }, [geoJson, selectedBoundaryLevel, tableData]);

  const chartProps = useMemo(() => {
    if (chart.type === 'map') {
      return {};
    }
  });

  return {
    chart: chartProps,
    tableData,
    isTableDataLoading,
    onBoundaryLevelChange,
  };
}
