import { type NextFetchEvent, type NextProxy, NextRequest } from 'next/server';

export default async function securityHeaders(
  req: NextRequest,
  event: NextFetchEvent,
  middleware: NextProxy,
) {
  const res = await middleware(req, event);

  if (!res) {
    return null;
  }

  res.headers.set('X-Content-Type-Options', 'nosniff');
  return res;
}
