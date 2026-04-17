import redirectPages from '@frontend/middleware/pages/redirectPages';
import chain from './middleware/chain';
import rewritePaths from './middleware/pages/rewritePaths';
import noIndexPagesWithParams from './middleware/pages/noIndexPagesWithParams';
import cacheControlHeaders from './middleware/headers/cacheControlHeaders';
import securityHeaders from './middleware/headers/securityHeaders';

export default chain([
  cacheControlHeaders,
  securityHeaders,
  rewritePaths,
  redirectPages,
  noIndexPagesWithParams,
]);

// Only run the middleware on the specified paths below.
// Ideally we'd just exclude build files and run it on all routes,
// e.g. '/((?!api|_next/static|_next/image|favicon.ico|assets).*)'
// But a bug in Next.js v12 causes problems for the back button
// in the table tool (see https://github.com/vercel/next.js/pull/43919).
export const config = {
  matcher: [
    '/cookies/:path*',
    '/find-statistics/:path*/:path*',
    '/data-tables/:path*/:path*',
    '/data-catalogue',
    '/data-catalogue/data-set/:dataSetFileId/csv',
    '/methodology/:path*',
    '/subscriptions',
  ],
  runtime: 'nodejs', // Required while we are using react 18 (can remove if using react 19)
};
