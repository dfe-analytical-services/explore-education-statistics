import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseQueries from '@admin/queries/releaseQueries';
import {
  releaseContentRoute,
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  releaseDataBlocksRoute,
  releaseDataFileReplacementCompleteRoute,
  releaseDataRoute,
  releaseAncillaryFilesRoute,
  releaseAncillaryFileRoute,
  releaseDataFileRoute,
  releaseDataFileReplaceRoute,
  releaseFootnotesCreateRoute,
  releaseFootnotesEditRoute,
  releaseFootnotesRoute,
  releasePreReleaseAccessRoute,
  ReleaseRouteParams,
  releaseStatusRoute,
  releaseSummaryEditRoute,
  releaseSummaryRoute,
  releaseTableToolRoute,
  releaseApiDataSetsRoute,
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetFiltersMappingRoute,
  releaseApiDataSetLocationsMappingRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenRoute,
  releaseApiDataSetPreviewTokenLogRoute,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';
import { generatePath, RouteComponentProps, Switch } from 'react-router';
import { publicationReleasesRoute } from '@admin/routes/publicationRoutes';
import { PublicationRouteParams } from '@admin/routes/routes';

const allNavRoutes = [
  releaseSummaryRoute,
  releaseDataRoute,
  releaseFootnotesRoute,
  releaseDataBlocksRoute,
  releaseContentRoute,
  releaseStatusRoute,
  releasePreReleaseAccessRoute,
];

const routes = [
  ...allNavRoutes,
  releaseAncillaryFilesRoute,
  releaseAncillaryFileRoute,
  releaseDataFileRoute,
  releaseDataFileReplaceRoute,
  releaseDataFileReplacementCompleteRoute,
  releaseApiDataSetsRoute,
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetFiltersMappingRoute,
  releaseApiDataSetLocationsMappingRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenRoute,
  releaseApiDataSetPreviewTokenLogRoute,
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

  const { user } = useAuthContext();

  const {
    data: release,
    isLoading: loadingRelease,
    refetch,
  } = useQuery(releaseQueries.get(releaseId));

  const navRoutes = useMemo(() => {
    return allNavRoutes.filter(route => {
      return (
        user?.permissions &&
        (!route.protectionAction || route.protectionAction(user.permissions))
      );
    });
  }, [user?.permissions]);

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
        <Page
          wide
          breadcrumbs={[
            {
              name: 'Publication',
              link: `${generatePath<PublicationRouteParams>(
                publicationReleasesRoute.path,
                { publicationId: release.publicationId },
              )}`,
            },
            { name: 'Edit release' },
          ]}
        >
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <PageTitle
                title={release.publicationTitle}
                caption={`${
                  release.amendment ? 'Amend release' : 'Edit release'
                } for ${release.title}`}
              />
            </div>
          </div>

          <Tag>{getReleaseApprovalStatusLabel(release.approvalStatus)}</Tag>
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
            label="Release"
          />

          <ReleaseContextProvider release={release} onReleaseChange={refetch}>
            <Switch>
              {routes.map(route => (
                <ProtectedRoute exact key={route.path} {...route} />
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
