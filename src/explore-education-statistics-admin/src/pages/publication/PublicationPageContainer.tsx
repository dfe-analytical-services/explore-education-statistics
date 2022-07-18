import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { PublicationContextProvider } from '@admin/pages/publication/contexts/PublicationContext';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';

const navRoutes = [publicationReleasesRoute];

const routes = [...navRoutes];

interface MatchProps {
  publicationId: string;
}

const PublicationPageContainer = ({
  match,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId } = match.params;
  const {
    value: publication,
    setState: setPublication,
    isLoading: loadingPublication,
    retry: reloadPublication,
  } = useAsyncHandledRetry(() =>
    publicationService.getMyPublication(publicationId),
  );

  return (
    <LoadingSpinner loading={loadingPublication}>
      {publication && (
        <Page wide breadcrumbs={[{ name: 'Manage publication' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <PageTitle
                title={publication.title}
                caption="Manage publication"
              />
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
            routes={navRoutes.map(route => ({
              title: route.title,
              to: generatePath<PublicationRouteParams>(route.path, {
                publicationId,
              }),
            }))}
            label="Release"
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
      )}
    </LoadingSpinner>
  );
};

export default PublicationPageContainer;
