import BaseDocument, { Head, Html, Main, NextScript } from 'next/document';
import React from 'react';

class Document extends BaseDocument {
  render() {
    return (
      <Html
        lang="en"
        className="govuk-template govuk-template--rebranded app-html-class"
      >
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
