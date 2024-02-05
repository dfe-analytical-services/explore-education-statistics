import redirectPages from '@frontend/middleware/pages/redirectPages';
import type { NextRequest } from 'next/server';

export default async function middleware(request: NextRequest) {
  return redirectPages(request);
}

// Only run the middleware on the specified paths below.
// Ideally we'd just exclude build files and run it on all routes,
// e.g. '/((?!api|_next/static|_next/image|favicon.ico|assets).*)'
// But a bug in NextJS v12 causes problems for the back button
// in the table tool (see https://github.com/vercel/next.js/pull/43919).
export const config = {
  matcher: [
    '/cookies/:path*',
    '/find-statistics/:path*/:path*',
    '/data-tables',
    '/data-catalogue',
    '/methodology/:path*',
    '/subscriptions',
  ],
};
