import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTableMeta from '@common/modules/table-tool/utils/mapFullTableMeta';
import { TableDataResponse } from '@common/services/tableBuilderService';

export default function mapFullTable(
  unmappedFullTable: TableDataResponse,
): FullTable {
  const subjectMeta = unmappedFullTable.subjectMeta || {
    indicators: [],
    locations: [],
    timePeriodRange: [],
  };

  return {
    ...unmappedFullTable,
    subjectMeta: mapFullTableMeta(subjectMeta),
  };
}
