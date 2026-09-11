import { matchPath, useLocation } from 'react-router';

export default function useCurrentRouteTitle(
  routes: { title: string; path: string }[],
): string | undefined {
  const { pathname } = useLocation();

  return routes.find(
    route => matchPath(route.path, pathname)?.pattern.path === route.path,
  )?.title;

  /* this is a little confusing after upgrading from 5, they changed how it worked, so I'm leaving it here as reference
  const pathPattern = matchPath(
    pathname,
    routes.map(route => route.path),
  )?.path;

  return routes.find(route => route.path === pathPattern)?.title;  
   */
}
