import { ChainedMiddleware } from '@frontend/middleware/pages/chain';
import { NextFetchEvent } from 'next/dist/server/web/spec-extension/fetch-event';
import { NextRequest, NextResponse } from 'next/server';

export default function runMiddleware(
  middleware: ChainedMiddleware,
  page: string,
) {
  const request = new NextRequest(new Request(page));

  const fetchEvent = new NextFetchEvent({
    page,
    request,
  });

  return middleware(request, fetchEvent, () => NextResponse.next());
}
