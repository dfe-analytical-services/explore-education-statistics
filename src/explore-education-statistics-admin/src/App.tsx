// Load app styles first to ensure correct style ordering
import './styles/_all.scss';

import apiAuthorizationRouteList from '@admin/components/api-authorization/ApiAuthorizationRoutes';
import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import { ConfigContextProvider } from '@admin/contexts/ConfigContext';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import routes from '@admin/routes/routes';
import {
  ApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React, { lazy, Suspense, useEffect } from 'react';
import { Route, Switch, useHistory } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import { LastLocationContextProvider } from './contexts/LastLocationContext';

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

function App() {
  return (
    <ConfigContextProvider>
      {config => (
        <ApplicationInsightsContextProvider
          instrumentationKey={config.AppInsightsKey}
        >
          <BrowserRouter>
            <ApplicationInsightsTracking />

            <QueryClientProvider client={queryClient}>
              <AuthContextProvider>
                <LastLocationContextProvider>
                  <PageErrorBoundary>
                    <Switch>
                      {Object.entries(apiAuthorizationRouteList).map(
                        ([key, authRoute]) => (
                          <Route exact key={key} {...authRoute} />
                        ),
                      )}

                      {Object.entries(routes).map(([key, route]) => (
                        <ProtectedRoute key={key} {...route} />
                      ))}

                      {/* Prototype pages are protected by default. To open them up change the ProtectedRoute to: */}
                      {/* <Route path="/prototypes" component={PrototypesEntry} /> */}
                      <ProtectedRoute
                        path="/prototypes"
                        protectionAction={user => user.permissions.isBauUser}
                        component={PrototypesEntry}
                      />

                      <ProtectedRoute
                        path="*"
                        allowAnonymousUsers
                        component={PageNotFoundPage}
                      />
                    </Switch>
                  </PageErrorBoundary>
                </LastLocationContextProvider>
              </AuthContextProvider>
            </QueryClientProvider>
          </BrowserRouter>
        </ApplicationInsightsContextProvider>
      )}
    </ConfigContextProvider>
  );
}

export default App;
