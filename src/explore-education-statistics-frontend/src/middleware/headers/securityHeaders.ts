import {
  type NextFetchEvent,
  type NextMiddleware,
  NextRequest,
} from 'next/server';

export default async function securityHeaders(
  req: NextRequest,
  event: NextFetchEvent,
  middleware: NextMiddleware,
) {
  const res = await middleware(req, event);

  if (!res) {
    return null;
  }

  res.headers.set('X-Content-Type-Options', 'nosniff');
  return res;
}
