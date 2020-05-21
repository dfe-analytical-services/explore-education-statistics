import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import methodologyRoutes, {
  MethodologyRouteParams,
  navRoutes,
} from '@admin/routes/edit-methodology/routes';
import methodologyService from '@admin/services/methodology/methodologyService';
import permissionService from '@admin/services/permissions/permissionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { Route, RouteComponentProps, Switch } from 'react-router';
import {
  MethodologyContextState,
  MethodologyProvider,
} from './content/context/MethodologyContext';

const MethodologyPage = ({
  match,
  location,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { value, isLoading } = useAsyncRetry<
    MethodologyContextState
  >(async () => {
    const methodology = await methodologyService.getMethodologyContent(
      methodologyId,
    );
    const canUpdateMethodology = await permissionService.canUpdateMethodology(
      methodologyId,
    );

    return {
      methodology,
      canUpdateMethodology,
    };
  }, [methodologyId]);

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
                <h1 className="govuk-heading-xl">
                  <span className="govuk-caption-xl">Edit methodology</span>
                  {value?.methodology.title}
                </h1>
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

            <MethodologyProvider value={value}>
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
            </MethodologyProvider>

            <PreviousNextLinks
              previousSection={previousSection}
              nextSection={nextSection}
            />
          </>
        ) : (
          <p>Could not load methodology</p>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default MethodologyPage;
