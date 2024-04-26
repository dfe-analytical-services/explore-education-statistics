'use client';

import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import React, { ReactNode, useEffect } from 'react';
import { useRouter } from 'next/router';

export default function ApplicationInsightsContextProvider({
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

export const ApplicationInsightsTracking = () => {
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
