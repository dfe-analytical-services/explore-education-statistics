import { ApplicationInsights } from '@microsoft/applicationinsights-web';

// eslint-disable-next-line import/prefer-default-export
export const initApplicationInsights = () => {
  if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY) {
    const appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: process.env.APPINSIGHTS_INSTRUMENTATIONKEY,
        enableAutoRouteTracking: true,
        autoTrackPageVisitTime: true,
      },
    });
    appInsights.loadAppInsights();

    // eslint-disable-next-line no-console
    console.log('Application Insights initialised');
  }
};
