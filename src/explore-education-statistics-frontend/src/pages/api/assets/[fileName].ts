import { ErrorBody } from '@frontend/modules/api/types/error';
import { NextApiRequest, NextApiResponse } from 'next';
import withMethods from '@frontend/middleware/api/withMethods';
import Stream, { Readable } from 'stream';

export enum AssetType {
  Image,
}

export default withMethods({
  get: async function getAsset(
    req: NextApiRequest,
    res: NextApiResponse<Stream | ErrorBody>,
  ) {
    const { fileName } = req.query;

    const response = await fetch(
      `${process.env.CONTENT_API_BASE_URL}/asset/${AssetType.Image}/${fileName}`,
    );

    if (!response.ok || !response.body) {
      return res.status(response.status).end();
    }

    const contentType = response.headers.get('content-type') || 'image/png';
    res.setHeader('Content-Type', contentType);

    // @ts-expect-error: Argument of type 'ReadableStream<Uint8Array<ArrayBuffer>>' is not assignable to parameter of type 'ReadableStream<any>'.
    const stream = Readable.fromWeb(response.body);
    return stream.pipe(res);
  },
});
