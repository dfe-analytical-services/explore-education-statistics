import redirectPages from '@frontend/middleware/pages/redirectPages';
import chain from './middleware/pages/chain';
import rewritePaths from './middleware/pages/rewritePaths';
import noIndexPagesWithParams from './middleware/pages/noIndexPagesWithParams';

export default chain([rewritePaths, redirectPages, noIndexPagesWithParams]);

// Only run the middleware on the specified paths below.
// Ideally we'd just exclude build files and run it on all routes,
// e.g. '/((?!api|_next/static|_next/image|favicon.ico|assets).*)'
// But a bug in NextJS v12 causes problems for the back button
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
};
