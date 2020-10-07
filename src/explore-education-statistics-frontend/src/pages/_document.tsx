import BaseDocument, {
  DocumentContext,
  Head,
  Html,
  Main,
  NextScript,
} from 'next/document';
import React from 'react';

class Document extends BaseDocument {
  public static async getInitialProps(ctx: DocumentContext) {
    const initialProps = await BaseDocument.getInitialProps(ctx);
    return { ...initialProps };
  }

  public render() {
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
            rel="shortcut icon"
            sizes="16x16 32x32 48x48"
            href="/assets/images/favicon.ico"
            type="image/x-icon"
          />
          <link
            rel="mask-icon"
            href="/assets/images/govuk-mask-icon.svg"
            color="blue"
          />
          <link
            rel="apple-touch-icon"
            sizes="180x180"
            href="/assets/images/govuk-apple-touch-icon-180x180.png"
          />
          <link
            rel="apple-touch-icon"
            sizes="167x167"
            href="/assets/images/govuk-apple-touch-icon-167x167.png"
          />
          <link
            rel="apple-touch-icon"
            sizes="152x152"
            href="/assets/images/govuk-apple-touch-icon-152x152.png"
          />
          <link
            rel="apple-touch-icon"
            href="/assets/images/govuk-apple-touch-icon.png"
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
      </Html>
    );
  }
}

export default Document;
