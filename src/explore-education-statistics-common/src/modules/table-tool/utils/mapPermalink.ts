import { UnmappedPermalink } from '@common/services/permalinkService';
import { Permalink } from '@common/modules/table-tool/types/permalink';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';

export default function mapPermalink(
  unmappedPermalink: UnmappedPermalink,
): Permalink {
  const fullTable = mapFullTable(unmappedPermalink.fullTable);

  return {
    ...unmappedPermalink,
    fullTable,
    query: {
      ...unmappedPermalink.query,
      configuration: {
        tableHeaders: mapTableHeadersConfig(
          unmappedPermalink.query.configuration.tableHeaders,
          fullTable.subjectMeta,
        ),
      },
    },
  };
}
