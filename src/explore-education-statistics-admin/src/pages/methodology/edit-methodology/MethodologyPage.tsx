import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import {
  methodologyContentRoute,
  MethodologyRouteParams,
  MethodologyRouteProps,
  methodologyStatusRoute,
  methodologySummaryEditRoute,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';

const methodologyRoutes: MethodologyRouteProps[] = [
  methodologySummaryRoute,
  methodologySummaryEditRoute,
  methodologyContentRoute,
  methodologyStatusRoute,
];

const navRoutes: MethodologyRouteProps[] = [
  methodologySummaryRoute,
  methodologyContentRoute,
  methodologyStatusRoute,
];

const MethodologyPage = ({
  match,
  location,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId, publicationId } = match.params;

  const { value, isLoading } = useAsyncHandledRetry(
    () => methodologyService.getMethodology(methodologyId),
    [methodologyId],
  );

  const currentRouteIndex =
    methodologyRoutes.findIndex(
      route =>
        generatePath<MethodologyRouteParams>(route.path, {
          publicationId,
          methodologyId,
        }) === location.pathname,
    ) || 0;

  const previousRoute =
    currentRouteIndex > 0
      ? methodologyRoutes[currentRouteIndex - 1]
      : undefined;

  const nextRoute =
    currentRouteIndex < methodologyRoutes.length - 1
      ? methodologyRoutes[currentRouteIndex + 1]
      : undefined;

  const previousSection = previousRoute
    ? {
        label: previousRoute.title,
        linkTo: generatePath<MethodologyRouteParams>(previousRoute.path, {
          publicationId,
          methodologyId,
        }),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: generatePath<MethodologyRouteParams>(nextRoute.path, {
          publicationId,
          methodologyId,
        }),
      }
    : undefined;

  return (
    <Page wide breadcrumbs={[{ name: 'Edit methodology' }]}>
      <LoadingSpinner loading={isLoading}>
        {value ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <PageTitle title={value.title} caption="Edit methodology" />
              </div>

              <div className="govuk-grid-column-one-third">
                <RelatedInformation heading="Help and guidance">
                  <ul className="govuk-list">
                    <li>
                      <Link to="/documentation" target="blank">
                        Creating new methodology
                      </Link>
                    </li>
                  </ul>
                </RelatedInformation>
              </div>
            </div>

            <NavBar
              routes={navRoutes.map(route => ({
                title: route.title,
                to: generatePath<MethodologyRouteParams>(route.path, {
                  publicationId,
                  methodologyId,
                }),
              }))}
            />

            <Switch>
              {methodologyRoutes.map(route => (
                <Route
                  exact
                  key={route.path}
                  path={route.path}
                  component={route.component}
                />
              ))}
            </Switch>

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
