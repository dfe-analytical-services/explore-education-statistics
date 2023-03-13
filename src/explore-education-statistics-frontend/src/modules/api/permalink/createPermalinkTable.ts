import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import mapTableToJson, {
  TableJson,
} from '@common/modules/table-tool/utils/mapTableToJson';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import logger from '@common/services/logger';
import { ErrorBody } from '@frontend/modules/api/types/error';
import type { NextApiRequest, NextApiResponse } from 'next';

interface SuccessBody {
  table: TableJson;
  title: string;
}

/**
 * Endpoint to generate table json and title for permalinks.
 *
 * Expects req.body to be:
 * {
 *  fullTable: TableDataResponse,
 *  configuration: {
 *   tableHeaders: UnmappedTableHeadersConfig
 *  }
 * }
 */
export default async function createPermalinkTable(
  req: NextApiRequest,
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
    const fullTable = mapFullTable(unmappedFullTable);
    const tableHeadersConfig = mapTableHeadersConfig(
      configuration.tableHeaders,
      fullTable,
    );

    const { tableJson } = mapTableToJson({
      tableHeadersConfig,
      subjectMeta: fullTable.subjectMeta,
      results: fullTable.results,
    });

    const title = generateTableTitle(fullTable.subjectMeta);

    if (tableJson && title) {
      return res.status(200).send({ table: tableJson, title });
    }
    return res.status(500).send({ message: 'Cannot build table', status: 500 });
  } catch (error) {
    logger.error(error);
    return res
      .status(500)
      .send({ message: 'Something went wrong', status: 500 });
  }
}
