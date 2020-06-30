import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import methodologyRoutes, {
  MethodologyRouteParams,
  navRoutes,
} from '@admin/routes/edit-methodology/routes';
import methodologyService from '@admin/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { Route, RouteComponentProps, Switch } from 'react-router';

const MethodologyPage = ({
  match,
  location,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { value, isLoading } = useAsyncHandledRetry(
    () => methodologyService.getMethodology(methodologyId),
    [methodologyId],
  );

  const currentRouteIndex =
    methodologyRoutes.findIndex(
      route => route.generateLink(methodologyId) === location.pathname,
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
        linkTo: previousRoute.generateLink(methodologyId),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: nextRoute.generateLink(methodologyId),
      }
    : undefined;

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Manage methodologies', link: '/methodologies' },
        { name: 'Edit methodology' },
      ]}
    >
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
                path: route.path,
                title: route.title,
                to: route.generateLink(methodologyId),
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
