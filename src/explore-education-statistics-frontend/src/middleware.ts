// import redirectPages from '@frontend/middleware/pages/redirectPages';
import type { NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export default async function middleware(request: NextRequest) {
  // TO DO - EES-4482
  // Uncomment this and the import and remove `return NextResponse.next();` to enable redirects.
  // Also remove the line disabling linting no-unused-vars.
  // return redirectPages(request);

  return NextResponse.next();
}

// Restrict to release and methodology pages.
export const config = {
  matcher: ['/find-statistics/:path/:path*', '/methodology/:path*'],
};
