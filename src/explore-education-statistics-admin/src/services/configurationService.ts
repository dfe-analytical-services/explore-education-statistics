import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import client from '@admin/services/util/service';

export const configurationService = {
  getInsightsKey(): Promise<string> {
    return client.get<string>('/configuration/application-insights');
  },
  getPublicBaseUrl(): Promise<string> {
    return client.get<string>('/configuration/public-app-url');
  }
};

const initApplicationInsights = async () => {
  const key = await configurationService.getInsightsKey();
  if (key) {
    const appInsights = new ApplicationInsights({
      config: {
        instrumentationKey: key,
        enableAutoRouteTracking: true,
        autoTrackPageVisitTime: true,
      },
    });
    appInsights.loadAppInsights();

    // eslint-disable-next-line no-console
    console.log('Application Insights initialised');
  }
};

export { initApplicationInsights as default };
