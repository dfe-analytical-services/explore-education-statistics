import React from 'react';
import {
  createMemoryRouter,
  MemoryRouter,
  RouterProvider,
  Routes,
} from 'react-router';
import { Route } from 'react-router-dom';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import TestLocationContext from '@admin/components/testing/TestLocationContext';

/**
 * This is to help with testing components that need to render during the upgrade to react router 6
 *
 * The router will be set up with the url set to `initialUrl`. There should be a route that matches it
 * to render the component.
 *
 * For simpler cases, if only a single component is needed, just use the route attribute and directly use the children.
 *
 * For more complex cases, use the routes array.
 */

type RouteAndChildren = {
  route: string;
  children: React.ReactNode;
  disableTestContext?: boolean;
};

type Props = {
  initialUrl: string;
  routes?: (string | RouteAndChildren)[];
} & Partial<RouteAndChildren>;

type RouteAndChildrenProps = {
  children?: React.ReactNode;
  disableTestContext?: boolean;
};
const RenderRouteWithChildren = ({
  children,
  disableTestContext,
}: RouteAndChildrenProps) => {
  if (disableTestContext) {
    return (
      <>
        {children}
        <TestLocationContext />
      </>
    );
  }

  return (
    <TestConfigContextProvider>
      {children}
      <TestLocationContext />
    </TestConfigContextProvider>
  );
};

export default function TestRouterRenderer({
  initialUrl,
  routes,
  route,
  disableTestContext,
  children,
}: Props) {
  return (
    <MemoryRouter initialEntries={[initialUrl]}>
      <Routes>
        {route && (
          <Route
            path={route}
            element={
              <RenderRouteWithChildren disableTestContext={disableTestContext}>
                {children}
              </RenderRouteWithChildren>
            }
          />
        )}

        {routes?.map(extraRoute =>
          typeof extraRoute === 'string' ? (
            <Route
              key={extraRoute}
              element={<RenderRouteWithChildren />}
              path={extraRoute}
            />
          ) : (
            <Route
              key={extraRoute.route}
              element={
                <RenderRouteWithChildren
                  disableTestContext={extraRoute.disableTestContext}
                >
                  {extraRoute.children}
                </RenderRouteWithChildren>
              }
              path={extraRoute.route}
            />
          ),
        )}
      </Routes>
    </MemoryRouter>
  );
}

/**
 * This is the same thing, but using createMemoryRouter instead, it might be better to use this
 * instead?
 *
 * @param param0
 * @param param0.initialUrl
 * @param param0.routes
 * @param param0.route
 * @param param0.disableTestContext
 * @param param0.children
 * @constructor
 */
export function TestRouterWithProvider({
  initialUrl,
  routes,
  route,
  disableTestContext,
  children,
}: Props) {
  const routeDefinitions = [
    route
      ? {
          path: route,
          element: (
            <RenderRouteWithChildren disableTestContext={disableTestContext}>
              {children}
            </RenderRouteWithChildren>
          ),
        }
      : undefined,
    ...(routes?.map(extraRoute =>
      typeof extraRoute === 'string'
        ? {
            path: extraRoute,
            element: <RenderRouteWithChildren />,
          }
        : {
            path: extraRoute.route,
            element: (
              <RenderRouteWithChildren
                disableTestContext={extraRoute.disableTestContext}
              >
                {extraRoute.children}
              </RenderRouteWithChildren>
            ),
          },
    ) || []),
  ].filter(r => r !== undefined);

  const router = createMemoryRouter(routeDefinitions, {
    initialEntries: [initialUrl],
  });

  return <RouterProvider router={router} />;
}
