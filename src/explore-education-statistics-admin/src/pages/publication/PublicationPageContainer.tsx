import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationContactRoute,
  publicationManageReleaseContributorsPageRoute,
  publicationDetailsRoute,
  publicationAdoptMethodologyRoute,
  publicationExternalMethodologyRoute,
  publicationMethodologiesRoute,
  publicationInviteUsersPageRoute,
  publicationTeamAccessRoute,
  publicationReleasesRoute,
  publicationReleaseSeriesRoute,
  publicationCreateReleaseLegacyLinkRoute,
  publicationEditReleaseSeriesLegacyLinkRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';

const navRoutes = [
  publicationReleasesRoute,
  publicationMethodologiesRoute,
  publicationDetailsRoute,
  publicationContactRoute,
  publicationTeamAccessRoute,
  publicationReleaseSeriesRoute,
];

const routes = [
  ...navRoutes,
  publicationAdoptMethodologyRoute,
  publicationExternalMethodologyRoute,
  publicationCreateReleaseLegacyLinkRoute,
  publicationEditReleaseSeriesLegacyLinkRoute,
  publicationManageReleaseContributorsPageRoute,
  publicationInviteUsersPageRoute,
];

const PublicationPageContainer = ({
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;
  const {
    value: publication,
    setState: setPublication,
    isLoading: loadingPublication,
    retry: reloadPublication,
  } = useAsyncHandledRetry(() =>
    publicationService.getPublication<PublicationWithPermissions>(
      publicationId,
      true,
    ),
  );

  const getNavRoutes = () => {
    return navRoutes.filter(route => {
      switch (route) {
        case publicationDetailsRoute:
          return (
            publication?.permissions?.canUpdatePublication ||
            publication?.permissions?.canUpdatePublicationSummary
          );
        case publicationContactRoute:
          return publication?.permissions?.canUpdateContact;
        default:
          return true;
      }
    });
  };

  return (
    <LoadingSpinner loading={loadingPublication}>
      {publication ? (
        <Page wide breadcrumbs={[{ name: 'Manage publication' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <PageTitle
                title={publication.title}
                caption="Manage publication"
              />
              {publication.isSuperseded && (
                <WarningMessage className="govuk-!-margin-bottom-0">
                  This publication is archived.
                </WarningMessage>
              )}
              {publication.supersededById && !publication.isSuperseded && (
                <WarningMessage className="govuk-!-margin-bottom-0">
                  This publication will be archived when its superseding
                  publication has a live release published.
                </WarningMessage>
              )}
            </div>

            <div className="govuk-grid-column-one-third">
              <RelatedInformation heading="Help and guidance">
                <ul className="govuk-list">
                  <li>
                    <Link to="/contact-us" target="_blank">
                      Contact us
                    </Link>
                  </li>
                </ul>
              </RelatedInformation>
            </div>
          </div>

          <NavBar
            routes={getNavRoutes().map(route => ({
              title: route.title,
              to: generatePath<PublicationRouteParams>(route.path, {
                publicationId,
              }),
            }))}
            label="Publication"
          />

          <PublicationContextProvider
            publication={publication}
            onPublicationChange={nextPublication => {
              setPublication({ value: nextPublication });
            }}
            onReload={reloadPublication}
          >
            <Switch>
              {routes.map(route => (
                <Route exact key={route.path} {...route} />
              ))}
            </Switch>
          </PublicationContextProvider>
        </Page>
      ) : (
        <WarningMessage>
          There was a problem loading this publication.
        </WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default PublicationPageContainer;
