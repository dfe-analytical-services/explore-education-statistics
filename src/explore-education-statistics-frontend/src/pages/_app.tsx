import useMounted from '@common/hooks/useMounted';
import '@frontend/loadEnv';
import '@frontend/polyfill';

import { initApplicationInsights } from '@frontend/services/applicationInsightsService';
import { logPageView } from '@frontend/services/googleAnalyticsService';
import { initHotJar } from '@frontend/services/hotjarService';
import { AppProps } from 'next/app';
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

export default App;
