// Import order is important - these should be at the top
import '@frontend/polyfill';
import '@frontend/loadEnv';
import '../styles/_all.scss';
import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import { NetworkActivityContextProvider } from '@common/contexts/NetworkActivityContext';
import composeProviders from '@common/hocs/composeProviders';
import useMounted from '@common/hooks/useMounted';
import { Dictionary } from '@common/types';
import { useCookies } from '@frontend/hooks/useCookies';
import NextApp, { AppContext, AppProps } from 'next/app';
import Head from 'next/head';
import { useRouter } from 'next/router';
import React, { ReactNode, useEffect, useState } from 'react';
import {
  Hydrate,
  QueryClient,
  QueryClientProvider as BaseQueryClientProvider,
} from '@tanstack/react-query';
import { parseCookies } from 'nookies';

const ApplicationInsightsTracking = () => {
  const appInsights = useApplicationInsights();
  const router = useRouter();

  useEffect(() => {
    if (!appInsights) {
      return;
    }

    appInsights.trackPageView({
      uri: router.pathname,
    });

    router.events.on('routeChangeComplete', uri => {
      appInsights.trackPageView({ uri });
    });
  }, [appInsights, router.events, router.pathname]);

  return null;
};

type Props = AppProps<{ dehydratedState: unknown }> & {
  cookies: Dictionary<string>;
};

export default function App({ Component, pageProps, cookies }: Props) {
  const router = useRouter();
  const { getCookie } = useCookies(cookies);

  useMounted(() => {
    if (process.env.GA_TRACKING_ID && getCookie('disableGA') !== 'true') {
      import('@frontend/services/googleAnalyticsService').then(
        ({ initGoogleAnalytics, logPageView }) => {
          initGoogleAnalytics(process.env.GA_TRACKING_ID);

          logPageView();

          router.events.on('routeChangeComplete', logPageView);
        },
      );
    }

    document.body.classList.add('js-enabled');
  });

  return (
    <Providers>
      <Head>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
      </Head>

      <ApplicationInsightsTracking />

      <Hydrate state={pageProps.dehydratedState}>
        <Component {...pageProps} />
      </Hydrate>
    </Providers>
  );
}

App.getInitialProps = async (appContext: AppContext) => {
  const appProps = await NextApp.getInitialProps(appContext);

  return {
    ...appProps,
    cookies: parseCookies(appContext.ctx),
  };
};

const Providers = composeProviders(
  ApplicationInsightsContextProvider,
  NetworkActivityContextProvider,
  QueryClientProvider,
);

function ApplicationInsightsContextProvider({
  children,
}: {
  children?: ReactNode;
}) {
  return (
    <BaseApplicationInsightsContextProvider
      instrumentationKey={process.env.APPINSIGHTS_INSTRUMENTATIONKEY}
    >
      {children}
    </BaseApplicationInsightsContextProvider>
  );
}

function QueryClientProvider({ children }: { children?: ReactNode }) {
  const [queryClient] = useState(() => new QueryClient());

  return (
    <BaseQueryClientProvider client={queryClient}>
      {children}
    </BaseQueryClientProvider>
  );
}
