// Import order is important - these should be at the top
import '@frontend/polyfill';
import { Dictionary } from '@common/types';
import { useCookies } from '@frontend/hooks/useCookies';
import loadEnv from '@frontend/loadEnv';

import useMounted from '@common/hooks/useMounted';
import { setApiBaseUrls } from '@common/services/api';
import { initApplicationInsights } from '@common/services/applicationInsightsService';
import {
  initGoogleAnalytics,
  logPageView,
} from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
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

  setApiBaseUrls({
    content: process.env.CONTENT_API_BASE_URL,
    data: process.env.DATA_API_BASE_URL,
    notification: process.env.NOTIFICATION_API_BASE_URL,
  });

  useMounted(() => {
    if (getCookie('disableGA') !== 'true') {
      initGoogleAnalytics(process.env.GA_TRACKING_ID);
    }

    initHotJar(process.env.HOTJAR_ID);
    initApplicationInsights(process.env.APPINSIGHTS_INSTRUMENTATIONKEY);

    logPageView();

    router.events.on('routeChangeComplete', logPageView);

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
