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
    context: {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      waitUntil: (promise: Promise<any>) => {
        // In a real environment, this extends the request lifetime.
        // For manual execution/testing, strictly tracking this is usually not required
        // unless you are testing side effects.
        return promise;
      },
    },
  });

  return middleware(request, fetchEvent, () => NextResponse.next());
}
