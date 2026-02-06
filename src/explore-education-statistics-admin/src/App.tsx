import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import {
  ConfigContextProvider,
  useConfig,
} from '@admin/contexts/ConfigContext';
import { ConfiguredMsalProvider } from '@admin/contexts/ConfiguredMsalProvider';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import routes, { publicRoutes } from '@admin/routes/routes';
import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import { NetworkActivityContextProvider } from '@common/contexts/NetworkActivityContext';
import composeProviders from '@common/hocs/composeProviders';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import {
  QueryClientProvider as BaseQueryClientProvider,
  QueryClient,
} from '@tanstack/react-query';
import {
  createHead,
  UnheadProvider as BaseUnheadProvider,
} from '@unhead/react/client';
import React, { lazy, ReactNode, Suspense, useEffect } from 'react';
import { Route, Switch, useHistory } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import { LastLocationContextProvider } from './contexts/LastLocationContext';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';

import 'ckeditor5/ckeditor5.css';

const queryClient = new QueryClient();

const head = createHead();

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
