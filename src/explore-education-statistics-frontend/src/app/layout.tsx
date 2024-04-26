// Import order is important - these should be at the top
import '@frontend/polyfill';
import '@frontend/loadEnv';
import '../styles/_all.scss';
import { NetworkActivityContextProvider } from '@common/contexts/NetworkActivityContext';
import composeProviders from '@common/hocs/composeProviders';
import { AppProps } from 'next/app';
import { cookies } from 'next/headers';
import Head from 'next/head';
import React, { Suspense } from 'react';
import { Hydrate } from '@tanstack/react-query';
import { Metadata } from 'next';
import { Dictionary } from '@common/types';
import ApplicationInsightsContextProvider, {
  ApplicationInsightsTracking,
} from '../components/application-insights-context-provider';
import QueryClientProvider from '../components/query-client-provider';
import NavigationEvents from '../components/navigation-events';

type Props = AppProps<{ dehydratedState: unknown }> & {
  children: React.ReactNode;
};

const Providers = composeProviders(
  ApplicationInsightsContextProvider,
  NetworkActivityContextProvider,
  QueryClientProvider,
);

export default function RootLayout({ Component, pageProps, children }: Props) {
  const allCookies: Dictionary<string> = Object.assign(
    {},
    ...cookies()
      .getAll()
      .map(cookie => ({ [cookie.name]: cookie.value })),
  );

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
        <meta name="viewport" content="width=device-width, initial-scale=1" />
      </Head>

      <body className="govuk-template__body app-body-class">
        <Providers>
          <ApplicationInsightsTracking />
          <Suspense fallback={null}>
            <NavigationEvents cookies={allCookies} />
          </Suspense>
          <Hydrate state={pageProps.dehydratedState}>
            <Component {...pageProps} />
            {children}
          </Hydrate>
        </Providers>
      </body>
    </html>
  );
}

export const metadata: Metadata = {
  title: 'A test title', // Explore our statistics and data
  description: 'A test description',
};
