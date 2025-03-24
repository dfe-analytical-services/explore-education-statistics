import ChartBuilder, {
  TableQueryUpdateHandler,
} from '@admin/pages/release/datablocks/components/chart/ChartBuilder';
import { SavedDataBlock } from '@admin/pages/release/datablocks/components/DataBlockPageTabs';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import releaseChartFileService from '@admin/services/releaseChartFileService';
import { RefContext } from '@common/contexts/RefContext';
import { Chart } from '@common/modules/charts/types/chart';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService, {
  ReleaseTableDataQuery,
  TableDataQuery,
} from '@common/services/tableBuilderService';
import isEqual from 'lodash/isEqual';
import React, { useCallback, useMemo, useRef } from 'react';

export type ChartBuilderTableUpdateHandler = (params: {
  table: FullTable;
  query: ReleaseTableDataQuery;
}) => void;

interface Props {
  dataBlock: ReleaseDataBlock;
  query: TableDataQuery;
  releaseVersionId: string;
  table: FullTable;
  onDataBlockSave: (dataBlock: SavedDataBlock) => void;
  onTableUpdate: ChartBuilderTableUpdateHandler;
}

const ChartBuilderTabSection = ({
  dataBlock,
  query,
  releaseVersionId,
  table,
  onDataBlockSave,
  onTableUpdate,
}: Props) => {
  const meta = useMemo(
    () => ({
      ...table.subjectMeta,
      // Don't render footnotes as they take
      // up too much screen space.
      footnotes: [],
    }),
    [table.subjectMeta],
  );

  const handleChartSave = useCallback(
    async (chart: Chart, file?: File) => {
      let chartToSave = chart;

      if (chart.type === 'infographic' && file) {
        const { id } = await releaseChartFileService.uploadChartFile(
          releaseVersionId,
          { file },
        );

        chartToSave = {
          ...chart,
          fileId: id,
        };
      }

      await onDataBlockSave({
        ...dataBlock,
        query,
        charts: [chartToSave],
      });
    },
    [dataBlock, onDataBlockSave, query, releaseVersionId],
  );

  const handleChartDelete = useCallback(
    async (chart: Chart) => {
      // Cleanup potential infographic chart file if required
      if (chart.type === 'infographic' && chart.fileId) {
        await releaseChartFileService.deleteChartFile(
          releaseVersionId,
          chart.fileId,
        );
      }

      await onDataBlockSave({
        ...dataBlock,
        charts: [],
      });
    },
    [dataBlock, onDataBlockSave, releaseVersionId],
  );

  const handleTableQueryUpdate: TableQueryUpdateHandler = useCallback(
    async (updatedQuery, updatedBoundaryLevel) => {
      const nextQuery: ReleaseTableDataQuery = {
        ...query,
        ...updatedQuery,
      };

      // Don't update if nothing has changed
      if (isEqual(query, nextQuery) && updatedBoundaryLevel === undefined) {
        return;
      }

      const [tableData, geoJson] = await Promise.all([
        tableBuilderService.getTableData(nextQuery, releaseVersionId),
        updatedBoundaryLevel
          ? tableBuilderService.getDataBlockGeoJson(
              releaseVersionId,
              dataBlock.dataBlockParentId,
              updatedBoundaryLevel,
            )
          : undefined,
      ]);

      onTableUpdate({
        table: mapFullTable({
          ...tableData,
          subjectMeta: {
            ...tableData.subjectMeta,
            locations: geoJson ?? tableData.subjectMeta.locations,
          },
        }),
        query: nextQuery,
      });
    },
    [onTableUpdate, query, releaseVersionId, dataBlock],
  );

  const chartExportRef = useRef(null);

  return (
    <RefContext.Provider value={chartExportRef}>
      <ChartBuilder
        releaseVersionId={releaseVersionId}
        data={table.results}
        meta={meta}
        initialChart={dataBlock.charts[0]}
        tableTitle={dataBlock.heading}
        onChartSave={handleChartSave}
        onChartDelete={handleChartDelete}
        onTableQueryUpdate={handleTableQueryUpdate}
      />
    </RefContext.Provider>
  );
};

export default ChartBuilderTabSection;
