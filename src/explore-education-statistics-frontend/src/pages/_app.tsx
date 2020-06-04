// Import order is important - these should be at the top
import '@frontend/polyfill';

import useMounted from '@common/hooks/useMounted';
import { contentApi, dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import { useCookies } from '@frontend/hooks/useCookies';
import loadEnv from '@frontend/loadEnv';
import notificationApi from '@frontend/services/clients/notificationApi';
import NextApp, { AppContext, AppProps } from 'next/app';
import { useRouter } from 'next/router';
import { parseCookies } from 'nookies';
import React from 'react';
import '../styles/_all.scss';

loadEnv();

interface Props extends AppProps {
  cookies: Dictionary<string>;
}

const App = ({ Component, pageProps, cookies }: Props) => {
  const router = useRouter();
  const { getCookie } = useCookies(cookies);

  loadEnv();

  contentApi.axios.defaults.baseURL = process.env.CONTENT_API_BASE_URL;
  dataApi.axios.defaults.baseURL = process.env.DATA_API_BASE_URL;
  notificationApi.axios.defaults.baseURL =
    process.env.NOTIFICATION_API_BASE_URL;

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

    if (process.env.HOTJAR_ID) {
      import('@frontend/services/hotjarService').then(({ initHotJar }) => {
        initHotJar(process.env.HOTJAR_ID);
      });
    }

    if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY) {
      import('@common/services/applicationInsightsService').then(
        ({ initApplicationInsights }) => {
          initApplicationInsights(process.env.APPINSIGHTS_INSTRUMENTATIONKEY);
        },
      );
    }

    document.body.classList.add('js-enabled');
  });

  return <Component {...pageProps} />;
};

App.getInitialProps = async (appContext: AppContext) => {
  const appProps = await NextApp.getInitialProps(appContext);

  loadEnv();

  return {
    ...appProps,
    cookies: parseCookies(appContext.ctx),
  };
};

export default App;
