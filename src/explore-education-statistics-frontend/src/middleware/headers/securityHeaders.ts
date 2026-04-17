import { NextRequest, NextResponse } from 'next/server';

export default function securityHeaders(req: NextRequest) {
  const { pathname } = req.nextUrl;
  const res = NextResponse.next();

  // Exclude altering any requests to /_next/* in any way. Next will handle these separately.
  if (pathname.startsWith('/_next/')) {
    return res;
  }

  res.headers.set('X-Content-Type-Options', 'nosniff');
  return res;
}
