import { matchPath, useLocation } from 'react-router';

export default function useCurrentRouteTitle(
  routes: { title: string; path: string }[],
): string | undefined {
  const { pathname } = useLocation();

  const pathPattern = matchPath(
    pathname,
    routes.map(route => route.path),
  )?.path;

  return routes.find(route => route.path === pathPattern)?.title;
}
