import Link from '@admin/components/Link';
import NavLink from '@admin/components/NavLink';
import Page from '@admin/components/Page';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import methodologyRoutes from '@admin/routes/edit-methodology/routes';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import RelatedInformation from '@common/components/RelatedInformation';
import React, { useEffect, useState } from 'react';
import { Route, RouteComponentProps, Switch } from 'react-router';
import { Methodology } from '@common/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';

const MethodologyPage = ({
  match,
  location,
}: RouteComponentProps<{ methodologyId: string }> & ErrorControlProps) => {
  const { methodologyId } = match.params;

  const [methodology, setMethodology] = useState<Methodology>();

  const refreshMethodology = () => {};

  useEffect(refreshMethodology, [methodologyId]);

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
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">Edit methodology</span>
            [[methodology title]]
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

      {methodology ? (
        <>
          <nav className="app-navigation govuk-!-margin-top-6 govuk-!-margin-bottom-9">
            <ul className="app-navigation__list govuk-!-margin-bottom-0">
              {methodologyRoutes.map(route => (
                <li key={route.path}>
                  <NavLink
                    key={route.path}
                    to={route.generateLink(methodologyId)}
                  >
                    {route.title}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>

          <Switch>
            {methodologyRoutes.map(route => (
              <Route
                exact
                key={route.path}
                path={route.path}
                render={() =>
                  route.component({ methodology, refreshMethodology })
                }
              />
            ))}
          </Switch>

          <PreviousNextLinks
            previousSection={previousSection}
            nextSection={nextSection}
          />
        </>
      ) : (
        <LoadingSpinner />
      )}
    </Page>
  );
};

export default withErrorControl(MethodologyPage);
