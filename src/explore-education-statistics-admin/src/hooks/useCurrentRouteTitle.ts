import { matchPath, useLocation } from 'react-router';

export default function useCurrentRouteTitle(
  routes: { title: string; path: string; fullPath: string }[],
): string | undefined {
  const { pathname } = useLocation();

  const foundMatch = routes.find(
    route =>
      matchPath({ path: route.fullPath, end: false }, pathname)?.pattern
        .path === route.fullPath,
  );

  return foundMatch?.title;
}
