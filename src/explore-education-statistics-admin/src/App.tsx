import apiAuthorizationRouteList from '@admin/components/api-authorization/ApiAuthorizationRoutes';
import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import ThemeAndTopic from '@admin/components/ThemeAndTopic';
import { getConfig } from '@admin/config';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import {
  ApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React, { lazy, Suspense, useEffect } from 'react';
import { Route, Switch, useHistory } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import './App.scss';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import appRouteList from './routes/dashboard/routes';

const PrototypeIndexPage = lazy(() =>
  import('@admin/prototypes/PrototypeIndexPage'),
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
  const { value: routes = [] } = useAsyncRetry(() =>
    import('./prototypes/prototypeRoutes').then(module => module.default),
  );

  return (
    <Suspense fallback={<ServiceProblemsPage />}>
      <Switch>
        <Route exact path="/prototypes" component={PrototypeIndexPage} />
        {routes?.map(route => (
          <Route key={route.path} exact={route.exact ?? true} {...route} />
        ))}
      </Switch>
    </Suspense>
  );
}

function App() {
  return (
    <ApplicationInsightsContextProvider
      instrumentationKey={getConfig().then(config => config.AppInsightsKey)}
    >
      <ThemeAndTopic>
        <BrowserRouter>
          <ApplicationInsightsTracking />

          <AuthContextProvider>
            <PageErrorBoundary>
              <Switch>
                {Object.entries(apiAuthorizationRouteList).map(
                  ([key, authRoute]) => (
                    <Route exact key={key} {...authRoute} />
                  ),
                )}

                {Object.entries(appRouteList).map(([key, appRoute]) => (
                  <ProtectedRoute
                    key={key}
                    protectionAction={appRoute.protectedAction}
                    {...appRoute}
                  />
                ))}

                <ProtectedRoute
                  path="/prototypes"
                  protectionAction={user =>
                    user.permissions.canAccessUserAdministrationPages
                  }
                  component={PrototypesEntry}
                />

                <ProtectedRoute
                  allowAnonymousUsers
                  component={PageNotFoundPage}
                />
              </Switch>
            </PageErrorBoundary>
          </AuthContextProvider>
        </BrowserRouter>
      </ThemeAndTopic>
    </ApplicationInsightsContextProvider>
  );
}

export default App;
