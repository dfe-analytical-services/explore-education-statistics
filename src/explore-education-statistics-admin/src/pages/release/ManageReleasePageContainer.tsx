import NavLink from '@admin/components/NavLink';
import Page from '@admin/components/Page';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import releaseRoutes, { viewRoutes } from '@admin/routes/edit-release/routes';
import service from '@admin/services/common/service';
import { BasicPublicationDetails } from '@admin/services/common/types';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import ManageReleaseContext from './ManageReleaseContext';

interface MatchProps {
  publicationId: string;
  releaseId: string;
}

const ManageReleasePageContainer = ({
  match,
  location,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId, releaseId } = match.params;

  const [publication, setPublication] = useState<BasicPublicationDetails>();

  useEffect(() => {
    service.getBasicPublicationDetails(publicationId).then(setPublication);
  }, [publicationId, releaseId]);

  const currentRouteIndex =
    viewRoutes.findIndex(
      route =>
        route.generateLink(publicationId, releaseId) === location.pathname,
    ) || 0;

  const previousRoute =
    currentRouteIndex > 0 ? viewRoutes[currentRouteIndex - 1] : undefined;

  const nextRoute =
    currentRouteIndex < viewRoutes.length - 1
      ? viewRoutes[currentRouteIndex + 1]
      : undefined;

  const previousSection = previousRoute
    ? {
        label: previousRoute.title,
        linkTo: previousRoute.generateLink(publicationId, releaseId),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: nextRoute.generateLink(publicationId, releaseId),
      }
    : undefined;

  return (
    <>
      {publication && (
        <Page wide breadcrumbs={[{ name: 'Edit release' }]}>
          <h1 className="govuk-heading-l">
            {publication.title}
            <span className="govuk-caption-l">Edit release</span>
          </h1>
          <nav className="app-navigation govuk-!-margin-bottom-9">
            <ul className="app-navigation__list govuk-!-margin-bottom-0">
              <li>
                {viewRoutes.map(route => (
                  <NavLink
                    key={route.path}
                    to={route.generateLink(publicationId, releaseId)}
                    activeClassName="app-navigation--current-page"
                  >
                    {route.title}
                  </NavLink>
                ))}
              </li>
            </ul>
          </nav>

          <ManageReleaseContext.Provider value={{ publication, releaseId }}>
            {releaseRoutes.manageReleaseRoutes.map(route => (
              <ProtectedRoute
                exact
                key={route.path}
                path={route.path}
                component={route.component}
              />
            ))}
          </ManageReleaseContext.Provider>

          <PreviousNextLinks
            previousSection={previousSection}
            nextSection={nextSection}
          />
        </Page>
      )}
    </>
  );
};

export default ManageReleasePageContainer;
