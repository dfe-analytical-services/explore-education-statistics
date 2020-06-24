import apiAuthorizationRouteList from '@admin/components/api-authorization/ApiAuthorizationRoutes';
import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import ThemeAndTopic from '@admin/components/ThemeAndTopic';
import { getConfig } from '@admin/config';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import {
  ApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import React, { useEffect } from 'react';
import { Route, Switch, useHistory } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import './App.scss';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import appRouteList from './routes/dashboard/routes';

function ApplicationInsightsTracking() {
  const appInsights = useApplicationInsights();
  const history = useHistory();

  useEffect(() => {
    appInsights.trackPageView({
      uri: history.location.pathname,
    });

    history.listen(location => {
      appInsights.trackPageView({
        uri: location.pathname,
      });
    });
  }, [appInsights, history]);

  return null;
}

function App() {
  const authRoutes = Object.entries(apiAuthorizationRouteList).map(
    ([key, authRoute]) => {
      return <Route exact key={`authRoute-${key}`} {...authRoute} />;
    },
  );

  const appRoutes = Object.entries(appRouteList).map(([key, appRoute]) => {
    return (
      <ProtectedRoute
        key={`appRoute-${key}`}
        protectionAction={appRoute.protectedAction}
        {...appRoute}
      />
    );
  });

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
                {authRoutes}
                {appRoutes}
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
