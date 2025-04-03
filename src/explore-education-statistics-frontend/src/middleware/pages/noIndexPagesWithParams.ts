import { NextRequest, NextResponse } from 'next/server';

export default function noIndexPagesWithParams(request: NextRequest) {
  const response = NextResponse.next();

  const reqUrl = new URL(request.url);
  const params = new URLSearchParams(reqUrl.search);
  const numParams = params.size;

  // Only allow indexing of pages without query params, or if the only param is 'page'
  if (numParams > 1 || (numParams === 1 && !params.has('page'))) {
    response.headers.set('X-Robots-Tag', 'noindex, nofollow');
  }

  return response;
}
