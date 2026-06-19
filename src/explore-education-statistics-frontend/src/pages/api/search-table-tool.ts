import logger from '@common/services/logger';
import createMockSseStream from '@frontend/modules/table-tool/utils/createMockSseStream';
import { NextRequest } from 'next/server';

export const config = {
  runtime: 'edge',
};

export default async function handler(req: NextRequest) {
  if (req.method !== 'POST') {
    return new Response(JSON.stringify({ message: 'Method Not Allowed' }), {
      status: 405,
      headers: { 'Content-Type': 'application/json' },
    });
  }

  if (process.env.USE_MOCK_TABLE_TOOL_SEARCH_API === 'true') {
    return new Response(createMockSseStream({ returnResults: true }), {
      headers: {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache, no-transform',
        Connection: 'keep-alive',
      },
    });
  }

  const endpoint = process.env.AZURE_TABLE_TOOL_SEARCH_ENDPOINT;
  const functionKey = process.env.AZURE_TABLE_TOOL_SEARCH_FUNCTIONS_KEY;

  if (!endpoint) {
    return new Response(
      JSON.stringify({
        message: 'Server configuration error: Missing endpoint',
      }),
      { status: 500, headers: { 'Content-Type': 'application/json' } },
    );
  }

  try {
    const body = await req.json();
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };
    if (functionKey) {
      headers['X-Functions-Key'] = functionKey;
    }

    const response = await fetch(endpoint, {
      method: 'POST',
      body: JSON.stringify(body),
      headers,
    });

    if (!response.ok) {
      logger.error(`Failed request: ${response.status} ${response.statusText}`);

      return new Response(
        JSON.stringify({
          message: `Backend error: ${response.statusText}`,
          status: response.status,
        }),
        {
          status: response.status,
          statusText: response.statusText,
          headers: { 'Content-Type': 'application/json' },
        },
      );
    }

    return new Response(response.body, {
      headers: {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache, no-transform',
        Connection: 'keep-alive',
      },
    });
  } catch (error) {
    logger.error(error);
    return new Response(
      JSON.stringify({ message: 'Internal server error', status: 500 }),
      { status: 500, headers: { 'Content-Type': 'application/json' } },
    );
  }
}
