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
import { createBrowserRouter, Outlet, useLocation } from 'react-router';
import { RouterProvider } from 'react-router/dom';
import ServiceProblemsPage from '@admin/pages/errors/ServiceProblemsPage';
import RedirectPage from '@admin/auth/RedirectPage';
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

const BaseProviders = composeProviders(
  ConfigContextProvider,
  ApplicationInsightsContextProvider,
  NetworkActivityContextProvider,
  UnheadProvider,
  QueryClientProvider,
);

const Providers = composeProviders(
  ConfiguredMsalProvider,
  AuthContextProvider,
  LastLocationContextProvider,
);

function AppLayout() {
  return (
    <BaseProviders>
      <Providers>
        <PageErrorBoundary>
          <NotificationHubContextProvider>
            <ApplicationInsightsTracking />

            <Outlet />
          </NotificationHubContextProvider>
        </PageErrorBoundary>
      </Providers>
    </BaseProviders>
  );
}

function generatePublicRoute(id: string, route: PublicRouteProps) {
  return {
    id,
    ...route,
  };
}

function generateProtectedRoute(id: string, route: ProtectedRouteProps) {
  const { element, protectionAction, path } = route;

  return {
    id,
    path,
    element: (
      <ProtectedRoute protectionAction={protectionAction}>
        {element}
      </ProtectedRoute>
    ),
  };
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
  {
    element: (
      <BaseProviders>
        <RedirectPage />
      </BaseProviders>
    ),
    path: '/redirect',
  },
]);

export default function App() {
  return <RouterProvider router={router} />;
}

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
