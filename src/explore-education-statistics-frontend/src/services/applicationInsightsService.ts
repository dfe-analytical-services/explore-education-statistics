import { ApplicationInsights } from '@microsoft/applicationinsights-web';

// eslint-disable-next-line import/prefer-default-export
export const initApplicationInsights = () => {
  if (
    process.env.AI_INSTRUMENTATION !== undefined &&
    process.env.AI_TRACKING === 'true'
  ) {
    const appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: process.env.AI_INSTRUMENTATION,
        enableAutoRouteTracking: true,
        autoTrackPageVisitTime: true,
      },
    });
    appInsights.loadAppInsights();

    // eslint-disable-next-line no-console
    console.log('Application Insights initialised');
  }
};
