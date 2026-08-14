import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';

/**
 * A route within a feature area's page container.
 *
 * `title` is used for the nav bar link text, the page's meta title and the
 * previous/next step links. `protectionAction` is inherited from
 * {@see ProtectedRouteProps} and is only used by feature areas that render
 * their routes with `<RouteSwitch protect />`.
 */
export interface NavRouteProps extends ProtectedRouteProps {
  path: string;
  title: string;
}
