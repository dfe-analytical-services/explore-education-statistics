import redirectPages from '@frontend/middleware/pages/redirectPages';
import chain from './middleware/chain';
import rewritePaths from './middleware/pages/rewritePaths';
import noIndexPagesWithParams from './middleware/pages/noIndexPagesWithParams';
import cacheControlHeaders from './middleware/headers/cacheControlHeaders';
import securityHeaders from './middleware/headers/securityHeaders';

export default chain([
  rewritePaths,
  redirectPages,
  cacheControlHeaders,
  securityHeaders,
  noIndexPagesWithParams,
]);

export const config = {
  matcher: ['/((?!_next/).*)'],
  runtime: 'nodejs', // Required while we are using react 18 (can remove if using react 19)
};
