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
import React, { DependencyList, ReactNode, useEffect } from 'react';
import { Location, Outlet, RouterProvider, useLocation } from 'react-router';
import { createBrowserRouter } from 'react-router-dom';
import { LastLocationContextProvider } from './contexts/LastLocationContext';
import PageNotFoundPage from './pages/errors/PageNotFoundPage';

import 'ckeditor5/ckeditor5.css';
import { NotificationHubContextProvider } from './contexts/NotificationHubContext';

const queryClient = new QueryClient();

const head = createHead();

const useLocationEffect = (
  callback: (location: Location) => void,
  deps: DependencyList = [],
) => {
  const location = useLocation();

  callback(location);

  useEffect(() => {
    callback(location);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [callback, location, ...deps]);
};

function ApplicationInsightsTracking() {
  const appInsights = useApplicationInsights();

  useEffect(() => {
    document.body.classList.add('js-enabled', 'govuk-frontend-supported');
  }, []);

  useLocationEffect(
    location => {
      if (appInsights) {
        appInsights.trackPageView({
          uri: location.pathname,
        });
      }
    },
    [appInsights],
  );

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

const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      ...Object.entries(publicRoutes).map(([key, route]) => ({
        id: key,
        ...route,
      })),

      ...Object.entries(routes).map(
        ([key, { element, protectionAction, ...route }]) => ({
          id: key,
          element: (
            <ProtectedRoute protectionAction={protectionAction}>
              {element}
            </ProtectedRoute>
          ),
          ...route,
        }),
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
