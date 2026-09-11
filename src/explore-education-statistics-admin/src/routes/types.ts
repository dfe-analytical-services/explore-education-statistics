import { GlobalPermissions } from '@admin/services/authService';
import { NonIndexRouteObject } from 'react-router/dist/lib/context';

export type PublicRouteProps = NonIndexRouteObject & {
  path: string;
  fullPath: string;
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
