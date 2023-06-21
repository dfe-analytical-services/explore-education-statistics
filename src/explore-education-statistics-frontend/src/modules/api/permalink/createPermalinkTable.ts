import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import mapTableToJson, {
  TableJson,
} from '@common/modules/table-tool/utils/mapTableToJson';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import logger from '@common/services/logger';
import { ErrorBody } from '@frontend/modules/api/types/error';
import type { NextApiRequest, NextApiResponse } from 'next';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import deduplicatePermalinkLocations from '@common/services/util/permalinkServiceUtils';
import { TableDataResponse } from '@common/services/tableBuilderService';

interface SuccessBody {
  json: TableJson;
  caption: string;
}

/**
 * Endpoint to generate table json and title for permalinks.
 */

interface Request extends NextApiRequest {
  body: {
    fullTable: TableDataResponse;
    configuration: {
      tableHeaders: UnmappedTableHeadersConfig;
    };
  };
}

export default async function createPermalinkTable(
  req: Request,
  res: NextApiResponse<SuccessBody | ErrorBody>,
) {
  const {
    body: { configuration, fullTable: unmappedFullTable },
  } = req;

  if (!unmappedFullTable || !configuration) {
    return res
      .status(400)
      .send({ message: 'fullTable and configuration required', status: 400 });
  }

  try {
    // TO DO - EES-4259
    // For old permalinks with duplicate locations.
    // Can be removed once the permalinks migration is done.
    const dedupedUnmappedFullTable = deduplicatePermalinkLocations(
      unmappedFullTable,
    );
    const fullTable = mapFullTable(dedupedUnmappedFullTable);
    const tableHeadersConfig = mapTableHeadersConfig(
      configuration.tableHeaders,
      fullTable,
    );

    const { tableJson } = mapTableToJson({
      tableHeadersConfig,
      subjectMeta: fullTable.subjectMeta,
      results: fullTable.results,
    });

    const caption = generateTableTitle(fullTable.subjectMeta);

    if (tableJson && caption) {
      return res.status(200).send({ json: tableJson, caption });
    }
    return res.status(500).send({ message: 'Cannot build table', status: 500 });
  } catch (error) {
    logger.error(error);
    return res
      .status(500)
      .send({ message: 'Something went wrong', status: 500 });
  }
}
