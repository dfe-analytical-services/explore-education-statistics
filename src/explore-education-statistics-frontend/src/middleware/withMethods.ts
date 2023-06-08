import { NextApiHandler, NextApiRequest, NextApiResponse } from 'next';

export type Method =
  | 'get'
  | 'post'
  | 'put'
  | 'patch'
  | 'delete'
  | 'options'
  | 'head';

export type MethodsConfig = {
  [key in Method]?: NextApiHandler;
};

export default function withMethods(config: MethodsConfig) {
  return async (req: NextApiRequest, res: NextApiResponse) => {
    const method = req.method?.toLowerCase() as Method;
    const handler = config[method];

    if (!handler) {
      return res.status(405).end();
    }

    return handler(req, res);
  };
}
