import apiAuthorizationRouteList from '@admin/components/api-authorization/ApiAuthorizationRoutes';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import ThemeAndTopic from '@admin/components/ThemeAndTopic';
import AriaLiveAnnouncer from '@common/components/AriaLiveAnnouncer';
import React from 'react';
import { Route, Switch } from 'react-router';
import { BrowserRouter } from 'react-router-dom';
import './App.scss';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import appRouteList from './routes/dashboard/routes';
import prototypeRouteList from './routes/prototypeRoutes';

function App() {
  const authRoutes = Object.entries(apiAuthorizationRouteList).map(
    ([key, authRoute]) => {
      return <Route exact key={`authRoute-${key}`} {...authRoute} />;
    },
  );

  const appRoutes = Object.entries(appRouteList).map(([key, appRoute]) => {
    return <ProtectedRoute key={`appRoute-${key}`} {...appRoute} />;
  });

  const prototypeRoutes = Object.entries(prototypeRouteList).map(
    ([key, prototypeRoute]) => {
      return <Route key={`authRoute-${key}`} {...prototypeRoute} />;
    },
  );

  return (
    <AriaLiveAnnouncer>
      <ThemeAndTopic>
        <BrowserRouter>
          <Switch>
            {authRoutes}
            {appRoutes}
            {prototypeRoutes}
            <ProtectedRoute
              redirectIfNotLoggedIn={false}
              component={PageNotFoundPage}
            />
          </Switch>
        </BrowserRouter>
      </ThemeAndTopic>
    </AriaLiveAnnouncer>
  );
}

export default App;
