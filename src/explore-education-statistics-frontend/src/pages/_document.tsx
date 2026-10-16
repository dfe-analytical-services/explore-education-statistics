import BaseDocument, { Head, Html, Main, NextScript } from 'next/document';
import React from 'react';

class Document extends BaseDocument {
  render() {
    return (
      <Html lang="en" className="govuk-template app-html-class">
        <Head>
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
          {/*
            Add `js-enabled` before first paint, as the GOV.UK Frontend page
            template does. Content using `dfe-js-hidden` is rendered on the
            server so it works without JS; adding the class here hides it
            immediately for JS users, instead of after hydration (which made
            the page visibly jump).
          */}
          <script
            // eslint-disable-next-line react/no-danger
            dangerouslySetInnerHTML={{
              __html: "document.body.classList.add('js-enabled');",
            }}
          />
          <Main />
          <NextScript />
        </body>
      </Html>
    );
  }
}

export default Document;
