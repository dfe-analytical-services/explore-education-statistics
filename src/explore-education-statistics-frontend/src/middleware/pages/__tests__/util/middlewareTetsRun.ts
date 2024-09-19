import { MiddlewareFactory } from '@frontend/middleware/chain';
import { NextFetchEvent } from 'next/dist/server/web/spec-extension/fetch-event';
import { NextRequest, NextResponse } from 'next/server';

export default function middlewareTestRun(
  middlewareFactory: MiddlewareFactory,
  page: string,
) {
  const request = new NextRequest(new Request(page));

  const fetchEvent = new NextFetchEvent({
    page,
    request,
  });

  return middlewareFactory(() => NextResponse.next())(request, fetchEvent);
}
