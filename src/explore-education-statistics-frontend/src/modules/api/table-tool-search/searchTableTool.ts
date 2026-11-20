import logger from '@common/services/logger';
import createMockSseStream from '@frontend/modules/table-tool/utils/createMockSseStream';
import { once } from 'events';
import type { NextApiRequest, NextApiResponse } from 'next';

/**
 * Proxies table tool search requests to the natural language
 * search function and streams its server-sent events back.
 */
export default async function searchTableTool(
  req: NextApiRequest,
  res: NextApiResponse,
) {
  // Stop streaming (and cancel the upstream request)
  // if the client disconnects before we've finished.
  const abortController = new AbortController();

  res.on('close', () => {
    if (!res.writableEnded) {
      abortController.abort();
    }
  });

  if (process.env.USE_MOCK_TABLE_TOOL_SEARCH_API === 'true') {
    await streamEvents(
      createMockSseStream({ returnResults: true }),
      res,
      abortController.signal,
    );
    return;
  }

  const endpoint = process.env.AZURE_TABLE_TOOL_SEARCH_ENDPOINT;

  if (!endpoint) {
    res
      .status(500)
      .json({ message: 'Server configuration error: Missing endpoint' });
    return;
  }

  try {
    const response = await fetch(endpoint, {
      method: 'POST',
      body: JSON.stringify(req.body),
      headers: {
        'Content-Type': 'application/json',
      },
      signal: abortController.signal,
    });

    if (!response.ok || !response.body) {
      logger.error(`Failed request: ${response.status} ${response.statusText}`);

      res.status(response.ok ? 502 : response.status).json({
        message: `Backend error: ${response.statusText}`,
        status: response.status,
      });
      return;
    }

    await streamEvents(response.body, res, abortController.signal);
  } catch (error) {
    if (abortController.signal.aborted) {
      return;
    }

    logger.error(error);

    if (res.headersSent) {
      // Already streaming, so we can't change the status.
      res.end();
      return;
    }

    res.status(500).json({ message: 'Internal server error', status: 500 });
  }
}

async function streamEvents(
  stream: ReadableStream<Uint8Array>,
  res: NextApiResponse,
  signal: AbortSignal,
) {
  res.writeHead(200, {
    'Content-Type': 'text/event-stream',
    'Cache-Control': 'no-cache, no-transform',
    Connection: 'keep-alive',
  });

  const reader = stream.getReader();

  // Cancelling the reader resolves any pending read, so
  // the loop below finishes when the client disconnects.
  const cancel = () => reader.cancel().catch(() => {});
  signal.addEventListener('abort', cancel, { once: true });

  try {
    while (!signal.aborted) {
      // eslint-disable-next-line no-await-in-loop
      const { done, value } = await reader.read();

      if (done) {
        break;
      }

      if (res.write(Buffer.from(value)) === false) {
        // Wait for a slow client to catch up before reading more, so the
        // rest of the response isn't buffered in memory (backpressure).
        // eslint-disable-next-line no-await-in-loop
        await once(res, 'drain', { signal }).catch(error => {
          // Rejects when the client disconnects, which ends the loop.
          if (!signal.aborted) {
            throw error;
          }
        });
      }
    }
  } finally {
    signal.removeEventListener('abort', cancel);
    reader.releaseLock();
  }

  if (!signal.aborted) {
    res.end();
  }
}
