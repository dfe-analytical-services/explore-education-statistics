import { GlobalPermissions } from '@admin/services/authService';
import React from 'react';

/**
 * Removed the explicit React Router type and only defining the required properties
 */
export type PublicRouteProps = {
  path: string;
  fullPath: string;
  element?: React.ReactNode;
};

export type ProtectedRouteProps = PublicRouteProps & {
  protectionAction?: (permissions: GlobalPermissions) => boolean;
};

/**
 * A route within a feature area's page container.
 *
 * `title` is used for the nav bar link text, the page's meta title and the
 * previous/next step links. `protectionAction` is inherited from
 * {@see ProtectedRouteProps} and is only used by feature areas that render
 * their routes with `<RouteSwitch protect />`.
 */
export type NavRouteProps = ProtectedRouteProps & {
  title: string;
};
