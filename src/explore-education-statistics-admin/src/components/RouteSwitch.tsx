import ProtectedRoute from '@admin/components/ProtectedRoute';
import { NavRouteProps } from '@admin/routes/types';
import React from 'react';
import { Route, Switch } from 'react-router';

interface Props {
  /**
   * Render each route as a {@see ProtectedRoute}, so that it's checked
   * against the user's global permissions before rendering.
   */
  protect?: boolean;
  routes: NavRouteProps[];
}

/**
 * Renders a feature area's routes within a `Switch`. Routes match exactly
 * unless they opt out with `exact: false`.
 */
const RouteSwitch = ({ protect = false, routes }: Props) => {
  return (
    <Switch>
      {routes.map(route =>
        protect ? (
          <ProtectedRoute exact key={route.path} {...route} />
        ) : (
          <Route exact key={route.path} {...route} />
        ),
      )}
    </Switch>
  );
};

export default RouteSwitch;
