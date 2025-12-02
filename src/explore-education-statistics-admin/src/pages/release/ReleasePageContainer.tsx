import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import ProtectedRoute from '@admin/components/ProtectedRoute';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
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
  releaseApiDataSetVersionHistoryRoute,
  releaseApiDataSetChangelogRoute,
  releaseChecklistRoute,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';
import { generatePath, RouteComponentProps, Switch } from 'react-router';
import { publicationReleasesRoute } from '@admin/routes/publicationRoutes';
import { PublicationRouteParams } from '@admin/routes/routes';
import useCurrentRouteTitle from '@admin/utils/useCurrentRouteTitle';

const allNavRoutes = [
  releaseSummaryRoute,
  releaseDataRoute,
  releaseFootnotesRoute,
  releaseDataBlocksRoute,
  releaseContentRoute,
  releaseChecklistRoute,
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
  releaseApiDataSetVersionHistoryRoute,
  releaseApiDataSetChangelogRoute,
  releaseSummaryEditRoute,
  releaseFootnotesCreateRoute,
  releaseFootnotesEditRoute,
  releaseTableToolRoute,
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
];

interface MatchProps {
  publicationId: string;
  releaseVersionId: string;
}

const ReleasePageContainer = ({
  match,
  location,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId, releaseVersionId } = match.params;

  const { user } = useAuthContext();

  const {
    data: releaseVersion,
    isLoading: loadingRelease,
    refetch,
  } = useQuery(releaseQueries.get(releaseVersionId));

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
          releaseVersionId,
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
          releaseVersionId,
        }),
      }
    : undefined;

  const nextSection = nextRoute
    ? {
        label: nextRoute.title,
        linkTo: generatePath<ReleaseRouteParams>(nextRoute.path, {
          publicationId,
          releaseVersionId,
        }),
      }
    : undefined;

  const pageTitle = useCurrentRouteTitle(navRoutes);

  return (
    <LoadingSpinner loading={loadingRelease}>
      {releaseVersion && (
        <Page
          wide
          breadcrumbs={[
            {
              name: 'Publication',
              link: `${generatePath<PublicationRouteParams>(
                publicationReleasesRoute.path,
                { publicationId: releaseVersion.publicationId },
              )}`,
            },
            { name: 'Edit release' },
          ]}
        >
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <PageTitle
                metaTitle={
                  pageTitle
                    ? `${pageTitle} - ${releaseVersion.publicationTitle}`
                    : releaseVersion.publicationTitle
                }
                title={releaseVersion.publicationTitle}
                caption={`${
                  releaseVersion.amendment ? 'Amend release' : 'Edit release'
                } for ${releaseVersion.title}`}
              />
            </div>
          </div>

          <Tag>
            {getReleaseApprovalStatusLabel(releaseVersion.approvalStatus)}
          </Tag>
          {releaseVersion.amendment && (
            <Tag className="govuk-!-margin-left-2">Amendment</Tag>
          )}
          {releaseVersion.live && (
            <Tag className="govuk-!-margin-left-2">Live</Tag>
          )}

          <NavBar
            routes={navRoutes.map(route => ({
              title: route.title,
              to: generatePath<ReleaseRouteParams>(route.path, {
                publicationId,
                releaseVersionId,
              }),
            }))}
            label="Release"
          />

          <ReleaseVersionContextProvider
            releaseVersion={releaseVersion}
            onReleaseChange={refetch}
          >
            <Switch>
              {routes.map(route => (
                <ProtectedRoute exact key={route.path} {...route} />
              ))}
            </Switch>
          </ReleaseVersionContextProvider>

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
