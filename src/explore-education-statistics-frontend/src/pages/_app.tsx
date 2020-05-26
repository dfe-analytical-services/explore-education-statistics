import '@frontend/polyfill';
import '@frontend/loadEnv';

import useMounted from '@common/hooks/useMounted';
import { initApplicationInsights } from '@frontend/services/applicationInsightsService';
import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import NextApp, { AppContext, AppProps } from 'next/app';
import { useRouter } from 'next/router';
import React from 'react';
import '../styles/_all.scss';

const App = ({ Component, pageProps }: AppProps) => {
  const router = useRouter();

  useMounted(() => {
    logPageView();
    initHotJar();
    initApplicationInsights();

    router.events.on('routeChangeComplete', logPageView);

    document.body.classList.add('js-enabled');
  });

  return <Component {...pageProps} />;
};

App.getInitialProps = async (appContext: AppContext) => {
  const appProps = await NextApp.getInitialProps(appContext);

  return { ...appProps };
};

export default App;
