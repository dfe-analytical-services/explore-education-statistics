import {
  methodologyContentRoute,
  methodologyNavRoutes,
  MethodologyRouteParams,
  methodologyStatusRoute,
  methodologySummaryEditRoute,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import useNavRoutes from '@admin/hooks/useNavRoutes';
import { renderHook } from '@testing-library/react';
import React, { ReactNode } from 'react';
import { generatePath, MemoryRouter } from 'react-router';

describe('useNavRoutes', () => {
  const methodologyId = 'methodology-1';

  const renderAtRoute = (path: string) => {
    const location = generatePath<MethodologyRouteParams>(path, {
      methodologyId,
    });

    return renderHook(
      () => useNavRoutes(methodologyNavRoutes, { methodologyId }),
      {
        wrapper: ({ children }: { children?: ReactNode }) => (
          <MemoryRouter initialEntries={[location]}>{children}</MemoryRouter>
        ),
      },
    );
  };

  test('resolves the nav route paths for the nav bar', () => {
    const { result } = renderAtRoute(methodologySummaryRoute.path);

    expect(result.current.navBarRoutes).toEqual([
      { title: 'Summary', to: '/methodology/methodology-1/summary' },
      { title: 'Manage content', to: '/methodology/methodology-1/content' },
      { title: 'Sign off', to: '/methodology/methodology-1/status' },
    ]);
  });

  test('returns no previous section for the first nav route', () => {
    const { result } = renderAtRoute(methodologySummaryRoute.path);

    expect(result.current.currentRouteIndex).toBe(0);
    expect(result.current.currentRouteTitle).toBe('Summary');
    expect(result.current.previousSection).toBeUndefined();
    expect(result.current.nextSection).toEqual({
      label: 'Manage content',
      linkTo: '/methodology/methodology-1/content',
    });
  });

  test('returns both sections for a middle nav route', () => {
    const { result } = renderAtRoute(methodologyContentRoute.path);

    expect(result.current.currentRouteIndex).toBe(1);
    expect(result.current.currentRouteTitle).toBe('Manage content');
    expect(result.current.previousSection).toEqual({
      label: 'Summary',
      linkTo: '/methodology/methodology-1/summary',
    });
    expect(result.current.nextSection).toEqual({
      label: 'Sign off',
      linkTo: '/methodology/methodology-1/status',
    });
  });

  test('returns no next section for the last nav route', () => {
    const { result } = renderAtRoute(methodologyStatusRoute.path);

    expect(result.current.currentRouteIndex).toBe(2);
    expect(result.current.currentRouteTitle).toBe('Sign off');
    expect(result.current.previousSection).toEqual({
      label: 'Manage content',
      linkTo: '/methodology/methodology-1/content',
    });
    expect(result.current.nextSection).toBeUndefined();
  });

  test('returns no sections when the location is not a nav route', () => {
    const { result } = renderAtRoute(methodologySummaryEditRoute.path);

    expect(result.current.currentRouteIndex).toBe(-1);
    expect(result.current.previousSection).toBeUndefined();
    expect(result.current.nextSection).toBeUndefined();
  });

  test('falls back to the closest matching nav route for the title', () => {
    // The edit page isn't a nav route, but `useCurrentRouteTitle` matches
    // paths as prefixes, so it still resolves to the summary route's title.
    const { result } = renderAtRoute(methodologySummaryEditRoute.path);

    expect(result.current.currentRouteTitle).toBe('Summary');
  });
});
