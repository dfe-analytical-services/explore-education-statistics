import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import RouteSwitch from '@admin/components/RouteSwitch';
import { useAuthContext } from '@admin/contexts/AuthContext';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseVersionQueries from '@admin/queries/releaseVersionQueries';
import {
  publicationReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/publicationRoutes';
import releasePageRoutes from '@admin/routes/releasePageRoutes';
import { releaseNavRoutes } from '@admin/routes/releaseRoutes';
import useNavRoutes from '@admin/hooks/useNavRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

interface MatchProps {
  publicationId: string;
  releaseVersionId: string;
}

const ReleasePageContainer = ({ match }: RouteComponentProps<MatchProps>) => {
  const { publicationId, releaseVersionId } = match.params;

  const { user } = useAuthContext();

  const {
    data: releaseVersion,
    isLoading: loadingRelease,
    refetch,
  } = useQuery(releaseVersionQueries.get(releaseVersionId));

  const navRoutes = useMemo(() => {
    return releaseNavRoutes.filter(route => {
      return (
        user?.permissions &&
        (!route.protectionAction || route.protectionAction(user.permissions))
      );
    });
  }, [user?.permissions]);

  const { navBarRoutes, currentRouteTitle, previousSection, nextSection } =
    useNavRoutes(navRoutes, { publicationId, releaseVersionId });

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
                  currentRouteTitle
                    ? `${currentRouteTitle} - ${releaseVersion.publicationTitle}`
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

          <NavBar routes={navBarRoutes} label="Release" />

          <ReleaseVersionContextProvider
            releaseVersion={releaseVersion}
            onReleaseChange={refetch}
          >
            <RouteSwitch protect routes={releasePageRoutes} />
          </ReleaseVersionContextProvider>

          <PreviousNextLinks
            previousSection={previousSection}
            nextSection={nextSection}
          />
        </Page>
      )}
    </LoadingSpinner>
  );
};

export default ReleasePageContainer;
