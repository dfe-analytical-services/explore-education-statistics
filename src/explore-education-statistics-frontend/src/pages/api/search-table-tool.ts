import logger from '@common/services/logger';
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

  const endpoint = process.env.AZURE_TABLE_TOOL_SEARCH_ENDPOINT;

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

    const response = await fetch(endpoint, {
      method: 'POST',
      body: JSON.stringify(body),
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
