import redirectPages from '@frontend/middleware/pages/redirectPages';
import type { NextRequest } from 'next/server';

export default async function middleware(request: NextRequest) {
  return redirectPages(request);
}

// Restrict to release and methodology pages.
export const config = {
  matcher: ['/find-statistics/:path/:path*', '/methodology/:path*'],
};
