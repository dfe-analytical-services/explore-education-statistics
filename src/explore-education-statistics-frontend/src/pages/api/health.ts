import { ErrorBody } from '@frontend/modules/api/types/error';
import { NextApiRequest, NextApiResponse } from 'next';
import withMethods from '@frontend/middleware/api/withMethods';

const health = async function (
  _req: NextApiRequest,
  res: NextApiResponse<string | ErrorBody>,
) {
  return res.status(200).send('Health check OK');
};

export default withMethods({
  head: health,
  get: health,
});
