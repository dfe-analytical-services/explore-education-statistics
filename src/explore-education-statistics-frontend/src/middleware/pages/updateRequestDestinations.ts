import {
  NextFetchEvent,
  NextMiddleware,
  NextRequest,
  NextResponse,
  URLPattern,
} from 'next/server';
import type { URLPatternInit, URLPatternResult } from 'next/server';

interface DestinationUpdate {
  source: URLPattern;
  destination: (matched: URLPatternResult) => string;
  type: 'rewrite' | 'permanent-redirect' | 'temporary-redirect';
}

interface DestinationUpdateWithResult extends DestinationUpdate {
  result: URLPatternResult;
}

const destinationUpdates: DestinationUpdate[] = [
  {
    source: new URLPattern({
      pathname: '/data-catalogue/data-set/:dataSetFileId/csv',
    }),
    destination: ({ pathname }) =>
      `${process.env.CONTENT_API_BASE_URL}/data-set-files/${pathname.groups.dataSetFileId}/download`,
    type: 'temporary-redirect',
  },
];

export default function updateRequestDestination(
  request: NextRequest,
  event: NextFetchEvent,
  middleware: NextMiddleware,
) {
  const { nextUrl } = request;

  const decodedPathname = decodeURIComponent(nextUrl.pathname);

  const matchedDestinationUpdate = findMatchingDestinationUpdate({
    pathname: decodedPathname,
  });

  if (!matchedDestinationUpdate) {
    return middleware(request, event);
  }

  if (matchedDestinationUpdate.type === 'rewrite') {
    return NextResponse.rewrite(
      matchedDestinationUpdate.destination(matchedDestinationUpdate.result),
    );
  }

  return NextResponse.redirect(
    matchedDestinationUpdate.destination(matchedDestinationUpdate.result),
    {
      status:
        matchedDestinationUpdate.type === 'permanent-redirect' ? 308 : 307,
    },
  );
}

function findMatchingDestinationUpdate(
  input: URLPatternInit,
): DestinationUpdateWithResult | null {
  return destinationUpdates.reduce<DestinationUpdateWithResult | null>(
    (acc, destinationUpdate) => {
      if (acc) {
        return acc;
      }

      const result = destinationUpdate.source.exec(input);

      if (result) {
        return {
          ...destinationUpdate,
          result,
        };
      }

      return acc;
    },
    null,
  );
}
