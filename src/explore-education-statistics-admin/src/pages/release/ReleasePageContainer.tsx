import Link from '@admin/components/Link';
import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import { getReleaseStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import {
  releaseContentRoute,
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  releaseDataBlocksRoute,
  releaseDataFileReplacementCompleteRoute,
  releaseDataFileRoute,
  releaseDataFileReplaceRoute,
  releaseDataRoute,
  releaseFootnotesCreateRoute,
  releaseFootnotesEditRoute,
  releaseFootnotesRoute,
  releasePreReleaseAccessRoute,
  ReleaseRouteParams,
  releaseStatusRoute,
  releaseSummaryEditRoute,
  releaseSummaryRoute,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import releaseService from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import RelatedInformation from '@common/components/RelatedInformation';
import Tag from '@common/components/Tag';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';

const navRoutes = [
  releaseSummaryRoute,
  releaseDataRoute,
  releaseFootnotesRoute,
  releaseDataBlocksRoute,
  releaseContentRoute,
  releaseStatusRoute,
  releasePreReleaseAccessRoute,
];

const routes = [
  ...navRoutes,
  releaseDataFileRoute,
  releaseDataFileReplaceRoute,
  releaseDataFileReplacementCompleteRoute,
  releaseSummaryEditRoute,
  releaseFootnotesCreateRoute,
  releaseFootnotesEditRoute,
  releaseTableToolRoute,
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
];

interface MatchProps {
  publicationId: string;
  releaseId: string;
}

const ReleasePageContainer = ({
  match,
  location,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId, releaseId } = match.params;
  const {
    value: release,
    setState: setRelease,
    isLoading: loadingRelease,
  } = useAsyncHandledRetry(() => releaseService.getRelease(releaseId), [
    releaseId,
  ]);

  const currentRouteIndex =
    navRoutes.findIndex(
      route =>
        generatePath<ReleaseRouteParams>(route.path, {
          publicationId,
          releaseId,
        }) === location.pathname,
    ) || 0;

  const previousRoute =
    currentRouteIndex > 0 ? navRoutes[currentRouteIndex - 1] : undefined;

  const nextRoute =
    currentRouteIndex < navRoutes.length - 1
      ? navRoutes[currentRouteIndex + 1]
      : undefined;

  const previousSection = previousRoute
    ? {
        label: previousRoute.title,
        linkTo: generatePath<ReleaseRouteParams>(previousRoute.path, {
          publicationId,
          releaseId,
        }),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: generatePath<ReleaseRouteParams>(nextRoute.path, {
          publicationId,
          releaseId,
        }),
      }
    : undefined;

  return (
    <LoadingSpinner loading={loadingRelease}>
      {release && (
        <Page wide breadcrumbs={[{ name: 'Edit release' }]}>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <PageTitle
                title={release.publicationTitle}
                caption={`${
                  release.amendment ? 'Amend release' : 'Edit release'
                } for ${release.title}`}
              />
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

          <Tag>{getReleaseStatusLabel(release.status)}</Tag>

          {release.amendment && (
            <Tag className="govuk-!-margin-left-2">Amendment</Tag>
          )}

          {release.live && <Tag className="govuk-!-margin-left-2">Live</Tag>}

          <NavBar
            routes={navRoutes.map(route => ({
              title: route.title,
              to: generatePath<ReleaseRouteParams>(route.path, {
                publicationId,
                releaseId,
              }),
            }))}
          />

          <ReleaseContextProvider
            release={release}
            onReleaseChange={nextRelease => {
              setRelease({ value: nextRelease });
            }}
          >
            <Switch>
              {routes.map(route => (
                <Route exact key={route.path} {...route} />
              ))}
            </Switch>
          </ReleaseContextProvider>

          {currentRouteIndex > -1 && (
            <PreviousNextLinks
              previousSection={previousSection}
              nextSection={nextSection}
            />
          )}
        </Page>
      )}
    </LoadingSpinner>
  );
};

export default ReleasePageContainer;
