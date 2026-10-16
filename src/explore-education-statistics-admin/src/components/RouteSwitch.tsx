import ProtectedRoute from '@admin/components/ProtectedRoute';
import { NavRouteProps } from '@admin/routes/types';
import React from 'react';
import { Route, Routes } from 'react-router';

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
    <Routes>
      {routes.map(({ protectionAction, element, ...route }) =>
        protect ? (
          <Route
            key={route.path}
            {...route}

            element={
              <ProtectedRoute protectionAction={protectionAction}>
                {element}
              </ProtectedRoute>
            }
          />
        ) : (
          <Route key={route.path} {...route} element={element} />
        ),
      )}
    </Routes>
  );
};

export default RouteSwitch;
