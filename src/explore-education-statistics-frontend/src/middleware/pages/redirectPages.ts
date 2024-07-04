import applyRedirectRules from '@common/utils/url/applyRedirectRules';
import redirectService from '@frontend/services/redirectsService';
import type { NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

export default async function redirectPages(request: NextRequest) {
  const { nextUrl } = request;
  const decodedPathname = decodeURIComponent(request.nextUrl.pathname);

  // Check for redirects for release and methodology pages
  const redirects = await redirectService.list();
  const redirectPath = applyRedirectRules(decodedPathname, redirects);

  if (redirectPath !== decodedPathname) {
    const redirectUrl = nextUrl.clone();
    redirectUrl.pathname = redirectPath;
    return NextResponse.redirect(redirectUrl, 301);
  }

  // Redirect any URLs with uppercase characters to lowercase.
  if (decodedPathname !== decodedPathname.toLowerCase()) {
    const url = nextUrl.clone();
    url.pathname = decodedPathname.toLowerCase();
    return NextResponse.redirect(url, 301);
  }

  return NextResponse.next();
}
