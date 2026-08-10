import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import { MethodologyContextProvider } from '@admin/pages/methodology/contexts/MethodologyContext';
import MethodologyContentPage from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import MethodologySummaryEditPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryEditPage';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import getMethodologyApprovalStatusLabel from '@admin/pages/methodology/utils/getMethodologyApprovalStatusLabel';
import {
  methodologyContentRoute,
  MethodologyRouteParams,
  MethodologyRouteProps,
  methodologyStatusRoute,
  methodologySummaryEditRoute,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import useCurrentRouteTitle from '@admin/utils/useCurrentRouteTitle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';

const navRoutes: MethodologyRouteProps[] = [
  methodologySummaryRoute,
  methodologyContentRoute,
  methodologyStatusRoute,
];

const routes: MethodologyRouteProps[] = [
  {
    ...methodologySummaryRoute,
    component: MethodologySummaryPage,
  },
  {
    ...methodologyContentRoute,
    component: MethodologyContentPage,
  },
  {
    ...methodologyStatusRoute,
    component: MethodologyStatusPage,
  },
  {
    ...methodologySummaryEditRoute,
    component: MethodologySummaryEditPage,
  },
];

const MethodologyPage = ({
  match,
  location,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const {
    value: methodology,
    setState: setMethodology,
    isLoading,
  } = useAsyncHandledRetry(() => {
    return methodologyService.getMethodology(methodologyId);
  }, [methodologyId]);

  const currentRouteIndex =
    navRoutes.findIndex(
      route =>
        generatePath<MethodologyRouteParams>(route.path, {
          methodologyId,
        }) === location.pathname,
    ) || 0;

  const previousRoute =
    currentRouteIndex > 0 ? navRoutes[currentRouteIndex - 1] : undefined;

  const nextRoute =
    currentRouteIndex < navRoutes.length - 1
      ? navRoutes[currentRouteIndex + 1]
      : undefined;

  const previousSection = previousRoute
    ? {
        label: previousRoute.title,
        linkTo: generatePath<MethodologyRouteParams>(previousRoute.path, {
          methodologyId,
        }),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: generatePath<MethodologyRouteParams>(nextRoute.path, {
          methodologyId,
        }),
      }
    : undefined;

  const pageTitle = useCurrentRouteTitle(navRoutes);

  return (
    <Page wide breadcrumbs={[{ name: 'Edit methodology' }]}>
      <LoadingSpinner loading={isLoading}>
        {methodology ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <PageTitle
                  metaTitle={
                    pageTitle
                      ? `${pageTitle} - ${methodology.title}`
                      : methodology.title
                  }
                  title={methodology.title}
                  caption={
                    methodology.amendment
                      ? 'Amend methodology'
                      : 'Edit methodology'
                  }
                />
              </div>
            </div>

            <Tag>{getMethodologyApprovalStatusLabel(methodology.status)}</Tag>

            {methodology.amendment && (
              <Tag className="govuk-!-margin-left-2">Amendment</Tag>
            )}

            <NavBar
              routes={navRoutes.map(route => ({
                title: route.title,
                to: generatePath<MethodologyRouteParams>(route.path, {
                  methodologyId,
                }),
              }))}
              label="Methodology"
            />

            <MethodologyContextProvider
              methodology={methodology}
              onMethodologyChange={nextMethodology => {
                setMethodology({ value: nextMethodology });
              }}
            >
              <Switch>
                {routes.map(route => (
                  <Route
                    exact
                    key={route.path}
                    path={route.path}
                    component={route.component}
                  />
                ))}
              </Switch>
            </MethodologyContextProvider>

            <PreviousNextLinks
              previousSection={previousSection}
              nextSection={nextSection}
            />
          </>
        ) : (
          <WarningMessage>Could not load methodology</WarningMessage>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default MethodologyPage;
