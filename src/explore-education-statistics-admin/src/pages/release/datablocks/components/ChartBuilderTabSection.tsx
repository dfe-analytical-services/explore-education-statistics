import ChartBuilder, {
  TableQueryUpdateHandler,
} from '@admin/pages/release/datablocks/components/ChartBuilder';
import { SavedDataBlock } from '@admin/pages/release/datablocks/components/ReleaseDataBlocksPageTabs';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import releaseChartFileService from '@admin/services/releaseChartFileService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService, {
  TableDataQuery,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import isEqual from 'lodash/isEqual';
import React, { useCallback, useMemo } from 'react';
import { useParams } from 'react-router';

interface Props {
  dataBlock: ReleaseDataBlock;
  query: TableDataQuery;
  table: FullTable;
  onDataBlockSave: (dataBlock: SavedDataBlock) => void;
  onTableUpdate: (params: { table: FullTable; query: TableDataQuery }) => void;
}

const ChartBuilderTabSection = ({
  dataBlock,
  query,
  table,
  onDataBlockSave,
  onTableUpdate,
}: Props) => {
  const { releaseId } = useParams<{ releaseId: string }>();

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
    async (chart: Chart) => {
      await onDataBlockSave({
        ...dataBlock,
        query,
        charts: [chart],
      });
    },
    [dataBlock, onDataBlockSave, query],
  );

  const handleChartDelete = useCallback(
    async (chart: Chart) => {
      // Cleanup potential infographic chart file if required
      if (chart.type === 'infographic' && chart.fileId) {
        await releaseChartFileService.deleteChartFile(
          releaseId,
          meta.subjectName,
          chart.fileId,
        );
      }

      await onDataBlockSave({
        ...dataBlock,
        charts: [],
      });
    },
    [dataBlock, onDataBlockSave, releaseId, meta.subjectName],
  );

  const handleTableQueryUpdate: TableQueryUpdateHandler = useCallback(
    async updatedQuery => {
      const nextQuery: TableDataQuery = {
        ...query,
        ...updatedQuery,
      };

      // Don't fetch table data again if queries are the same
      if (isEqual(query, nextQuery)) {
        return;
      }

      const tableData = await tableBuilderService.getTableData(
        nextQuery,
        releaseId,
      );

      onTableUpdate({
        table: mapFullTable(tableData),
        query: nextQuery,
      });
    },
    [onTableUpdate, query, releaseId],
  );

  return (
    <ChartBuilder
      releaseId={releaseId}
      data={table.results}
      meta={meta}
      initialConfiguration={dataBlock.charts[0]}
      onChartSave={handleChartSave}
      onChartDelete={handleChartDelete}
      onTableQueryUpdate={handleTableQueryUpdate}
    />
  );
};

export default ChartBuilderTabSection;
