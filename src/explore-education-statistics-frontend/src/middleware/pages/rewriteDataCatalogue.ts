import type { NextFetchEvent, NextMiddleware, NextRequest } from 'next/server';
import { NextResponse } from 'next/server';
import { match } from 'node-match-path';

export default function rewriteDataCatalogueDownload(
  middleware: NextMiddleware,
) {
  return async (request: NextRequest, event: NextFetchEvent) => {
    const { nextUrl } = request;
    const decodedPathname = decodeURIComponent(nextUrl.pathname);

    const source = '/data-catalogue/data-set/:dataSetFileId/csv';
    const matchedUrl = match(source, decodedPathname);

    if (matchedUrl.matches && matchedUrl.params?.dataSetFileId) {
      const destination = `${process.env.CONTENT_API_BASE_URL}/data-set-files/${matchedUrl.params.dataSetFileId}/download`;

      return NextResponse.redirect(new URL(destination), 301);
    }

    return middleware(request, event);
  };
}
