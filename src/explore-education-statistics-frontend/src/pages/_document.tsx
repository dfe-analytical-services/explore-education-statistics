import { default as BaseDocument, Head, Main, NextScript } from 'next/document';
import React from 'react';

class Document extends BaseDocument {
  public static async getInitialProps(ctx: any) {
    const initialProps: any = await BaseDocument.getInitialProps(ctx);
    return { ...initialProps };
  }

  public render() {
    return (
      <html lang="en" className="govuk-template app-html-class">
        <Head>
          <meta name="viewport" content="width=device-width, initial-scale=1" />

          <link
            rel="shortcut icon"
            sizes="16x16 32x32 48x48"
            href="/static/images/favicon.ico"
            type="image/x-icon"
          />
          <link
            rel="mask-icon"
            href="/static/images/govuk-mask-icon.svg"
            color="blue"
          />
          <link
            rel="apple-touch-icon"
            sizes="180x180"
            href="/static/images/govuk-apple-touch-icon-180x180.png"
          />
          <link
            rel="apple-touch-icon"
            sizes="167x167"
            href="/static/images/govuk-apple-touch-icon-167x167.png"
          />
          <link
            rel="apple-touch-icon"
            sizes="152x152"
            href="/static/images/govuk-apple-touch-icon-152x152.png"
          />
          <link
            rel="apple-touch-icon"
            href="/static/images/govuk-apple-touch-icon.png"
          />

          <meta
            property="og:image"
            content="/assets/images/govuk-opengraph-image.png"
          />
        </Head>

        <body className="govuk-template__body app-body-class">
          <Main />
          <NextScript />
        </body>
      </html>
    );
  }
}

export default Document;
