import Page from '@admin/components/Page';
import PublicationReleaseContributorsForm from '@admin/pages/publication/components/PublicationReleaseContributorsForm';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { publicationManageTeamAccessReleaseRoute } from '@admin/routes/routes';
import releasePermissionService, {
  ContributorViewModel,
} from '@admin/services/releasePermissionService';
import publicationService, {
  Publication,
} from '@admin/services/publicationService';
import releaseService, { Release } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

interface Model {
  publication: Publication;
  release: Release;
  publicationContributors: ContributorViewModel[];
  releaseContributors: ContributorViewModel[];
}

const PublicationReleaseContributorsPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [
      publication,
      release,
      publicationContributors,
      releaseContributors,
    ] = await Promise.all([
      publicationService.getPublication(publicationId),
      releaseService.getRelease(releaseId),
      releasePermissionService.listPublicationContributors(publicationId),
      releasePermissionService.listReleaseContributors(releaseId),
    ]);
    return {
      publication,
      release,
      publicationContributors,
      releaseContributors,
    };
  }, [publicationId, releaseId]);

  if (!model || isLoading) {
    return <LoadingSpinner />;
  }

  const {
    publication,
    release,
    publicationContributors,
    releaseContributors,
  } = model;

  return (
    <LoadingSpinner
      loading={!publicationContributors && !releaseContributors && isLoading}
    >
      <Page
        title={`Manage release contributors (${release.title})`}
        caption={publication.title}
        breadcrumbs={[
          {
            name: 'Manage team access',
            link: `/publication/${publicationId}/manage-team/${releaseId}`,
          },
          { name: 'Add or remove existing users' },
        ]}
      >
        <PublicationReleaseContributorsForm
          publicationId={publicationId}
          releaseId={releaseId}
          publicationContributors={publicationContributors}
          releaseContributors={releaseContributors}
          returnRoute={generatePath<ReleaseRouteParams>(
            publicationManageTeamAccessReleaseRoute.path,
            {
              publicationId,
              releaseId,
            },
          )}
        />
      </Page>
    </LoadingSpinner>
  );
};

export default PublicationReleaseContributorsPage;
