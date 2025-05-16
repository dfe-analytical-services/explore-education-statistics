import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import {
  ConfigContextProvider,
  useConfig,
} from '@admin/contexts/ConfigContext';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import routes, { publicRoutes } from '@admin/routes/routes';
import { ConfiguredMsalProvider } from '@admin/contexts/ConfiguredMsalProvider';
import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import { NetworkActivityContextProvider } from '@common/contexts/NetworkActivityContext';
import composeProviders from '@common/hocs/composeProviders';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import {
  QueryClient,
  QueryClientProvider as BaseQueryClientProvider,
} from '@tanstack/react-query';
import React, { lazy, ReactNode, Suspense, useEffect } from 'react';
import { Route, Switch, useHistory } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import { LastLocationContextProvider } from './contexts/LastLocationContext';
import { ConfiguredFeatureFlagProvider } from './contexts/FeatureFlagContext';

const queryClient = new QueryClient();

const PrototypeIndexPage = lazy(
  () => import('@admin/prototypes/PrototypeIndexPage'),
);

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

function PrototypesEntry() {
  const { value: prototypeRoutes = [] } = useAsyncRetry(() =>
    import('./prototypes/prototypeRoutes').then(module => module.default),
  );

  return (
    <Suspense fallback={<ServiceProblemsPage />}>
      <Switch>
        <Route exact path="/prototypes" component={PrototypeIndexPage} />
        {prototypeRoutes?.map(route => (
          <Route key={route.path} exact={route.exact ?? true} {...route} />
        ))}
      </Switch>
    </Suspense>
  );
}

export default function App() {
  return (
    <Providers>
      <PageErrorBoundary>
        <ApplicationInsightsTracking />

        <Switch>
          {Object.entries(publicRoutes).map(([key, route]) => (
            <Route key={key} {...route} />
          ))}

          {Object.entries(routes).map(([key, route]) => (
            <ProtectedRoute key={key} {...route} />
          ))}

          {/* Prototype pages are protected by default. To open them up change the ProtectedRoute to: */}
          {/* <Route path="/prototypes" component={PrototypesEntry} /> */}
          <ProtectedRoute
            path="/prototypes"
            protectionAction={permissions => permissions.isBauUser}
            component={PrototypesEntry}
          />

          <ProtectedRoute path="*" component={PageNotFoundPage} />
        </Switch>
      </PageErrorBoundary>
    </Providers>
  );
}

const Providers = composeProviders(
  ConfigContextProvider,
  ApplicationInsightsContextProvider,
  BrowserRouter,
  NetworkActivityContextProvider,
  QueryClientProvider,
  ConfiguredMsalProvider,
  AuthContextProvider,
  LastLocationContextProvider,
  ConfiguredFeatureFlagProvider,
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
