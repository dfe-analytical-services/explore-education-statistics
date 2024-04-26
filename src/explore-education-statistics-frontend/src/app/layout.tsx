import { Metadata } from 'next';
import Head from 'next/head';

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="govuk-template app-html-class">
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
        <link rel="apple-touch-icon" href="/assets/images/govuk-icon-180.png" />
        <link rel="manifest" href="/assets/manifest.json" />

        <meta
          property="og:image"
          content="/assets/images/govuk-opengraph-image.png"
        />
      </Head>

      <body className="govuk-template__body app-body-class">{children}</body>
    </html>
  );
}

export const metadata: Metadata = {
  title: 'A test title',
  description: 'A test description',
};
