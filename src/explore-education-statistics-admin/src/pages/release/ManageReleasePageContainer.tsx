import Link from '@admin/components/Link';
import NavLink from '@admin/components/NavLink';
import Page from '@admin/components/Page';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import releaseRoutes, { viewRoutes } from '@admin/routes/edit-release/routes';
import service from '@admin/services/common/service';
import { BasicPublicationDetails } from '@admin/services/common/types';
import RelatedInformation from '@common/components/RelatedInformation';
import React, { useEffect, useState } from 'react';
import { Route, RouteComponentProps } from 'react-router';
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
        route.generateLink({ publicationId, releaseId }) === location.pathname,
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
        linkTo: previousRoute.generateLink({ publicationId, releaseId }),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: nextRoute.generateLink({ publicationId, releaseId }),
      }
    : undefined;

  return (
    <>
      {publication && (
        <Page wide breadcrumbs={[{ name: 'Edit release' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <h1 className="govuk-heading-xl">
                <span className="govuk-caption-xl">Edit release</span>
                {publication.title}
              </h1>
            </div>

            <div className="govuk-grid-column-one-third">
              <RelatedInformation heading="Help and guidance">
                <ul className="govuk-list">
                  <li>
                    <Link
                      to="/documentation/create-new-release"
                      target="_blank"
                    >
                      Creating a new release
                    </Link>
                  </li>
                </ul>
              </RelatedInformation>
            </div>
          </div>

          <nav className="app-navigation govuk-!-margin-top-6 govuk-!-margin-bottom-9">
            <ul className="app-navigation__list govuk-!-margin-bottom-0">
              {viewRoutes.map(route => (
                <li key={route.path}>
                  <NavLink
                    key={route.path}
                    to={route.generateLink({ publicationId, releaseId })}
                  >
                    {route.title}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>

          <ManageReleaseContext.Provider
            value={{
              publication,
              releaseId,
            }}
          >
            {releaseRoutes.manageReleaseRoutes.map(route => (
              <Route exact key={route.path} {...route} />
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
