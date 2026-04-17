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

  const { pathname } = req.nextUrl;

  // Exclude altering any requests to /_next/* in any way. Next will handle these separately.
  if (pathname.startsWith('/_next/')) {
    return res;
  }

  res.headers.set('X-Content-Type-Options', 'nosniff');
  return res;
}
