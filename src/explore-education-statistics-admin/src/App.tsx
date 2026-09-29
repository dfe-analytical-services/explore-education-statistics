import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { AuthContextProvider } from '@admin/contexts/AuthContext';
import {
  ConfigContextProvider,
  useConfig,
} from '@admin/contexts/ConfigContext';
import { ConfiguredMsalProvider } from '@admin/contexts/ConfiguredMsalProvider';
import routes, { publicRoutes } from '@admin/routes/appRoutes';
import {
  ApplicationInsightsContextProvider as BaseApplicationInsightsContextProvider,
  useApplicationInsights,
} from '@common/contexts/ApplicationInsightsContext';
import { NetworkActivityContextProvider } from '@common/contexts/NetworkActivityContext';
import composeProviders from '@common/hocs/composeProviders';
import {
  QueryClient,
  QueryClientProvider as BaseQueryClientProvider,
} from '@tanstack/react-query';
import {
  createHead,
  UnheadProvider as BaseUnheadProvider,
} from '@unhead/react/client';
import React, { ReactNode, useEffect } from 'react';
import {
  createBrowserRouter,
  Outlet,
  RouteObject,
  useLocation,
} from 'react-router';
import { RouterProvider } from 'react-router/dom';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import { ProtectedRouteProps, PublicRouteProps } from '@admin/routes/types';
import { LastLocationContextProvider } from './contexts/LastLocationContext';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';
import 'ckeditor5/ckeditor5.css';
import { NotificationHubContextProvider } from './contexts/NotificationHubContext';

const queryClient = new QueryClient();

const head = createHead();

function ApplicationInsightsTracking() {
  const appInsights = useApplicationInsights();

  const location = useLocation();

  useEffect(() => {
    document.body.classList.add('js-enabled', 'govuk-frontend-supported');
  }, []);

  useEffect(() => {
    if (appInsights) {
      appInsights.trackPageView({
        uri: location.pathname,
      });
    }
  }, [appInsights, location]);

  return null;
}

function AppLayout() {
  return (
    <Providers>
      <PageErrorBoundary>
        <NotificationHubContextProvider>
          <ApplicationInsightsTracking />

          <Outlet />
        </NotificationHubContextProvider>
      </PageErrorBoundary>
    </Providers>
  );
}

function generatePublicRoute(id: string, route: PublicRouteProps) {
  return {
    id,
    ...route,
  } as RouteObject;
}

function generateProtectedRoute(
  id: string,
  route: ProtectedRouteProps,
): RouteObject {
  const { element, protectionAction, path } = route;

  const protectedElement = (
    <ProtectedRoute protectionAction={protectionAction}>
      {element}
    </ProtectedRoute>
  );

  if (path.endsWith('/*')) {
    const parentPath = path.substring(0, path.length - 2);

    return {
      id,
      path: parentPath,
      children: [
        {
          id: `${id}_child`,
          element: protectedElement,
          path: '*',
        },
      ],
    } as RouteObject;
  }

  return {
    id,
    path,
    element: protectedElement,
  } as RouteObject;
}

const router = createBrowserRouter([
  {
    element: <AppLayout />,
    errorElement: <ServiceProblemsPage />,

    children: [
      ...Object.entries(publicRoutes).map(([key, route]) =>
        generatePublicRoute(key, route),
      ),

      ...Object.entries(routes).map(([key, route]) =>
        generateProtectedRoute(key, route),
      ),

      {
        path: '*',

        element: (
          <ProtectedRoute>
            <PageNotFoundPage />
          </ProtectedRoute>
        ),
      },
    ],
  },
]);

export default function App() {
  return <RouterProvider router={router} />;
}

const Providers = composeProviders(
  ConfigContextProvider,
  ApplicationInsightsContextProvider,
  NetworkActivityContextProvider,
  UnheadProvider,
  QueryClientProvider,
  ConfiguredMsalProvider,
  AuthContextProvider,
  LastLocationContextProvider,
);

function ApplicationInsightsContextProvider({
  children,
}: {
  children?: ReactNode;
}) {
  const config = useConfig();

  return (
    <BaseApplicationInsightsContextProvider
      instrumentationKey={config.appInsightsKey}
    >
      {children}
    </BaseApplicationInsightsContextProvider>
  );
}

function QueryClientProvider({ children }: { children?: ReactNode }) {
  return (
    <BaseQueryClientProvider client={queryClient}>
      {children}
    </BaseQueryClientProvider>
  );
}

function UnheadProvider({ children }: { children?: ReactNode }) {
  return <BaseUnheadProvider head={head}>{children}</BaseUnheadProvider>;
}
