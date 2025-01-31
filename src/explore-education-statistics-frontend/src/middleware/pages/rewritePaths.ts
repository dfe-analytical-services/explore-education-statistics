import {
  NextFetchEvent,
  NextMiddleware,
  NextRequest,
  NextResponse,
  URLPattern,
} from 'next/server';
import type { URLPatternInit, URLPatternResult } from 'next/server';

interface Rewrite {
  source: URLPattern;
  destination: (matched: URLPatternResult) => string;
}

interface RewriteWithResult extends Rewrite {
  result: URLPatternResult;
}

const rewrites: Rewrite[] = [
  {
    source: new URLPattern({
      pathname: '/data-catalogue/data-set/:dataSetFileId/csv',
    }),
    destination: ({ pathname }) =>
      `${process.env.CONTENT_API_BASE_URL}/data-set-files/${pathname.groups.dataSetFileId}/download`,
  },
];

export default function rewritePaths(
  request: NextRequest,
  event: NextFetchEvent,
  middleware: NextMiddleware,
) {
  const { nextUrl } = request;

  const decodedPathname = decodeURIComponent(nextUrl.pathname);

  const matchedRewrite = findMatchingRewrite({
    pathname: decodedPathname,
  });

  if (matchedRewrite) {
    return NextResponse.rewrite(
      matchedRewrite.destination(matchedRewrite.result),
    );
  }

  return middleware(request, event);
}

function findMatchingRewrite(input: URLPatternInit): RewriteWithResult | null {
  return rewrites.reduce<RewriteWithResult | null>((acc, rewrite) => {
    if (acc) {
      return acc;
    }

    const result = rewrite.source.exec(input);

    if (result) {
      return {
        ...rewrite,
        result,
      };
    }

    return acc;
  }, null);
}
