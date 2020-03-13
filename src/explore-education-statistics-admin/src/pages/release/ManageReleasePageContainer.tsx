import Link from '@admin/components/Link';
import NavLink from '@admin/components/NavLink';
import Page from '@admin/components/Page';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import { getReleaseStatusLabel } from '@admin/pages/release/util/releaseSummaryUtil';
import releaseRoutes, { viewRoutes } from '@admin/routes/edit-release/routes';
import service from '@admin/services/common/service';
import { BasicPublicationDetails } from '@admin/services/common/types';
import releaseService from '@admin/services/release/edit-release/summary/service';
import { ReleasePublicationStatus } from '@admin/services/release/types';
import RelatedInformation from '@common/components/RelatedInformation';
import Tag from '@common/components/Tag';
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
  const [releasePublicationStatus, setReleasePublicationStatus] = useState<
    ReleasePublicationStatus
  >();

  useEffect(() => {
    Promise.all([
      service.getBasicPublicationDetails(publicationId),
      releaseService.getReleasePublicationStatus(releaseId),
    ]).then(([publicationDetails, publicationStatus]) => {
      setPublication(publicationDetails);
      setReleasePublicationStatus(publicationStatus);
    });
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
      {publication && releasePublicationStatus && (
        <Page wide breadcrumbs={[{ name: 'Edit release' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <h1 className="govuk-heading-xl">
                <span className="govuk-caption-xl">
                  {releasePublicationStatus &&
                  releasePublicationStatus.amendment
                    ? 'Amend release'
                    : 'Edit release'}
                </span>
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

          <Tag>{getReleaseStatusLabel(releasePublicationStatus.status)}</Tag>

          {releasePublicationStatus.amendment && (
            <Tag className="govuk-!-margin-left-2">Amendment</Tag>
          )}

          {releasePublicationStatus.live && (
            <Tag className="govuk-!-margin-left-2">Live</Tag>
          )}

          <nav className="app-navigation govuk-!-margin-top-6 govuk-!-margin-bottom-9">
            <ul className="app-navigation__list govuk-!-margin-bottom-0">
              {viewRoutes.map(route => (
                <li key={route.path}>
                  <NavLink
                    key={route.path}
                    to={route.generateLink(publicationId, releaseId)}
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
              onChangeReleaseStatus: (
                updatedStatus: Partial<ReleasePublicationStatus>,
              ) =>
                setReleasePublicationStatus({
                  ...releasePublicationStatus,
                  ...updatedStatus,
                }),
            }}
          >
            {releaseRoutes.manageReleaseRoutes.map(route => (
              <Route
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
