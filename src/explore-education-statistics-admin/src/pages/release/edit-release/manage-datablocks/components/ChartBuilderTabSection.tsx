import ChartBuilder, {
  TableQueryUpdateHandler,
} from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilder';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import { ReleaseDataBlock } from '@admin/services/release/edit-release/datablocks/service';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService, {
  TableDataQuery,
} from '@common/services/tableBuilderService';
import { Chart } from '@common/services/types/blocks';
import isEqual from 'lodash/isEqual';
import React, { useCallback, useMemo, useState } from 'react';
import { useParams } from 'react-router';

interface Props {
  dataBlock: ReleaseDataBlock;
  query: TableDataQuery;
  table: FullTable;
  onDataBlockSave: (dataBlock: ReleaseDataBlock) => void;
}

const ChartBuilderTabSection = ({
  dataBlock,
  query: initialQuery,
  table,
  onDataBlockSave,
}: Props) => {
  const { releaseId } = useParams<{ releaseId: string }>();

  const [tableQuery, setTableQuery] = useState<TableDataQuery>(initialQuery);
  const [fullTable, setFullTable] = useState<FullTable>(table);

  const meta = useMemo(
    () => ({
      ...fullTable.subjectMeta,
      // Don't render footnotes as they take
      // up too much screen space.
      footnotes: [],
    }),
    [fullTable.subjectMeta],
  );

  const handleChartSave = useCallback(
    async (chart: Chart) => {
      await onDataBlockSave({
        ...dataBlock,
        dataBlockRequest: tableQuery,
        charts: [chart],
      });
    },
    [dataBlock, onDataBlockSave, tableQuery],
  );

  const handleChartDelete = useCallback(
    async (chart: Chart) => {
      // Cleanup potential infographic chart file if required
      if (chart.type === 'infographic' && chart.fileId) {
        await editReleaseDataService.deleteChartFile(
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
    async query => {
      const nextTableQuery: TableDataQuery = {
        ...tableQuery,
        ...query,
      };

      // Don't fetch table data again if queries are the same
      if (isEqual(tableQuery, nextTableQuery)) {
        return;
      }

      const tableData = await tableBuilderService.getTableData(nextTableQuery);

      setTableQuery(nextTableQuery);
      setFullTable(mapFullTable(tableData));
    },
    [tableQuery],
  );

  return (
    <ChartBuilder
      releaseId={releaseId}
      data={fullTable.results}
      meta={meta}
      initialConfiguration={dataBlock.charts[0]}
      onChartSave={handleChartSave}
      onChartDelete={handleChartDelete}
      onTableQueryUpdate={handleTableQueryUpdate}
    />
  );
};

export default ChartBuilderTabSection;
