import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import {
  ConfigContextProvider,
  useConfig,
} from '@admin/contexts/ConfigContext';
import { ConfiguredMsalProvider } from '@admin/contexts/ConfiguredMsalProvider';
import routes, { publicRoutes } from '@admin/routes/routes';
import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import { NetworkActivityContextProvider } from '@common/contexts/NetworkActivityContext';
import composeProviders from '@common/hocs/composeProviders';
import {
  QueryClientProvider as BaseQueryClientProvider,
  QueryClient,
} from '@tanstack/react-query';
import {
  createHead,
  UnheadProvider as BaseUnheadProvider,
} from '@unhead/react/client';
import React, { ReactNode, useEffect } from 'react';
import { Route, Switch, useHistory } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import { LastLocationContextProvider } from './contexts/LastLocationContext';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';

import 'ckeditor5/ckeditor5.css';
import { NotificationHubContextProvider } from './contexts/NotificationHubContext';

const queryClient = new QueryClient();

const head = createHead();

function ApplicationInsightsTracking() {
  const appInsights = useApplicationInsights();
  const history = useHistory();

  useEffect(() => {
    if (appInsights) {
      appInsights.trackPageView({
        uri: history.location.pathname,
      });

      history.listen(location => {
        appInsights.trackPageView({
          uri: location.pathname,
        });
      });
    }

    document.body.classList.add('js-enabled', 'govuk-frontend-supported');
  }, [appInsights, history]);

  return null;
}

export default function App() {
  return (
    <Providers>
      <PageErrorBoundary>
        <NotificationHubContextProvider>
          <ApplicationInsightsTracking />

          <Switch>
            {Object.entries(publicRoutes).map(([key, route]) => (
              <Route key={key} {...route} />
            ))}

            {Object.entries(routes).map(([key, route]) => (
              <ProtectedRoute key={key} {...route} />
            ))}

            <ProtectedRoute path="*" component={PageNotFoundPage} />
          </Switch>
        </NotificationHubContextProvider>
      </PageErrorBoundary>
    </Providers>
  );
}

const Providers = composeProviders(
  ConfigContextProvider,
  ApplicationInsightsContextProvider,
  BrowserRouter,
  NetworkActivityContextProvider,
  UnheadProvider,
  QueryClientProvider,
  ConfiguredMsalProvider,
  AuthContextProvider,
  LastLocationContextProvider,
);

function ApplicationInsightsContextProvider({
  children,
}: {
  children?: ReactNode;
}) {
  const config = useConfig();

  return (
    <BaseApplicationInsightsContextProvider
      instrumentationKey={config.appInsightsKey}
    >
      {children}
    </BaseApplicationInsightsContextProvider>
  );
}

function QueryClientProvider({ children }: { children?: ReactNode }) {
  return (
    <BaseQueryClientProvider client={queryClient}>
      {children}
    </BaseQueryClientProvider>
  );
}

function UnheadProvider({ children }: { children?: ReactNode }) {
  return <BaseUnheadProvider head={head}>{children}</BaseUnheadProvider>;
}
