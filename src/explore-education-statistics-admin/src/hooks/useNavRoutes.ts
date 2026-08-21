import { PreviousNextLink } from '@admin/components/PreviousNextLinks';
import { NavRouteProps } from '@admin/routes/types';
import useCurrentRouteTitle from '@admin/hooks/useCurrentRouteTitle';
import { generatePath, useLocation } from 'react-router';

interface NavRoute {
  title: string;
  to: string;
}

interface UseNavRoutes {
  /**
   * The nav routes with their paths resolved, ready to pass straight to
   * {@see NavBar}.
   */
  navBarRoutes: NavRoute[];
  /**
   * The index of the current location within `navRoutes`, or -1 if the
   * current location isn't one of them (e.g. an edit page reached from a
   * nav route).
   */
  currentRouteIndex: number;
  currentRouteTitle?: string;
  previousSection?: PreviousNextLink;
  nextSection?: PreviousNextLink;
}

/**
 * Resolves a feature area's nav routes against the current location,
 * providing everything its page container needs to render a {@see NavBar}
 * and {@see PreviousNextLinks} for stepping through them in order.
 *
 * Previous/next sections are only returned when the current location is
 * itself one of the nav routes.
 */
export default function useNavRoutes<Params extends Record<string, string>>(
  navRoutes: NavRouteProps[],
  params: Params,
): UseNavRoutes {
  const { pathname } = useLocation();
  const currentRouteTitle = useCurrentRouteTitle(navRoutes);

  const navBarRoutes = navRoutes.map(route => ({
    title: route.title,
    to: generatePath<Params>(route.path, params),
  }));

  const currentRouteIndex = navBarRoutes.findIndex(
    route => route.to === pathname,
  );

  const toSection = (route?: NavRoute): PreviousNextLink | undefined =>
    route ? { label: route.title, linkTo: route.to } : undefined;

  return {
    navBarRoutes,
    currentRouteIndex,
    currentRouteTitle,
    previousSection:
      currentRouteIndex > 0
        ? toSection(navBarRoutes[currentRouteIndex - 1])
        : undefined,
    nextSection:
      currentRouteIndex > -1 && currentRouteIndex < navBarRoutes.length - 1
        ? toSection(navBarRoutes[currentRouteIndex + 1])
        : undefined,
  };
}
