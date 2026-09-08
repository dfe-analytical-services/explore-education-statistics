import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import RouteSwitch from '@admin/components/RouteSwitch';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import publicationPageRoutes from '@admin/routes/publicationPageRoutes';
import {
  publicationContactRoute,
  publicationDetailsRoute,
  publicationNavRoutes,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  PublicationWithPermissions,
} from '@admin/services/publicationService';
import useNavRoutes from '@admin/hooks/useNavRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useMemo } from 'react';
import { RouteComponentProps } from 'react-router';

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

  const navRoutes = useMemo(() => {
    return publicationNavRoutes.filter(route => {
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
  }, [publication?.permissions]);

  const { navBarRoutes, currentRouteTitle } = useNavRoutes(navRoutes, {
    publicationId,
  });

  return (
    <LoadingSpinner loading={loadingPublication}>
      {publication ? (
        <Page wide breadcrumbs={[{ name: 'Manage publication' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <PageTitle
                metaTitle={
                  currentRouteTitle
                    ? `${currentRouteTitle} - ${publication.title}`
                    : publication.title
                }
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
                      <VisuallyHidden> about general enquiries</VisuallyHidden>
                    </Link>
                  </li>
                </ul>
              </RelatedInformation>
            </div>
          </div>

          <NavBar routes={navBarRoutes} label="Publication" />

          <PublicationContextProvider
            publication={publication}
            onPublicationChange={nextPublication => {
              setPublication({ value: nextPublication });
            }}
            onReload={reloadPublication}
          >
            <RouteSwitch routes={publicationPageRoutes} />
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
