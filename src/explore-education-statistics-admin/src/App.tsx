import apiAuthorizationRouteList from '@admin/components/api-authorization/ApiAuthorizationRoutes';
import ErrorBoundary from '@admin/components/ErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import ThemeAndTopic from '@admin/components/ThemeAndTopic';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import React from 'react';
import { Route, Switch } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import './App.scss';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import appRouteList from './routes/dashboard/routes';

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
    <ThemeAndTopic>
      <BrowserRouter>
        <AuthContextProvider>
          <ErrorBoundary>
            <Switch>
              {authRoutes}
              {appRoutes}
              <ProtectedRoute
                allowAnonymousUsers
                component={PageNotFoundPage}
              />
            </Switch>
          </ErrorBoundary>
        </AuthContextProvider>
      </BrowserRouter>
    </ThemeAndTopic>
  );
}

export default App;
