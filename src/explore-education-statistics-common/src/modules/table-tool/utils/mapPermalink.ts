import {
  Permalink,
  UnmappedPermalink,
} from '@common/modules/table-tool/services/permalinkService';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';

export default function mapPermalink(
  unmappedPermalink: UnmappedPermalink,
): Permalink {
  const mappedFullTable = mapFullTable(unmappedPermalink.fullTable);

  return {
    ...unmappedPermalink,
    fullTable: mappedFullTable,
    query: {
      ...unmappedPermalink.query,
      configuration: {
        tableHeadersConfig: mapTableHeadersConfig(
          unmappedPermalink.query.configuration.tableHeadersConfig,
          mappedFullTable.subjectMeta,
        ),
      },
    },
  };
}
