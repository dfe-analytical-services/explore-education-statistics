import { NextApiRequest, NextApiResponse } from 'next';

const basicAuthHandler = (_: NextApiRequest, res: NextApiResponse) => {
  res.setHeader('WWW-Authenticate', 'Basic realm="Restricted"');
  res.status(401).end('Access denied');
};

export default basicAuthHandler;
