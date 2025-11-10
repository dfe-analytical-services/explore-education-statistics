import { ErrorBody } from '@frontend/modules/api/types/error';
import { NextApiRequest, NextApiResponse } from 'next';
import withMethods from '@frontend/middleware/api/withMethods';

export default withMethods({
  head: async function health(
    _req: NextApiRequest,
    res: NextApiResponse<string | ErrorBody>,
  ) {
    return res.status(200).send('Health check OK');
  },
});
