import BaseDocument, { Head, Html, Main, NextScript } from 'next/document';
import React from 'react';

// As part of making the incremental transition from the Pages Router to the App Router,
// the contents of this file are being temporary duplicated in ~src/app/layout.tsx
// https://nextjs.org/docs/app/building-your-application/upgrading/app-router-migration#migrating-_documentjs-and-_appjs

class Document extends BaseDocument {
  render() {
    return (
      <Html lang="en" className="govuk-template app-html-class">
        <Head>
          {process.env.NODE_ENV === 'development' && (
            <>
              {/* Force browser to not cache any assets */}
              <meta httpEquiv="pragma" content="no-cache" />
              <meta httpEquiv="cache-control" content="no-cache" />
            </>
          )}

          <link
            rel="icon"
            sizes="48x48"
            href="/assets/images/favicon.ico"
            type="image/x-icon"
          />
          <link
            rel="icon"
            sizes="any"
            href="/assets/images/favicon.svg"
            type="image/svg+xml"
          />
          <link
            rel="mask-icon"
            href="/assets/images/govuk-icon-mask.svg"
            color="#0b0c0c"
          />
          <link
            rel="apple-touch-icon"
            href="/assets/images/govuk-icon-180.png"
          />
          <link rel="manifest" href="/assets/manifest.json" />

          <meta
            property="og:image"
            content="/assets/images/govuk-opengraph-image.png"
          />
        </Head>

        <body className="govuk-template__body app-body-class">
          <Main />
          <NextScript />
        </body>
      </Html>
    );
  }
}

export default Document;
