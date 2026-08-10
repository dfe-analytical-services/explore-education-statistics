import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import ProtectedRoute, {
  ProtectedRouteProps,
} from '@admin/components/ProtectedRoute';
import { useAuthContext } from '@admin/contexts/AuthContext';
import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import ReleaseApiDataSetChangelogPage from '@admin/pages/release/data/ReleaseApiDataSetChangelogPage';
import ReleaseApiDataSetDetailsPage from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import ReleaseApiDataSetFiltersMappingPage from '@admin/pages/release/data/ReleaseApiDataSetFiltersMappingPage';
import ReleaseApiDataSetIndicatorsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetIndicatorsMappingPage';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import ReleaseApiDataSetPreviewPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewPage';
import ReleaseApiDataSetPreviewTokenLogPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenLogPage';
import ReleaseApiDataSetPreviewTokenPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenPage';
import ReleaseApiDataSetVersionHistoryPage from '@admin/pages/release/data/ReleaseApiDataSetVersionHistoryPage';
import ReleaseDataFileReplacePage from '@admin/pages/release/data/ReleaseDataFileReplacePage';
import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import ReleaseDataPage from '@admin/pages/release/data/ReleaseDataPage';
import ReleaseDataBlockCreatePage from '@admin/pages/release/datablocks/ReleaseDataBlockCreatePage';
import ReleaseDataBlockEditPage from '@admin/pages/release/datablocks/ReleaseDataBlockEditPage';
import ReleaseDataBlocksPage from '@admin/pages/release/datablocks/ReleaseDataBlocksPage';
import ReleaseTableToolPage from '@admin/pages/release/datablocks/ReleaseTableToolPage';
import ReleaseFootnoteCreatePage from '@admin/pages/release/footnotes/ReleaseFootnoteCreatePage';
import ReleaseFootnoteEditPage from '@admin/pages/release/footnotes/ReleaseFootnoteEditPage';
import ReleaseFootnotesPage from '@admin/pages/release/footnotes/ReleaseFootnotesPage';
import ReleasePreReleaseAccessPage from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';
import ReleasePublishChecklistPage from '@admin/pages/release/ReleaseChecklistPage';
import ReleasePublishStatusPage from '@admin/pages/release/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/ReleaseSummaryPage';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseVersionQueries from '@admin/queries/releaseVersionQueries';
import { publicationReleasesRoute } from '@admin/routes/publicationRoutes';
import {
  releaseAncillaryFileRoute,
  releaseAncillaryFilesRoute,
  releaseApiDataSetChangelogRoute,
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetFiltersMappingRoute,
  releaseApiDataSetIndicatorsMappingRoute,
  releaseApiDataSetLocationsMappingRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenLogRoute,
  releaseApiDataSetPreviewTokenRoute,
  releaseApiDataSetVersionHistoryRoute,
  releaseApiDataSetsRoute,
  releaseChecklistRoute,
  releaseContentRoute,
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  releaseDataBlocksRoute,
  releaseDataFileReplaceRoute,
  releaseDataFileReplacementCompleteRoute,
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
import { PublicationRouteParams } from '@admin/routes/routes';
import useCurrentRouteTitle from '@admin/utils/useCurrentRouteTitle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';
import { generatePath, RouteComponentProps, Switch } from 'react-router';

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

const routes: ProtectedRouteProps[] = [
  {
    ...releaseSummaryRoute,
    component: ReleaseSummaryPage,
  },
  {
    ...releaseDataRoute,
    component: ReleaseDataPage,
  },
  {
    ...releaseFootnotesRoute,
    component: ReleaseFootnotesPage,
  },
  {
    ...releaseDataBlocksRoute,
    component: ReleaseDataBlocksPage,
  },
  {
    ...releaseContentRoute,
    component: ReleaseContentPage,
  },
  {
    ...releaseChecklistRoute,
    component: ReleasePublishChecklistPage,
  },
  {
    ...releaseStatusRoute,
    component: ReleasePublishStatusPage,
  },
  {
    ...releasePreReleaseAccessRoute,
    component: ReleasePreReleaseAccessPage,
  },
  {
    ...releaseAncillaryFilesRoute,
    component: ReleaseDataPage,
  },
  {
    ...releaseAncillaryFileRoute,
    component: ReleaseAncillaryFilePage,
  },
  {
    ...releaseDataFileReplaceRoute,
    component: ReleaseDataFileReplacePage,
  },
  {
    ...releaseDataFileReplacementCompleteRoute,
    component: ReleaseDataFileReplacementCompletePage,
  },
  {
    ...releaseApiDataSetsRoute,
    component: ReleaseDataPage,
  },
  {
    ...releaseApiDataSetDetailsRoute,
    component: ReleaseApiDataSetDetailsPage,
  },
  {
    ...releaseApiDataSetFiltersMappingRoute,
    component: ReleaseApiDataSetFiltersMappingPage,
  },
  {
    ...releaseApiDataSetLocationsMappingRoute,
    component: ReleaseApiDataSetLocationsMappingPage,
  },
  {
    ...releaseApiDataSetIndicatorsMappingRoute,
    component: ReleaseApiDataSetIndicatorsMappingPage,
  },
  {
    ...releaseApiDataSetPreviewRoute,
    component: ReleaseApiDataSetPreviewPage,
  },
  {
    ...releaseApiDataSetPreviewTokenRoute,
    component: ReleaseApiDataSetPreviewTokenPage,
  },
  {
    ...releaseApiDataSetPreviewTokenLogRoute,
    component: ReleaseApiDataSetPreviewTokenLogPage,
  },
  {
    ...releaseApiDataSetVersionHistoryRoute,
    component: ReleaseApiDataSetVersionHistoryPage,
  },
  {
    ...releaseApiDataSetChangelogRoute,
    component: ReleaseApiDataSetChangelogPage,
  },
  {
    ...releaseSummaryEditRoute,
    component: ReleaseSummaryEditPage,
  },
  {
    ...releaseFootnotesCreateRoute,
    component: ReleaseFootnoteCreatePage,
  },
  {
    ...releaseFootnotesEditRoute,
    component: ReleaseFootnoteEditPage,
  },
  {
    ...releaseTableToolRoute,
    component: ReleaseTableToolPage,
  },
  {
    ...releaseDataBlockCreateRoute,
    component: ReleaseDataBlockCreatePage,
  },
  {
    ...releaseDataBlockEditRoute,
    component: ReleaseDataBlockEditPage,
  },
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
  } = useQuery(releaseVersionQueries.get(releaseVersionId));

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
